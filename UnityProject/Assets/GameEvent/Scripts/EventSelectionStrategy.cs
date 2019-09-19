namespace GameEvent {
    public interface EventSelectionStrategy {
        void Reset();
        
        int SelectNextEvent(EventDeck deck);
    }
}