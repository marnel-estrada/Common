using System;

namespace GameEvent {
    public struct EventCard : IEquatable<EventCard> {
        // Points to an event ID
        public readonly int eventId;

        public EventCard(int eventId) {
            this.eventId = eventId;
        }

        public bool Equals(EventCard other) {
            return this.eventId == other.eventId;
        }

        public override bool Equals(object obj) {
            return obj is EventCard other && Equals(other);
        }

        public override int GetHashCode() {
            return this.eventId;
        }

        public static bool operator ==(EventCard left, EventCard right) {
            return left.Equals(right);
        }

        public static bool operator !=(EventCard left, EventCard right) {
            return !left.Equals(right);
        }
    }
}