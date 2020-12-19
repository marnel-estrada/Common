using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(ExecuteNextActionOnFailSystem))]
    public class ExecuteNextAtomActionOnFailSystem : TemplateComponentSystem {
        private EntityTypeHandle entityType;
        private ComponentTypeHandle<OnFailAtomActionExecution> onFailAtomExecutionType;
        
        private ComponentDataFromEntity<GoapAgent> allAgents;
        private ComponentDataFromEntity<PlanRequest> allRequests;
        private ComponentDataFromEntity<OnFailActionExecution> allOnFailActionExecution;

        private GoapPlanningSystem planningSystem;

        protected override void OnCreate() {
            base.OnCreate();
            this.planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
        }

        protected override EntityQuery ComposeQuery() {
            return GetEntityQuery(typeof(OnFailAtomActionExecution), typeof(ExecuteNextAtomActionOnFail));
        }

        protected override void BeforeChunkTraversal() {
            this.entityType = GetEntityTypeHandle();
            this.onFailAtomExecutionType = GetComponentTypeHandle<OnFailAtomActionExecution>();

            this.allAgents = GetComponentDataFromEntity<GoapAgent>();
            this.allRequests = GetComponentDataFromEntity<PlanRequest>();
            this.allOnFailActionExecution = GetComponentDataFromEntity<OnFailActionExecution>();
        }

        private NativeArray<Entity> entities;
        private NativeArray<OnFailAtomActionExecution> onFailAtomExecutions;

        protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
            this.entities = chunk.GetNativeArray(this.entityType);
            this.onFailAtomExecutions = chunk.GetNativeArray(this.onFailAtomExecutionType);
        }

        protected override void Process(int index) {
            OnFailAtomActionExecution onFailAtomExecution = this.onFailAtomExecutions[index];
            Entity currentEntity = this.entities[index];
            if (onFailAtomExecution.IsDone) {
                // No more atom actions to process. Tell parent plan to execute the on fail actions of the next action.
                this.PostUpdateCommands.AddComponent(onFailAtomExecution.parentOnFailActionExecution, new ExecuteNextActionOnFail());
                this.PostUpdateCommands.DestroyEntity(currentEntity); // Destroy since it's already done
                return;
            }
            
            // We remove this component so it won't run on the next frame
            this.PostUpdateCommands.RemoveComponent<ExecuteNextAtomActionOnFail>(currentEntity);
            
            PlanRequest request = this.allRequests[onFailAtomExecution.parentOnFailActionExecution];
            OnFailActionExecution parentOnFailExecution = this.allOnFailActionExecution[onFailAtomExecution.parentOnFailActionExecution]; 
            GoapAgent agent = this.allAgents[request.agentEntity];
            GoapDomain domain = this.planningSystem.GetDomain(agent.domainId);
            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(onFailAtomExecution.parentOnFailActionExecution);
            ushort currentActionId = actions[parentOnFailExecution.currentIndex].actionId;
            AtomActionSet atomSet = domain.GetAtomActionSet(currentActionId);
            
            // Move until an atom action contains an on fail action
            while (!onFailAtomExecution.IsDone) {
                // Move to next
                onFailAtomExecution.currentIndex += 1;
                this.onFailAtomExecutions[index] = onFailAtomExecution; // Modify the data

                AtomActionComposer composer = atomSet.GetComposerAt(onFailAtomExecution.currentIndex);
                if (!composer.HasOnFailAction) {
                    // Current atom action has no on fail action
                    continue;
                }
                
                // Create the entity that will execute the fail action
                Entity atomOnFailActionEntity = this.PostUpdateCommands.CreateEntity();
                this.PostUpdateCommands.AddComponent(atomOnFailActionEntity, new AtomActionOnFail(request.agentEntity, currentEntity));
                
                // Let composer compose the on fail action
                composer.PrepareOnFailAction(request.agentEntity, atomOnFailActionEntity, this.PostUpdateCommands);
                
                // We create this link so that atomOnFailActionEntity will be destroyed if the current entity is destroyed
                EntityReference.Create(currentEntity, atomOnFailActionEntity, this.PostUpdateCommands);

                return;
            }
            
            // If it reaches at this point and the whole execution is done, we proceed to the next action
            this.PostUpdateCommands.AddComponent(onFailAtomExecution.parentOnFailActionExecution, new ExecuteNextActionOnFail());
            this.PostUpdateCommands.DestroyEntity(currentEntity); // Destroy since it's already done
        }
    }
}