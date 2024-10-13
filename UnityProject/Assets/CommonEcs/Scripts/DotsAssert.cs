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
        
        public static void IsTrue(bool condition, in FixedString128Bytes message) {
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
        
        public static void IsFalse(bool condition, FixedString128Bytes message) {
#if UNITY_EDITOR
            if (condition) {
                throw new Exception(message.ToString());
            }
#endif          
        }

        public static void NotNullEntity(in Entity entity) {
#if UNITY_EDITOR
            IsTrue(entity != Entity.Null);
#endif
        }

        public static void IsSome<T>(in ValueTypeOption<T> option, FixedString128Bytes message = new()) 
            where T : struct, IEquatable<T> {
#if UNITY_EDITOR
            IsTrue(option.IsSome, message);
#endif
        }

        public static void IsNone<T>(in ValueTypeOption<T> option, FixedString128Bytes message = new()) 
            where T : struct, IEquatable<T> {
#if UNITY_EDITOR
            IsTrue(option.IsNone, message);
#endif      
        }
    }
}