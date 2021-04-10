using UnityEngine;

namespace GoapBrain {
    class DebugLogAction : GoapAtomAction {

        private readonly string log;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="log"></param>
        public DebugLogAction(string log) {
            this.log = log;
        }

        public override GoapResult Update(GoapAgent agent) {
            Debug.Log(this.log);
            return GoapResult.SUCCESS;
        }

    }
}
