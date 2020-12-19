using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(ExecuteNextAtomActionOnFailSystem))]
    public class CheckAtomActionOnFailExecution : TemplateComponentSystem {
        private EntityTypeHandle entityType;
        private ComponentTypeHandle<AtomActionOnFail> onFailActionType;

        private ComponentDataFromEntity<OnFailAtomActionExecution> allOnFailAtomActionExecution;
    
        protected override EntityQuery ComposeQuery() {
            return GetEntityQuery(typeof(AtomActionOnFail));
        }

        protected override void BeforeChunkTraversal() {
            this.entityType = GetEntityTypeHandle();
            this.onFailActionType = GetComponentTypeHandle<AtomActionOnFail>(true);

            this.allOnFailAtomActionExecution = GetComponentDataFromEntity<OnFailAtomActionExecution>(true);
        }

        private NativeArray<Entity> entities;
        private NativeArray<AtomActionOnFail> onFailActions;

        protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
            this.entities = chunk.GetNativeArray(this.entityType);
            this.onFailActions = chunk.GetNativeArray(this.onFailActionType);
        }

        protected override void Process(int index) {
            AtomActionOnFail actionOnFail = this.onFailActions[index];
            
            // Check if the on failed routines has been executed
            if (actionOnFail.done) {
                // Execute the on fail routine of the next atom action
                this.PostUpdateCommands.AddComponent(actionOnFail.parentOnFailAtomActionExecution, new ExecuteNextAtomActionOnFail());
                this.PostUpdateCommands.DestroyEntity(this.entities[index]); // Destroy because it's already done
            }
        }
    }
}