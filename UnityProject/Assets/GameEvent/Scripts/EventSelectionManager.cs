using System.Collections.Generic;

using Common;

namespace GameEvent {
    public class EventSelectionManager {
        private readonly EventsPool pool; // The main pool of events
        private readonly IEventSelectionStrategy selectionStrategy;
        
        private readonly EventDeck deck = new EventDeck();

        private readonly Rarity forcedRarity;

        public EventSelectionManager(EventsPool pool, IEventSelectionStrategy selectionStrategy, Rarity forcedRarity) {
            this.pool = pool;
            this.selectionStrategy = selectionStrategy ?? new DefaultSelectionStrategy();
            this.forcedRarity = forcedRarity;
        }
        
        private readonly Dictionary<int, EventData> tempMap = new Dictionary<int, EventData>(100);

        public void Reset() {
            this.deck.Clear();
            this.tempMap.Clear();
            this.selectionStrategy.Reset();

            // Add to temporary map
            foreach (EventData eventData in this.pool.GetAll()) {
                this.tempMap.Add(eventData.IntId, eventData);
            }
            
            // Remove child events. They shouldn't be added to the deck
            foreach (EventData eventData in this.pool.GetAll()) {
                PruneChildEvents(eventData);
            }
            
            // Populate deck using map where child events were removed
            foreach (KeyValuePair<int, EventData> entry in this.tempMap) {
                PopulateDeck(entry.Value);
            }
            
            this.deck.Shuffle();
        }

        private void PruneChildEvents(EventData eventData) {
            List<OptionData> options = eventData.Options;
            for (int i = 0; i < options.Count; ++i) {
                if (options[i].HasChildEvent) {
                    this.tempMap.Remove(options[i].ChildEventId);
                }
            }
        }

        private void PopulateDeck(EventData eventData) {
            if (!eventData.Enabled) {
                // Do not add disabled events
                return;
            }

            // Use forcedRarity if it was specified
            Rarity rarity = this.forcedRarity == Rarity.NULL ? Rarity.ConvertFromId(eventData.Rarity) :
                this.forcedRarity; 
            
            int weight = rarity.weight;
            for (int i = 0; i < weight; ++i) {
                this.deck.Add(new EventCard(eventData.IntId));
            }
        }

        public Maybe<int> ResolveNextEvent() {
            return this.selectionStrategy.SelectNextEvent(this.deck);
        }

        public bool HasMoreEvents {
            get {
                return this.deck.Count > 0;
            }
        }
    }
}