using Common;

namespace GameEvent {
    public class EventDeck {
        private readonly SimpleList<EventCard> cards = new SimpleList<EventCard>(100);

        public void Clear() {
            this.cards.Clear();
        }

        public void Add(EventCard card) {
            this.cards.Add(card);
        }

        // This is like a pop
        public EventCard Draw() {
            // There should be cards
            Assertion.IsTrue(this.cards.Count > 0);
            
            int lastIndex = this.cards.Count - 1;
            EventCard top = this.cards[lastIndex];
            this.cards.RemoveAt(lastIndex);
            return top;
        }

        public void Shuffle() {
            // Just swap multiple times
            int cardsCount = this.cards.Count;
            int swapCount = cardsCount * 4;
            for (int i = 0; i < swapCount; ++i) {
                int a = UnityEngine.Random.Range(0, cardsCount);
                int b = UnityEngine.Random.Range(0, cardsCount);
                this.cards.Swap(a, b);
            }
        }

        public int Count {
            get {
                return this.cards.Count;
            }
        }
    }
}