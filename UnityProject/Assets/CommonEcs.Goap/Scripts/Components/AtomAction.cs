using Unity.Entities;

namespace CommonEcs.Goap {
    public struct AtomAction : IComponentData {
        public readonly Entity agentEntity;

        // Denotes the action where this atom action belongs
        public readonly int actionId;

        // The order number in which this atom action executes
        public readonly int order;
        
        // An optional ID that we can set for easier debugging
        public readonly int debugId;

        // The result when executing the action
        public GoapResult result;

        public bool canExecute;
        public bool started;
        
        // We need this such that we don't have to call MarkCanExecute() if its currently 
        // executing. We should not call MarkCanExecute() to an executing action because it will
        // reset the started flag which will run the Start() routine again.
        public bool executing;

        public AtomAction(int actionId, Entity agentEntity, int order, int debugId = 0) : this() {
            this.agentEntity = agentEntity;
            this.actionId = actionId;
            this.order = order;
            this.debugId = debugId;
        }

        public void MarkCanExecute() {
            this.canExecute = true;
            this.result = GoapResult.RUNNING;
            this.started = false;
        }
    }
}