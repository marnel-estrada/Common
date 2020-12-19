using Unity.Entities;

namespace GoapBrainEcs {
    public struct AtomActionSetExecution : IComponentData {
        public readonly Entity parentPlanExecution; // Parent plan here contains GoapPlanRequest and PlanExecution
        public readonly int atomActionCount; // Atom actions count
        public int atomActionIndex; // The current index of atom action to execute

        public AtomActionSetExecution(Entity parentPlanExecution, int atomActionCount) {
            this.parentPlanExecution = parentPlanExecution;
            this.atomActionCount = atomActionCount;
            this.atomActionIndex = -1;
        }

        public bool IsDone {
            get {
                // Done if current index is already the last. No more action to move to.
                return this.atomActionIndex >= this.atomActionCount - 1;
            }
        }
    }
}