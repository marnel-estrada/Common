using CommonEcs;

using Unity.Entities;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(ReplanAfterFinishSystem))]
    public class DestroyFinishedPlansSystem : ComponentSystem {
        private EntityQuery query;

        protected override void OnCreate() {
            // All PlanRequests that already executed with either PlanExecutionSucceeded or PlanExecutionFailed
            this.query = GetEntityQuery(this.ConstructQuery(new ComponentType[] {
                typeof(PlanExecutionSucceeded), typeof(PlanExecutionFailed)
            }, null, new ComponentType[] {
                typeof(PlanRequest), typeof(PlanExecution)
            }));
        }

        protected override void OnUpdate() {
            this.EntityManager.DestroyEntity(this.query);
        }
    }
}