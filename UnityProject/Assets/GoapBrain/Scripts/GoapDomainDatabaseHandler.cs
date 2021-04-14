using System;
using System.Collections.Generic;

using Common;

using CommonEcs.Goap;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace GoapBrain {
    /// <summary>
    /// Create the GoapDomainDatabase blob asset from the specified list of GoapDomainData.
    /// </summary>
    public class GoapDomainDatabaseHandler : MonoBehaviour {
        [SerializeField]
        private GoapDomainData[] domains;

        private AssemblerSet[] assemblerSets;
        
        private BlobAssetReference<GoapDomainDatabase> domainDbReference;

        private void Awake() {
            Assertion.NotNull(this.domains);
            Assertion.IsTrue(this.domains.Length > 0);
            
            BlobBuilder builder = new BlobBuilder(Allocator.Temp);

            // Prepare DomainDatabase
            ref GoapDomainDatabase db = ref builder.ConstructRoot<GoapDomainDatabase>();
            BlobBuilderArray<GoapDomain> domainsBuilder = builder.Allocate(ref db.domains, this.domains.Length);

            this.assemblerSets = new AssemblerSet[this.domains.Length];

            for (int i = 0; i < this.domains.Length; ++i) {
                GoapDomainData domainData = this.domains[i];
                domainsBuilder[i] = ParseDomain(domainData); // We use the index used here when creating the agent
                this.assemblerSets[i] = ParseAssemblerSet(domainData, i);
            }

            this.domainDbReference = builder.CreateBlobAssetReference<GoapDomainDatabase>(Allocator.Temp);
        }

        private static GoapDomain ParseDomain(GoapDomainData data) {
            GoapDomain domain = new GoapDomain();

            for (int i = 0; i < data.ActionCount; ++i) {
                GoapActionData actionData = data.GetActionAt(i);
                ConditionData? effectData = actionData.Effect;
                Assertion.NotNull(effectData);

                if (effectData == null) {
                    throw new Exception($"Action {actionData.Name} does not have an effect.");
                }

                Condition effect = new Condition(effectData.Name, effectData.Value);
                GoapAction action = new GoapAction(actionData.Name, actionData.Cost, actionData.AtomActions.Count, 
                    effect);
                    
                AddPreconditions(ref action, actionData);
                Assertion.IsTrue(action.preconditions.Count == actionData.Preconditions.Count);
                    
                domain.AddAction(action);
            }

            return domain;
        }
        
        public ref BlobAssetReference<GoapDomainDatabase> DbReference {
            get {
                return ref this.domainDbReference;
            }
        }

        private static void AddPreconditions(ref GoapAction action, GoapActionData data) {
            for (int i = 0; i < data.Preconditions.Count; ++i) {
                ConditionData preconditionData = data.Preconditions[i];
                CommonEcs.Goap.Condition unmanagedPrecondition =
                    new CommonEcs.Goap.Condition(preconditionData.Name, preconditionData.Value);
                action.AddPrecondition(unmanagedPrecondition);
            }
        }

        private static AssemblerSet ParseAssemblerSet(GoapDomainData data, int index) {
            AssemblerSet set = new AssemblerSet(index);
            
            PrepareActions(data, set);
            PrepareConditionResolvers(data, set);

            return set;
        }

        private static void PrepareActions(GoapDomainData domainData, AssemblerSet set) {
            for (int i = 0; i < domainData.ActionCount; ++i) {
                GoapActionData? action = domainData.GetActionAt(i);
                if (action == null) {
                    throw new Exception("One of the actions is null. This can't be.");
                }

                IReadOnlyList<ClassData> atomActionsData = action.AtomActions;
                PrepareActions(domainData, action.Name, atomActionsData, set);
            }
        }

        private static void PrepareActions(GoapDomainData domainData, FixedString64 actionName, IReadOnlyList<ClassData> atomActionsData, AssemblerSet set) {
            for (int i = 0; i < atomActionsData.Count; ++i) {
                Option<AtomActionAssembler> assemblerInstance = TypeUtils.Instantiate<AtomActionAssembler>(atomActionsData[i], domainData.Variables);
                assemblerInstance.Match(new AddAtomActionToSet(set, actionName.GetHashCode(), i));
            }
        }
        
        private readonly struct AddAtomActionToSet : IOptionMatcher<AtomActionAssembler> {
            private readonly AssemblerSet set;
            private readonly int actionId;
            private readonly int order;

            public AddAtomActionToSet(AssemblerSet set, int actionId, int order) {
                this.set = set;
                this.actionId = actionId;
                this.order = order;
            }

            public void OnSome(AtomActionAssembler actionAssembler) {
                // We Init() so that the archetypes would be created prior to adding
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                actionAssembler.Init(ref entityManager, this.actionId, this.order);
                
                this.set.Add(actionAssembler);
            }

            public void OnNone() {
            }
        }

        private static void PrepareConditionResolvers(GoapDomainData domainData, AssemblerSet set) {
            IReadOnlyList<ConditionResolverData> conditionResolvers = domainData.ConditionResolvers;
            for (int i = 0; i < conditionResolvers.Count; ++i) {
                ConditionResolverData data = conditionResolvers[i];
                Option<ConditionResolverAssembler> assemblerInstance =
                    TypeUtils.Instantiate<ConditionResolverAssembler>(data.ResolverClass, domainData.Variables);
                assemblerInstance.Match(new AddConditionResolverPairToSet(set, data.ConditionName));
            }
        }

        private readonly struct AddConditionResolverPairToSet : IOptionMatcher<ConditionResolverAssembler> {
            private readonly AssemblerSet set;
            private readonly string conditionName;

            public AddConditionResolverPairToSet(AssemblerSet set, string conditionName) {
                this.set = set;
                this.conditionName = conditionName;
            }

            public void OnSome(ConditionResolverAssembler assembler) {
                // We Init() so that the archetypes would be created prior to adding
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                assembler.Init(ref entityManager);

                this.set.Add(this.conditionName, assembler);
            }

            public void OnNone() {
            }
        }

        public AssemblerSet GetAssemblerSet(int id) {
            return this.assemblerSets[id];
        }
    }
}