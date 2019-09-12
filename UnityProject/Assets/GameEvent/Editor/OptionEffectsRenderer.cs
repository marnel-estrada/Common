using Common;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    public class OptionEffectsRenderer {
        private readonly EditorWindow parent;
        
        private readonly DataPool<EventData> pool;
        private readonly EventData eventItem;
        private readonly OptionData option;

        private readonly EffectsView view;

        public OptionEffectsRenderer(EditorWindow parent, DataPool<EventData> pool, EventData eventItem,
            OptionData option) {
            this.parent = parent;
            this.pool = pool;
            this.eventItem = eventItem;
            this.option = option;
            
            this.view = new EffectsView(this.parent, OptionDetailsWindow.REPAINT);
        }
        
        private Vector2 scrollPos;

        public void Render() {
            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
            
            this.view.Render(this.pool, this.option.Effects, this.pool.Skin);
            
            GUILayout.EndScrollView();
        }
    }
}