using Common;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    public class OptionRequirementsRenderer {
        private readonly EditorWindow parent;
        
        private readonly DataPool<EventData> pool;
        private readonly EventData eventItem;
        private readonly OptionData option;

        private readonly RequirementsView requirementsView;

        public OptionRequirementsRenderer(EditorWindow parent, DataPool<EventData> pool, EventData eventItem,
            OptionData option) {
            this.parent = parent;
            this.pool = pool;
            this.eventItem = eventItem;
            this.option = option;
            
            this.requirementsView = new RequirementsView(this.parent, OptionDetailsWindow.REPAINT);
        }
        
        private Vector2 scrollPos;

        public void Render() {
            this.requirementsView.Render(this.pool, this.option.Requirements, this.pool.Skin);
        }
    }
}