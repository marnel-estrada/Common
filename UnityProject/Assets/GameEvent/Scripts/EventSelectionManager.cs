using Common;

namespace GameEvent {
    public class EventSelectionManager {
        private readonly EventsPool pool; // The main pool of events
        private readonly IEventSelectionStrategy selectionStrategy;
        
        private readonly EventDeck deck = new EventDeck();

        public EventSelectionManager(EventsPool pool, IEventSelectionStrategy selectionStrategy) {
            this.pool = pool;
            this.selectionStrategy = selectionStrategy ?? new DefaultSelectionStrategy();
        }

        public void Reset() {
            this.deck.Clear();
            this.selectionStrategy.Reset();

            foreach (EventData eventData in this.pool.GetAll()) {
                PopulateDeck(eventData);
            }
            
            this.deck.Shuffle();
        }

        private void PopulateDeck(EventData eventData) {
            if (!eventData.Enabled) {
                // Do not add disabled events
                return;
            }
            
            int weight = Rarity.ConvertFromId(eventData.Rarity).weight;
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