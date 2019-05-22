using System;

namespace CommonEcs {
    /// <summary>
    /// A blittable boolean that is internally implemented as a byte
    /// </summary>
    public struct ByteBool : IEquatable<ByteBool> {
        private byte value;
    
        /// <summary>
        /// Constructor for custom initial value
        /// </summary>
        /// <param name="value"></param>
        public ByteBool(bool value) {
            this.value = (byte)(value ? 1 : 0);
        }
    
        public bool Value {
            get {
                return this.value != 0;
            }
    
            set {
                this.value = (byte)(value ? 1 : 0);
            }
        }
    
        public bool Equals(ByteBool other) {
            return this.value == other.value;
        }
    
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false; 
            }
    
            return obj is ByteBool && Equals((ByteBool) obj);
        }
    
        public override int GetHashCode() {
            return this.value.GetHashCode();
        }
        
        /// <summary>
        /// Converts a bool to a ByteBool
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator ByteBool(bool value) {
            return new ByteBool(value);
        }
        
        /// <summary>
        /// Converts a ByteBool to a bool
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static implicit operator bool(ByteBool source) {
            return source.Value;
        }   
    }
}
