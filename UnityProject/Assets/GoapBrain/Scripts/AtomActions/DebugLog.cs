using UnityEngine;

using Common;

namespace GoapBrain {
    /// <summary>
    /// A generic action the merely Debug.Log()'s a text.
    /// </summary>
    [Group("GoapBrain.General")]
    public class DebugLog : GoapAtomAction {

        public NamedString text { get; set; }

        public override GoapResult Start(GoapAgent agent) {
            Debug.Log(text.Value);
            return GoapResult.SUCCESS;
        }

    }
}
