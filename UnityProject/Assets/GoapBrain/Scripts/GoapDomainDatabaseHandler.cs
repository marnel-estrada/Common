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
        private GoapDomainDataContainer? domains;

        private AssemblerSet[]? assemblerSets;

        private BlobAssetReference<GoapDomainDatabase> domainDbReference;

        // We temporarily keep the conditions names and action names here 
        private readonly Dictionary<int, FixedString64Bytes> textMap = new(500);
        
        public GoapDomainDataContainer? Domains => this.domains;

        private void Awake() {
            Assertion.NotNull(this.domains);
            if (this.domains == null) {
                throw new Exception($"{nameof(this.domains)} can't be null");
            }

            List<GoapDomainData>? domainsDataList = this.domains.DataList;

            if (domainsDataList == null) {
                throw new CantBeNullException(nameof(domainsDataList));
            }

            Assertion.IsTrue(domainsDataList.Count > 0);

            BlobBuilder builder = new(Allocator.Temp);

            // Prepare DomainDatabase
            ref GoapDomainDatabase db = ref builder.ConstructRoot<GoapDomainDatabase>();
            BlobBuilderArray<GoapDomain> domainsBuilder = builder.Allocate(ref db.domains, domainsDataList.Count);

            this.assemblerSets = new AssemblerSet[domainsDataList.Count];

            for (int i = 0; i < domainsDataList.Count; ++i) {
                try {
                    GoapDomainData domainData = domainsDataList[i];
                    domainsBuilder[i] = ParseDomain(domainData); // We use the index used here when creating the agent
                    this.assemblerSets[i] = ParseAssemblerSet(domainData, i);
                } catch (Exception e) {
                    Debug.LogError($"Error while parsing domain at index {i}");
                    throw;
                }
            }

            this.domainDbReference = builder.CreateBlobAssetReference<GoapDomainDatabase>(Allocator.Persistent);
            
            // Don't forget to create the text DB after all domains are parsed
            GoapTextDbSystem textDbSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<GoapTextDbSystem>();
            Assertion.NotNull(textDbSystem);
            textDbSystem.CreateTextDb(this.textMap);
            this.textMap.Clear(); // We clear to conserve memory
        }

        // We need this to have unique action names
        private readonly HashSet<string> addedActions = new();

        private GoapDomain ParseDomain(GoapDomainData data) {
            this.addedActions.Clear();

            GoapDomain domain = new(data.name);
            AddActions(ref domain, data, this.addedActions);

            // Parse each extension domain
            // Note here that we're just adding the actions in the extensions to the main GoapDomain
            List<GoapExtensionData> extensions = data.Extensions;
            for (int i = 0; i < extensions.Count; ++i) {
                GoapDomainData? extensionDomain = extensions[i].DomainData;
                Assertion.NotNull(extensionDomain);
                if (extensionDomain != null) {
                    AddActions(ref domain, extensionDomain, this.addedActions);
                }
            }

            return domain;
        }

        private void AddActions(ref GoapDomain domain, GoapDomainData data, HashSet<string> addedActions) {
            for (int i = 0; i < data.ActionCount; ++i) {
                GoapActionData actionData = data.GetActionAt(i);
                if (!actionData.Enabled) {
                    // Not enabled. Skip adding the action.
                    continue;
                }

                // Check that the action is unique (not been added before)
                Assertion.IsTrue(!addedActions.Contains(actionData.Name), actionData.Name);

                ConditionData? effectData = actionData.Effect;
                Assertion.NotNull(effectData);

                if (effectData == null) {
                    throw new Exception($"Action {actionData.Name} does not have an effect.");
                }

                if (string.IsNullOrEmpty(effectData.Name?.Trim())) {
                    throw new Exception($"Action {actionData.Name} does not have an effect name.");
                }

                Condition effect = new(effectData.Name, effectData.Value);
                AddToTextMap(effectData.Name);
                
                GoapAction action = new(actionData.Name, actionData.Cost, actionData.AtomActions.Count,
                    effect);
                AddToTextMap(actionData.Name);

                AddPreconditions(ref action, actionData);
                Assertion.IsTrue(action.preconditions.Count == actionData.Preconditions.Count);

                domain.AddAction(action);

                // Add to unique set
                addedActions.Add(actionData.Name);
            }
        }

        public ref readonly BlobAssetReference<GoapDomainDatabase> DbReference {
            get {
                return ref this.domainDbReference;
            }
        }

        private void AddPreconditions(ref GoapAction action, GoapActionData data) {
            for (int i = 0; i < data.Preconditions.Count; ++i) {
                ConditionData preconditionData = data.Preconditions[i];
                Condition condition = new Condition(preconditionData.Name, preconditionData.Value);
                action.AddPrecondition(condition);
                
                AddToTextMap(preconditionData.Name);
            }
        }
        
        private void AddToTextMap(string? s) {
            Assertion.NotNull(s);

            if (s == null) {
                return;
            }

            FixedString64Bytes fixedString = s;
            int hashCode = fixedString.GetHashCode();

            if (this.textMap.TryGetValue(hashCode, out FixedString64Bytes existingString)) {
                // They should be the same because if not, then we have a problem. This is a collision.
                Assertion.IsTrue(existingString == fixedString);
                    
                // Already contains such text
                return;
            }
                
            this.textMap[hashCode] = fixedString;
        }

        // We need this so that we can check that each condition only has one resolver
        private readonly HashSet<string> conditionsWithResolvers = new();

        private AssemblerSet ParseAssemblerSet(GoapDomainData data, int index) {
            AssemblerSet set = new(index);

            this.conditionsWithResolvers.Clear();

            PrepareActions(data, set);
            PrepareConditionResolvers(data, set, this.conditionsWithResolvers);

            // Add atom actions and condition resolvers from extensions as well
            IReadOnlyList<GoapExtensionData> extensions = data.Extensions;
            for (int i = 0; i < extensions.Count; ++i) {
                GoapDomainData? extension = extensions[i].DomainData;
                if (extension != null) {
                    PrepareActions(extension, set);
                    PrepareConditionResolvers(extension, set, this.conditionsWithResolvers);
                }
            }

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

        private static void PrepareActions(GoapDomainData domainData, FixedString64Bytes actionName, IReadOnlyList<ClassData> atomActionsData, AssemblerSet set) {
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

        private static void PrepareConditionResolvers(GoapDomainData domainData, AssemblerSet set, HashSet<string> conditionsWithResolvers) {
            IReadOnlyList<ConditionResolverData> conditionResolvers = domainData.ConditionResolvers;
            for (int i = 0; i < conditionResolvers.Count; ++i) {
                ConditionResolverData data = conditionResolvers[i];

                // A condition can only have one resolver
                Assertion.IsTrue(!conditionsWithResolvers.Contains(data.ConditionName), data.ConditionName);

                Option<ConditionResolverAssembler> assemblerInstance =
                    TypeUtils.Instantiate<ConditionResolverAssembler>(data.ResolverClass, domainData.Variables);
                assemblerInstance.Match(new AddConditionResolverPairToSet(set, data.ConditionName));

                // Add to unique set
                conditionsWithResolvers.Add(data.ConditionName);
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
            if (this.assemblerSets == null) {
                throw new Exception($"{nameof(this.assemblerSets)} can't be null");
            }

            return this.assemblerSets[id];
        }
    }
}