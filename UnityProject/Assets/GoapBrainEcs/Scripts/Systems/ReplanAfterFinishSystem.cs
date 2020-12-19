using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace GoapBrainEcs {
    // Try to replan first before destroying the finished plans in DestroyFinishedPlans
    [UpdateAfter(typeof(CheckAtomActionOnFailExecution))]
    [UpdateBefore(typeof(DestroyFinishedPlansSystem))]
    public class ReplanAfterFinishSystem : TemplateComponentSystem {
        private ComponentTypeHandle<PlanRequest> requestType;
    
        protected override EntityQuery ComposeQuery() {
            // All PlanRequests that already executed with either PlanExecutionSucceeded or PlanExecutionFailed  
            return GetEntityQuery(this.ConstructQuery(new ComponentType[] {
                typeof(PlanExecutionSucceeded), typeof(PlanExecutionFailed)
            }, null, new ComponentType[] {
                typeof(PlanRequest), typeof(PlanExecution)
            }));
        }

        protected override void BeforeChunkTraversal() {
            this.requestType = GetComponentTypeHandle<PlanRequest>(true);
        }

        private NativeArray<PlanRequest> requests;

        protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
            this.requests = chunk.GetNativeArray(this.requestType);
        }

        protected override void Process(int index) {
            PlanRequest request = this.requests[index];
            
            // To replan, just create a new PlanRequest entity with the same agent
            PlanRequest.Create(request.agentEntity, this.PostUpdateCommands);
            
            // Note here that we don't need to destroy the plan entity as it will be destroyed by
            // DestroyFinishedPlansSystem
        }
    }
}