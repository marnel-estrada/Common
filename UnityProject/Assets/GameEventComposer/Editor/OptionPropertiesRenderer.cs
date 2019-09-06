using Common;

using UnityEditor.Experimental.Networking.PlayerConnection;

using UnityEngine;

namespace GameEvent {
    public class OptionPropertiesRenderer {
        private readonly DataPool<EventData> pool;
        private readonly EventData eventItem;
        private readonly OptionData option;
        
        private readonly GenericObjectRenderer renderer = new GenericObjectRenderer(typeof(OptionData));

        public OptionPropertiesRenderer(DataPool<EventData> pool, EventData eventItem, OptionData option) {
            this.pool = pool;
            this.eventItem = eventItem;
            this.option = option;
        }

        public void Render() {
            this.renderer.Render(this.option);
        }
    }
}