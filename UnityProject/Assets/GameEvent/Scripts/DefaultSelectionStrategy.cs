using System.Collections.Generic;

using Common;

namespace GameEvent {
    public class DefaultSelectionStrategy : IEventSelectionStrategy {
        private readonly HashSet<EventCard> drawnCards = new HashSet<EventCard>();

        public void Reset() {
            this.drawnCards.Clear();
        }

        public Maybe<int> SelectNextEvent(EventDeck deck) {
            EventCard drawnCard;
            
            // Draw a card that has not been drawn yet
            do {
                if (deck.Count <= 0) {
                    // There are no more cards in the deck
                    return Maybe<int>.Nothing;
                }
                
                drawnCard = deck.Draw();
            } while (this.drawnCards.Contains(drawnCard));

            // We add here so it will not be drawn again
            this.drawnCards.Add(drawnCard);

            return new Maybe<int>(drawnCard.eventId);
        }

        public IEnumerable<EventCard> DrawnCards {
            get {
                return this.drawnCards;
            }
        }
    }
}