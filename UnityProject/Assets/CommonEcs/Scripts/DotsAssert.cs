using System;

using Unity.Collections;

namespace CommonEcs {
    public struct DotsAssert {
        public static void IsTrue(bool condition, FixedString128 message = new FixedString128()) {
            if (!condition) {
                throw new Exception(message.ToString());
            }
        }
        
        public static void IsFalse(bool condition, FixedString128 message = new FixedString128()) {
            if (condition) {
                throw new Exception(message.ToString());
            }
        }
    }
}