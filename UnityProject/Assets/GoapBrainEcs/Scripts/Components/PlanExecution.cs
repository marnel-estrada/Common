using Unity.Entities;

namespace GoapBrainEcs {
    public struct PlanExecution : IComponentData {
        private readonly int actionCount;
        public int actionIndex;

        public PlanExecution(int actionCount) {
            this.actionCount = actionCount;
            this.actionIndex = -1;
        }

        public bool IsDone {
            get {
                // Current index is already the last
                // No more actions to move to
                return this.actionIndex >= this.actionCount - 1;
            }
        }
    }
}