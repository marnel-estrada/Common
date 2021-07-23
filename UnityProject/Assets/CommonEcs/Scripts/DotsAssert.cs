using System;

using Unity.Collections;

namespace CommonEcs {
    public struct DotsAssert {
        public static void IsTrue(bool condition, FixedString128 message = new FixedString128()) {
            if (!condition) {
#if UNITY_EDITOR
                throw new Exception(message.ToString());
#endif
            }
        }
        
        public static void IsFalse(bool condition, FixedString128 message = new FixedString128()) {
            if (condition) {
#if UNITY_EDITOR
                throw new Exception(message.ToString());
#endif                
            }
        }
    }
}