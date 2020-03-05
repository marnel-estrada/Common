using System.Collections.Generic;

using Common;

namespace GameEvent {
    public class EventInstanceManager {
        private readonly EventsPool pool;
        
        // Map of event ID to EventInstance
        private readonly Dictionary<int, EventInstance> map = new Dictionary<int, EventInstance>(50);

        public EventInstanceManager(EventsPool pool) {
            this.pool = pool;
        }

        /// <summary>
        /// Retrieves an event by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Option<EventInstance> Get(int id) {
            Maybe<EventData> foundData = this.pool.Find(id);
            if (!foundData.HasValue) {
                Assertion.Assert(false, "There's no event with ID: " + id);
                return Option<EventInstance>.NONE;
            }
            
            // Check if it already exists in map
            EventInstance instance = this.map.Find(id);
            if (instance != null) {
                return Option<EventInstance>.Some(instance);
            }

            // Not yet in map. We instantiate a new one.
            // We did it this way to save memory since not all events would be resolve
            // in a single game
            instance = new EventInstance(foundData.Value);
            this.map[instance.IntId] = instance;

            return Option<EventInstance>.Some(instance);
        }
    }
}