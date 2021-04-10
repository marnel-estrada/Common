using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common;
using Common.Signal;

namespace GoapBrain {
    /// <summary>
    /// Collection of all GOAP editor signals
    /// </summary>
    public static class GoapEditorSignals {

        public static readonly Signal REPAINT = new Signal("Repaint");

        public static readonly Signal CONDITION_SELECTED = new Signal("ConditionSelected");

    }
}
