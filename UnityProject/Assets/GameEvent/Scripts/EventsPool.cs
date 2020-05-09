using System;

using Common;

using UnityEngine;

namespace GameEvent {
    [CreateAssetMenu(menuName = "GameEvent/EventsPool")]
    public class EventsPool : DataPool<EventData> {
        public void Sort(){
            base.Sort(SortByNameId);
        }
        
        private static int SortByNameId(EventData a, EventData b) {
            return string.Compare(a.NameId, b.NameId, StringComparison.Ordinal);
        }
    }
}