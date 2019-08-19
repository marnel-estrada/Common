using System;
using System.Collections.Generic;

using Common;

using UnityEngine;

namespace GameEvent {
    [CreateAssetMenu(menuName = "GameEvent/EventsPool")]
    public class EventsPool : ScriptableObject {
        [SerializeField]
        private List<EventData> events;

        [SerializeField]
        private IdGenerator eventsIdGenerator = new IdGenerator();
        
        public Maybe<EventData> FindByNameId(string nameId) {
            foreach(EventData eventData in this.events) {
                if(eventData.NameId.EqualsFast(nameId)) {
                    return new Maybe<EventData>(eventData);
                }
            }

            return Maybe<EventData>.Nothing;
        }

        public EventData this[int index] {
            get {
                return this.events[index];
            }
        }

        public EventData Add(string nameId) {
            // Should not exist yet
            Assertion.Assert(!FindByNameId(nameId).HasValue);

            EventData eventData = new EventData(this.eventsIdGenerator.Generate());
            eventData.NameId = nameId;
            eventData.DescriptionId = nameId + ".Description";

            this.events.Add(eventData);

            return eventData;
        }

        public void Remove(EventData eventData) {
            this.events.Remove(eventData);
        }

        public int Count {
            get {
                return this.events.Count;
            }
        }

        public void Sort(){
            this.events.Sort(SortByNameId);
        }

        private int SortByNameId(EventData a, EventData b) {
            return String.Compare(a.NameId, b.NameId, StringComparison.Ordinal);
        }
    }
}