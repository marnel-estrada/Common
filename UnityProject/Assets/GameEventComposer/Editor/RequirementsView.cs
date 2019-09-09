using Common;
using Common.Signal;

using UnityEditor;

namespace GameEvent {
    public class RequirementsView : ClassDataView<Requirement, RequirementsBrowserWindow> {
        public RequirementsView(EditorWindow parent, Signal repaintSignal) : base(parent, repaintSignal) {
        }
    }
}