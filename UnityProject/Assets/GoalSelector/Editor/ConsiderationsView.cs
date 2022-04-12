using Common;
using Common.Signal;

using CommonEcs.UtilityBrain;

using UnityEditor;

namespace GoalSelector.Editor {
    public class ConsiderationsView : ClassDataView<ConsiderationAssembler, ConsiderationsAssemblerBrowserWindow> {
        public ConsiderationsView(EditorWindow parent, Signal repaintSignal) : base(parent, repaintSignal) {
        }
    }
}