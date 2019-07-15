namespace Delver {
    abstract class GateAdapter : Gate {
        private readonly Unit[] inputs;
        private readonly Unit forwardUnit = new Unit();

        private float stepSize;

        /// <summary>
        /// Default constructor
        /// </summary>
        public GateAdapter(int inputCount) {
            this.inputs = new Unit[inputCount];
            for(int i = 0; i < this.inputs.Length; ++i) {
                this.inputs[i] = new Unit();
            }
        }

        public virtual void SetInput(int index, Unit unit) {
            this.inputs[index] = unit;
        }

        public virtual Unit GetInput(int index) {
            return this.inputs[index];
        }

        public virtual void SetInputValue(int index, float value) {
            this.inputs[index].Value = value;

            // Note here that we reset the gradient every time a value is set
            this.inputs[index].Gradient = 0; 
        }

        /// <summary>
        /// May or may not be overriden
        /// </summary>
        public virtual void Prepare() {
        }

        public virtual void ResetGradients() {
            for(int i = 0; i < this.inputs.Length; ++i) {
                this.inputs[i].Gradient = 0;
            }

            this.forwardUnit.Gradient = 0;
        }

        public abstract void Forward();

        public abstract void Backward();

        public virtual void Regularize() {
        }

        public virtual void UpdateWeights() {
        }

        public virtual Unit ForwardUnit {
            get {
                return this.forwardUnit;
            }
        }

        public int InputCount {
            get {
                return this.inputs.Length;
            }
        }

        public virtual float RegularizationCost {
            get {
                return 0;
            }
        }

        public virtual float StepSize {
            get {
                return this.stepSize;
            }

            set {
                this.stepSize = value;
            }
        }

        public virtual float[] Weights {
            get {
                return null;
            }

            set {
                // None
            }
        }

        /// <summary>
        /// Returns the input at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected Unit GetInputAt(int index) {
            return this.inputs[index];
        }

    }
}
