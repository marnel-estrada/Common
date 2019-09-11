namespace GameEvent {
    public class EventSelectionManager {
        private readonly EventsPool pool; // The main pool of events
        private readonly EventSelectionStrategy selectionStrategy;
        
        private readonly EventDeck deck = new EventDeck();

        public EventSelectionManager(EventsPool pool, EventSelectionStrategy selectionStrategy) {
            this.pool = pool;
            this.selectionStrategy = selectionStrategy ?? new DefaultSelectionStrategy(this.deck);
        }

        public EventDeck Deck {
            get {
                return this.deck;
            }
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
            int weight = Rarity.ConvertFromId(eventData.Rarity).weight;
            for (int i = 0; i < weight; ++i) {
                this.deck.Add(new EventCard(eventData.IntId));
            }
        }

        public int ResolveNextEvent() {
            return this.selectionStrategy.SelectNextEvent();
        }
    }
}