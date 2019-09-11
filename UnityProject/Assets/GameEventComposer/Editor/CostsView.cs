using Common;
using Common.Signal;

using UnityEditor;

namespace GameEvent {
    public class CostsView : ClassDataView<Cost, CostsBrowserWindow> {
        public CostsView(EditorWindow parent, Signal repaintSignal) : base(parent, repaintSignal) {
        }
    }
}