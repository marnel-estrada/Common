using Common;

namespace GameEvent {
    /// <summary>
    /// The instance used during event selection
    /// This will hold the actual Requirement classes
    /// </summary>
    public class EventInstance {
        private readonly EventData data;
        
        private SimpleList<Requirement> requirements = new SimpleList<Requirement>(1);
    }
}