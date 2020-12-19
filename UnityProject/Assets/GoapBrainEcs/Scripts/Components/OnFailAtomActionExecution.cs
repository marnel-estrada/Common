using Unity.Entities;

namespace GoapBrainEcs {
    public struct OnFailAtomActionExecution : IComponentData {
        public readonly Entity parentOnFailActionExecution;
        private readonly int atomActionCount;
        public int currentIndex; // Index of the current action to execute OnFail action

        public OnFailAtomActionExecution(Entity parentOnFailActionExecution, int atomActionCount) {
            this.parentOnFailActionExecution = parentOnFailActionExecution;
            this.atomActionCount = atomActionCount;
            this.currentIndex = -1;
        }

        public bool IsDone {
            get {
                // Current index is already the last
                // No more actions to move to
                return this.currentIndex >= this.atomActionCount - 1;
            }
        }
    }
}