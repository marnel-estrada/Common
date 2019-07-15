namespace Delver {
    public class Unit {
        private float value;
        private float gradient;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Unit() {
        }

        /// <summary>
        /// Constructor with specified value only
        /// </summary>
        /// <param name="value"></param>
        public Unit(float value) : this(value, 0) {
        }

        /// <summary>
        /// Constructor with specified value and gradient
        /// </summary>
        /// <param name="value"></param>
        /// <param name="gradient"></param>
        public Unit(float value, float gradient) {
            this.value = value;
            this.gradient = gradient;
        }

        public float Value {
            get {
                return value;
            }

            set {
                this.value = value;
            }
        }

        public float Gradient {
            get {
                return gradient;
            }

            set {
                this.gradient = value;
            }
        }
    }
}
