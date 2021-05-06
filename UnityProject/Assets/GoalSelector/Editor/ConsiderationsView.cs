using Common;
using Common.Signal;

using CommonEcs.UtilityBrain;

using JetBrains.Annotations;

using UnityEditor;

namespace GoalSelector.Editor {
    public class ConsiderationsView : ClassDataView<ConsiderationAssembler, ConsiderationsAssemblerBrowserWindow> {
        public ConsiderationsView([NotNull] EditorWindow parent, [NotNull] Signal repaintSignal) : base(parent, repaintSignal) {
        }
    }
}