using System.Collections.Generic;

namespace GameEvent {
    public class DefaultSelectionStrategy : EventSelectionStrategy {
        private readonly EventDeck deck;
        private readonly HashSet<EventCard> drawnCards = new HashSet<EventCard>();

        public DefaultSelectionStrategy(EventDeck deck) {
            this.deck = deck;
        }

        public void Reset() {
            this.drawnCards.Clear();
        }

        public int SelectNextEvent() {
            EventCard drawnCard;
            
            // Draw a card that has not been drawn yet
            do {
                drawnCard = this.deck.Draw();
            } while (this.drawnCards.Contains(drawnCard));

            // We add here so it will not be drawn again
            this.drawnCards.Add(drawnCard);

            return drawnCard.eventId;
        }
    }
}