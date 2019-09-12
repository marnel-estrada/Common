using System.Collections.Generic;

namespace GameEvent {
    public class EventInstanceManager {
        private readonly EventsPool pool;
        
        // Map of event ID to EventInstance
        private Dictionary<int, EventInstance> map = new Dictionary<int, EventInstance>(50);

        public EventInstanceManager(EventsPool pool) {
            this.pool = pool;
            Parse();
        }

        private void Parse() {
            foreach (EventData eventData in pool.GetAll()) {
                EventInstance instance = new EventInstance(eventData);
                this.map[instance.IntId] = instance;
            }
        }
    }
}