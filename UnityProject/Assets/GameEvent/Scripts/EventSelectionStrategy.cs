using Common;

namespace GameEvent {
    public interface IEventSelectionStrategy {
        void Reset();
        
        Maybe<int> SelectNextEvent(EventDeck deck);
    }
}