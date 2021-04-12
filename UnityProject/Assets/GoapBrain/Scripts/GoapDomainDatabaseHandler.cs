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
        
        private BlobAssetReference<GoapDomainDatabase> domainDbReference;

        private void Awake() {
            Assertion.NotNull(this.domains);
            Assertion.IsTrue(this.domains.Length > 0);
            
            BlobBuilder builder = new BlobBuilder(Allocator.Temp);

            // Prepare DomainDatabase
            ref GoapDomainDatabase db = ref builder.ConstructRoot<GoapDomainDatabase>();
            BlobBuilderArray<GoapDomain> domainsBuilder = builder.Allocate(ref db.domains, this.domains.Length);

            for (int i = 0; i < this.domains.Length; ++i) {
                domainsBuilder[0] = Parse(this.domains[i]); // We use the index used here when creating the agent
            }

            this.domainDbReference = builder.CreateBlobAssetReference<GoapDomainDatabase>(Allocator.Temp);
        }

        private static GoapDomain Parse(GoapDomainData data) {
            GoapDomain domain = new GoapDomain();

            for (int i = 0; i < data.ActionCount; ++i) {
                GoapActionData actionData = data.GetActionAt(i);
                Condition? effectData = actionData.Effect;
                Assertion.NotNull(effectData);

                if (effectData != null) {
                    CommonEcs.Goap.Condition unmanagedEffect =
                        new CommonEcs.Goap.Condition(effectData.Name, effectData.Value);
                    GoapAction action = new GoapAction(actionData.Name, actionData.Cost, unmanagedEffect);
                    
                    AddPreconditions(ref action, actionData);
                    Assertion.IsTrue(action.preconditions.Count == actionData.Preconditions.Count);
                    
                    domain.AddAction(action);
                }
            }

            return domain;
        }

        private static void AddPreconditions(ref GoapAction action, GoapActionData data) {
            for (int i = 0; i < data.Preconditions.Count; ++i) {
                Condition preconditionData = data.Preconditions[i];
                CommonEcs.Goap.Condition unmanagedPrecondition =
                    new CommonEcs.Goap.Condition(preconditionData.Name, preconditionData.Value);
                action.AddPrecondition(unmanagedPrecondition);
            }
        }
    }
}