using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(ExecuteNextActionSystem))]
    public class ExecuteNextAtomActionSystem : TemplateComponentSystem {
        private EntityTypeHandle entityType;
        private ComponentTypeHandle<AtomActionSetExecution> actionSetExecutionType;
        
        private ComponentDataFromEntity<GoapAgent> allAgents;
        private ComponentDataFromEntity<PlanRequest> allRequests;
        private ComponentDataFromEntity<PlanExecution> allExecutions;

        private GoapPlanningSystem planningSystem;

        protected override void OnCreate() {
            base.OnCreate();
            this.planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
        }

        protected override EntityQuery ComposeQuery() {
            return GetEntityQuery(typeof(AtomActionSetExecution), typeof(ExecuteNextAtomAction));
        }

        protected override void BeforeChunkTraversal() {
            this.entityType = GetEntityTypeHandle();
            this.actionSetExecutionType = GetComponentTypeHandle<AtomActionSetExecution>();

            this.allAgents = GetComponentDataFromEntity<GoapAgent>();
            this.allRequests = GetComponentDataFromEntity<PlanRequest>();
            this.allExecutions = GetComponentDataFromEntity<PlanExecution>();
        }

        private NativeArray<Entity> entities;
        private NativeArray<AtomActionSetExecution> atomSetExecutions;

        protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
            this.entities = chunk.GetNativeArray(this.entityType);
            this.atomSetExecutions = chunk.GetNativeArray(this.actionSetExecutionType);
        }

        protected override void Process(int index) {
            AtomActionSetExecution atomSetExecution = this.atomSetExecutions[index];
            Entity atomSetExecutionEntity = this.entities[index];
            if (atomSetExecution.IsDone) {
                // No more atom actions to execute. Tell parent plan to execute the next action.
                this.PostUpdateCommands.AddComponent(atomSetExecution.parentPlanExecution, new ExecuteNextAction());
                this.PostUpdateCommands.DestroyEntity(atomSetExecutionEntity); // Destroy since it's done
                return;
            }
            
            // Move to next atom action
            atomSetExecution.atomActionIndex += 1;
            this.atomSetExecutions[index] = atomSetExecution; // Modify data

            // Prepare the entity that represents the atom action
            // Note that parent plan execution is also the PlanRequest
            PlanRequest request = this.allRequests[atomSetExecution.parentPlanExecution];
            PlanExecution parentExecution = this.allExecutions[atomSetExecution.parentPlanExecution];
            GoapAgent agent = this.allAgents[request.agentEntity];
            GoapDomain domain = this.planningSystem.GetDomain(agent.domainId);
            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(atomSetExecution.parentPlanExecution);
            ushort currentActionId = actions[parentExecution.actionIndex].actionId;
            AtomActionSet atomSet = domain.GetAtomActionSet(currentActionId);
            
            // Create the atom action entity
            Entity atomActionEntity = this.PostUpdateCommands.CreateEntity();
            this.PostUpdateCommands.AddComponent(atomActionEntity, new AtomAction(request.agentEntity, atomSetExecutionEntity));

            // Let composer compose the atomActionEntity
            AtomActionComposer composer = atomSet.GetComposerAt(atomSetExecution.atomActionIndex);
            composer.Prepare(request.agentEntity, atomActionEntity, this.PostUpdateCommands);
            
            EntityReference.Create(atomSetExecutionEntity, atomActionEntity, this.PostUpdateCommands);
            
            // We remove this component so it won't run on the next frame
            this.PostUpdateCommands.RemoveComponent<ExecuteNextAtomAction>(atomSetExecutionEntity);
        }
    }
}