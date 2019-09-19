using System.Collections.Generic;

namespace GameEvent {
    public class DefaultSelectionStrategy : EventSelectionStrategy {
        private readonly HashSet<EventCard> drawnCards = new HashSet<EventCard>();

        public void Reset() {
            this.drawnCards.Clear();
        }

        public int SelectNextEvent(EventDeck deck) {
            EventCard drawnCard;
            
            // Draw a card that has not been drawn yet
            do {
                drawnCard = deck.Draw();
            } while (this.drawnCards.Contains(drawnCard));

            // We add here so it will not be drawn again
            this.drawnCards.Add(drawnCard);

            return drawnCard.eventId;
        }

        public IEnumerable<EventCard> DrawnCards {
            get {
                return this.drawnCards;
            }
        }
    }
}