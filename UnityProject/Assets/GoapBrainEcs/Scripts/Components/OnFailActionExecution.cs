using Unity.Entities;

namespace GoapBrainEcs {
    public struct OnFailActionExecution : IComponentData {
        private readonly int actionCount;
        public int currentIndex; // Index of the current action to execute OnFail action

        public OnFailActionExecution(int actionCount) {
            this.actionCount = actionCount;
            this.currentIndex = -1;
        }

        public bool IsDone {
            get {
                // Current index is already the last
                // No more actions to move to
                return this.currentIndex >= this.actionCount - 1;
            }
        }
    }
}