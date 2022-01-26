using System;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    public struct DotsAssert {
        /// <summary>
        /// We provided an overload instead of default value so that the method will not create
        /// dummy FixedString128 for empty messages
        /// </summary>
        /// <param name="condition"></param>
        /// <exception cref="Exception"></exception>
        public static void IsTrue(bool condition) {
#if UNITY_EDITOR
            if (!condition) {
                throw new Exception();
            }
#endif
        }
        
        public static void IsTrue(bool condition, in FixedString128 message) {
#if UNITY_EDITOR
            if (!condition) {
                throw new Exception(message.ToString());
            }
#endif
        }
        
        /// <summary>
        /// We provided an overload instead of default value so that the method will not create
        /// dummy FixedString128 for empty messages
        /// </summary>
        /// <param name="condition"></param>
        /// <exception cref="Exception"></exception>
        public static void IsFalse(bool condition) {
#if UNITY_EDITOR
            if (condition) {
                throw new Exception();
            }
#endif          
        }
        
        public static void IsFalse(bool condition, FixedString128 message) {
#if UNITY_EDITOR
            if (condition) {
                throw new Exception(message.ToString());
            }
#endif          
        }

        public static void NotNullEntity(in Entity entity) {
            IsTrue(entity != Entity.Null);
        }

        public static void IsSome<T>(in ValueTypeOption<T> option, FixedString128 message = new FixedString128()) where T : struct, IEquatable<T> {
            IsTrue(option.IsSome, message);
        }
    }
}