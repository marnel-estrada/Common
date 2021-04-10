namespace GoapBrain {
    class GoapDelegateAction : GoapAtomAction {
        
        public delegate GoapResult ResultActionDelegate(GoapAgent agent);

        private readonly ResultActionDelegate startAction;
        private readonly ResultActionDelegate executeAction;

        /// <summary>
        /// Constructor with only start action
        /// </summary>
        /// <param name="startAction"></param>
        public GoapDelegateAction(ResultActionDelegate startAction) {
            this.startAction = startAction;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="startAction"></param>
        /// <param name="executeAction"></param>
        public GoapDelegateAction(ResultActionDelegate startAction, ResultActionDelegate executeAction) {
            this.startAction = startAction;
            this.executeAction = executeAction;
        }

        public override GoapResult Start(GoapAgent agent) {
            if(this.startAction != null) {
                return this.startAction(agent); // Invoke
            }

            return GoapResult.SUCCESS;
        }

        public override GoapResult Update(GoapAgent agent) {
            if(this.executeAction == null) {
                // No execute action. Always return success.
                return GoapResult.SUCCESS;
            }

            return this.executeAction(agent);
        }

    }
}
