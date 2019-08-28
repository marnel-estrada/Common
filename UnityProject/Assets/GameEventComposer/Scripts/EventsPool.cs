using System;

using Common;

using UnityEngine;

namespace GameEvent {
    [CreateAssetMenu(menuName = "GameEvent/EventsPool")]
    public class EventsPool : DataPool<EventData> {
        public void Sort(){
            base.Sort(SortByNameId);
        }
        
        private int SortByNameId(EventData a, EventData b) {
            return String.Compare(a.NameId, b.NameId, StringComparison.Ordinal);
        }
    }
}