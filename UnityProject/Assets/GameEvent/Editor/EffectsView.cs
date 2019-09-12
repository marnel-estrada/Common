using Common;
using Common.Signal;

using UnityEditor;

namespace GameEvent {
    public class EffectsView : ClassDataView<Effect, EffectsBrowserWindow> {
        public EffectsView(EditorWindow parent, Signal repaintSignal) : base(parent, repaintSignal) {
        }
    }
}