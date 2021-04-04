using Unity.Entities;

namespace CommonEcs.Goap {
    public struct AtomAction : IComponentData {
        public readonly Entity agentEntity;

        // Denotes the action where this atom action belongs
        public readonly int actionId;

        // The order number in which this atom action executes
        public readonly int order;

        // The result when executing the action
        public GoapResult result;

        public bool canExecute;
        public bool started;

        public AtomAction(int actionId, Entity agentEntity, int order) : this() {
            this.agentEntity = agentEntity;
            this.actionId = actionId;
            this.order = order;
        }

        public void MarkCanExecute() {
            this.canExecute = true;
            this.result = GoapResult.RUNNING;
            this.started = false;
        }
    }
}