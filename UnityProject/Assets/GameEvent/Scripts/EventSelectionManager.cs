namespace GameEvent {
    public class EventSelectionManager {
        private readonly EventsPool pool; // The main pool of events
        private readonly EventSelectionStrategy defaultSelectionStrategy;
        private readonly EventSelectionStrategy customSelectionStrategy;
        
        private readonly EventDeck deck = new EventDeck();

        public EventSelectionManager(EventsPool pool, EventSelectionStrategy selectionStrategy) {
            this.pool = pool;
            this.defaultSelectionStrategy = new DefaultSelectionStrategy();
            this.customSelectionStrategy = selectionStrategy ?? this.defaultSelectionStrategy;
        }

        public EventSelectionStrategy DefaultSelectionStrategy {
            get {
                return this.defaultSelectionStrategy;
            }
        }

        public void Reset() {
            this.deck.Clear();
            this.defaultSelectionStrategy.Reset();
            this.customSelectionStrategy.Reset();

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
            return this.customSelectionStrategy.SelectNextEvent(this.deck);
        }
    }
}