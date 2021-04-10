namespace GoapBrain {
    /// <summary>
    /// A class that represents an atomized action
    /// </summary>
    public abstract class GoapAtomAction {

        /// <summary>
        /// Reset routines during planning
        /// </summary>
        /// <param name="agent"></param>
        public virtual void ResetForPlanning(GoapAgent agent) {
        }

        /// <summary>
        /// Returns whether or not the action can be executed
        /// This is on top of precondition requirements
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public virtual bool CanExecute(GoapAgent agent) {
            return true;
        }

        /// <summary>
        /// Routines on start of execution
        /// For example, variables needed for execution may be cached here
        /// </summary>
        /// <param name="agent"></param>
        public virtual GoapResult Start(GoapAgent agent) {
            return GoapResult.SUCCESS;
        }

        /// <summary>
        /// Action Update routines
        /// May be called in multiple frames
        /// </summary>
        /// <param name="agent"></param>
        public virtual GoapResult Update(GoapAgent agent) {
            return GoapResult.SUCCESS;
        }

        /// <summary>
        /// Routines when the action owner is finished (fail or success or replanned)
        /// </summary>
        /// <param name="agent"></param>
        public virtual void OnActionOwnerFinished(GoapAgent agent) {
        }
        
        /// <summary>
        /// Routines when the whole plan fails
        /// This may involve actions like returning or releasing a reserved resource
        /// </summary>
        /// <param name="agent"></param>
        public virtual void OnFail(GoapAgent agent) {
        }

    }
}
