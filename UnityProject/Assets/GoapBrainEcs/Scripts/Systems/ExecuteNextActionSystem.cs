using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(CheckSearchActionSystem))]
    public class ExecuteNextActionSystem : TemplateComponentSystem {
        private EntityTypeHandle entityType;
        private ComponentTypeHandle<PlanRequest> requestType;
        private ComponentTypeHandle<PlanExecution> executionType;

        private ComponentDataFromEntity<GoapAgent> allAgents;

        private GoapPlanningSystem planningSystem;

        protected override void OnCreate() {
            // Needed to call base here
            base.OnCreate();
            
            // Cache
            this.planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
        }

        protected override EntityQuery ComposeQuery() {
            return GetEntityQuery(typeof(PlanRequest), typeof(PlanExecution), 
                typeof(ExecuteNextAction));
        }

        protected override void BeforeChunkTraversal() {
            this.entityType = GetEntityTypeHandle();
            this.requestType = GetComponentTypeHandle<PlanRequest>(true);
            this.executionType = GetComponentTypeHandle<PlanExecution>();

            this.allAgents = GetComponentDataFromEntity<GoapAgent>();
        }

        private NativeArray<Entity> entities;
        private NativeArray<PlanRequest> requests;
        private NativeArray<PlanExecution> executions;

        protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
            this.entities = chunk.GetNativeArray(this.entityType);
            this.requests = chunk.GetNativeArray(this.requestType);
            this.executions = chunk.GetNativeArray(this.executionType);
        }

        protected override void Process(int index) {
            PlanExecution execution = this.executions[index];
            Entity executionEntity = this.entities[index];
            
            // Remove this component so it won't be processed on the next frame
            this.PostUpdateCommands.RemoveComponent<ExecuteNextAction>(executionEntity);
            
            if (execution.IsDone) {
                // Execution is already done
                // We just add PlanExecutionSucceeded to mark it for replanning and eventual entity destruction
                this.PostUpdateCommands.AddComponent(executionEntity, new PlanExecutionSucceeded());
                return;
            }
            
            // Move to next action
            execution.actionIndex += 1;
            this.executions[index] = execution; // Modify data

            PlanRequest request = this.requests[index];
            GoapAgent agent = this.allAgents[request.agentEntity];
            GoapDomain domain = this.planningSystem.GetDomain(agent.domainId);
            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(executionEntity);
            ushort currentActionId = actions[execution.actionIndex].actionId;
            AtomActionSet atomSet = domain.GetAtomActionSet(currentActionId);
            
            // Create a new entity that handles the traversal and execution of atom actions
            Entity atomActionsExecutionEntity = this.PostUpdateCommands.CreateEntity();
            this.PostUpdateCommands.AddComponent(atomActionsExecutionEntity, new AtomActionSetExecution(executionEntity, atomSet.Count));
            
            // We added this entity so that the first atom action would be executed
            this.PostUpdateCommands.AddComponent(atomActionsExecutionEntity, new ExecuteNextAtomAction());
            
            // Automatically destroy atomActionsExecutionEntity when the PlanExecution entity is destroyed 
            EntityReference.Create(executionEntity, atomActionsExecutionEntity, this.PostUpdateCommands);
        }
    }
}