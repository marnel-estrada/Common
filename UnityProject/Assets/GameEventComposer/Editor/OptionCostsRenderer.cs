using Common;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    public class OptionCostsRenderer {
        private readonly EditorWindow parent;
        
        private readonly DataPool<EventData> pool;
        private readonly EventData eventItem;
        private readonly OptionData option;

        private readonly CostsView costsView;

        public OptionCostsRenderer(EditorWindow parent, DataPool<EventData> pool, EventData eventItem,
            OptionData option) {
            this.parent = parent;
            this.pool = pool;
            this.eventItem = eventItem;
            this.option = option;
            
            this.costsView = new CostsView(this.parent, OptionDetailsWindow.REPAINT);
        }
        
        private Vector2 scrollPos;

        public void Render() {
            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
            
            this.costsView.Render(this.pool, this.option.Costs, this.pool.Skin);
            
            GUILayout.EndScrollView();
        }
    }
}