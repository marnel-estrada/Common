using System.Collections.Generic;

namespace GameEvent {
    public class EventInstanceManager {
        private readonly EventsPool pool;
        
        // Map of event ID to EventInstance
        private readonly Dictionary<int, EventInstance> map = new Dictionary<int, EventInstance>(50);

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

        /// <summary>
        /// Retrieves an event by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public EventInstance Get(int id) {
            return this.map[id];
        }
    }
}