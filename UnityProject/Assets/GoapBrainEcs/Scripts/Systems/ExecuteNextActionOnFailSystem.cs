using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(CheckAtomActionExecution))]
    public class ExecuteNextActionOnFailSystem : TemplateComponentSystem {
        private EntityTypeHandle entityType;
        private ComponentTypeHandle<PlanRequest> requestType;
        private ComponentTypeHandle<OnFailActionExecution> onFailExecutionType;
        
        private ComponentDataFromEntity<GoapAgent> allAgents;

        private GoapPlanningSystem planningSystem;

        protected override void OnCreate() {
            base.OnCreate();
            
            // Cache
            this.planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
        }

        protected override EntityQuery ComposeQuery() {
            return GetEntityQuery(typeof(PlanRequest), typeof(OnFailActionExecution), 
                typeof(ExecuteNextActionOnFail));
        }

        protected override void BeforeChunkTraversal() {
            this.entityType = GetEntityTypeHandle();
            this.requestType = GetComponentTypeHandle<PlanRequest>(true);
            this.onFailExecutionType = GetComponentTypeHandle<OnFailActionExecution>();

            this.allAgents = GetComponentDataFromEntity<GoapAgent>();
        }
        
        private NativeArray<Entity> entities;
        private NativeArray<PlanRequest> requests;
        private NativeArray<OnFailActionExecution> onFailExecutions;

        protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
            this.entities = chunk.GetNativeArray(this.entityType);
            this.requests = chunk.GetNativeArray(this.requestType);
            this.onFailExecutions = chunk.GetNativeArray(this.onFailExecutionType);
        }

        protected override void Process(int index) {
            OnFailActionExecution onFailExecution = this.onFailExecutions[index];
            Entity currentEntity = this.entities[index];
            
            // Remove this component so it won't be processed on the next frame
            this.PostUpdateCommands.RemoveComponent<ExecuteNextActionOnFail>(currentEntity);

            if (onFailExecution.IsDone) {
                // Execution is already done
                // We just add PlanExecutionSucceeded to mark it for replanning and eventual entity destruction
                this.PostUpdateCommands.AddComponent(currentEntity, new PlanExecutionFailed());
                return;
            }
            
            // Move to next action
            onFailExecution.currentIndex += 1;
            this.onFailExecutions[index] = onFailExecution; // Modify data
            
            PlanRequest request = this.requests[index];
            GoapAgent agent = this.allAgents[request.agentEntity];
            GoapDomain domain = this.planningSystem.GetDomain(agent.domainId);
            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(currentEntity);
            ushort currentActionId = actions[onFailExecution.currentIndex].actionId;
            AtomActionSet atomSet = domain.GetAtomActionSet(currentActionId);
            
            // Create a new entity that handles the traversal and execution of atom on fail actions
            Entity onFailAtomActionExecutionEntity = this.PostUpdateCommands.CreateEntity();
            this.PostUpdateCommands.AddComponent(onFailAtomActionExecutionEntity, new OnFailAtomActionExecution(currentEntity, atomSet.Count));
            
            // We added this entity so that the first atom action's on fail action would be executed
            this.PostUpdateCommands.AddComponent(onFailAtomActionExecutionEntity, new ExecuteNextAtomActionOnFail());
            
            // Automatically destroy onFailAtomActionExecutionEntity when the OnFailAtomActionExecution entity is destroyed 
            EntityReference.Create(currentEntity, onFailAtomActionExecutionEntity, this.PostUpdateCommands);
        }
    }
}