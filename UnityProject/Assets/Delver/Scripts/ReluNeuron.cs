using UnityEngine;

namespace Delver {
    /// <summary>
    /// Rectified Linear Unit
    /// It's basically the same as LinearNeuron except for a little difference in forward and backward functions
    /// </summary>
    class ReluNeuron : GateAdapter {
        private readonly LinearNeuron linearNeuron;
        private readonly bool leaky;

        /// <summary>
        /// Constructor
        /// Defaults to not leaky
        /// </summary>
        /// <param name="inputCount"></param>
        /// <param name="stepSize"></param>
        public ReluNeuron(int inputCount, float stepSize) : this(inputCount, stepSize, false) {
        }

        /// <summary>
        /// Constructor with specification for leaky or not
        /// </summary>
        /// <param name="inputCount"></param>
        /// <param name="stepSize"></param>
        /// <param name="leaky"></param>
        public ReluNeuron(int inputCount, float stepSize, bool leaky) : base(inputCount) {
            this.linearNeuron = new LinearNeuron(inputCount, stepSize);
            this.leaky = leaky;
        }

        public override void Prepare() {
            this.linearNeuron.Prepare();
        }

        public override void SetInput(int index, Unit unit) {
            this.linearNeuron.SetInput(index, unit);
        }

        public override Unit GetInput(int index) {
            return this.linearNeuron.GetInput(index);
        }

        public override void SetInputValue(int index, float value) {
            this.linearNeuron.SetInputValue(index, value);
        }

        public override void Forward() {
            //Debug.Log("Relu Neuron");
            this.linearNeuron.Forward();

            if (this.leaky) {
                this.ForwardUnit.Value = Mathf.Max(this.linearNeuron.ForwardUnit.Value * 0.01f, this.linearNeuron.ForwardUnit.Value);
            } else {
                this.ForwardUnit.Value = Mathf.Max(0, this.linearNeuron.ForwardUnit.Value);
            }
        }

        public override void ResetGradients() {
            base.ResetGradients();
            this.linearNeuron.ResetGradients();
        }

        public override void Backward() {
            if(Comparison.IsZero(this.linearNeuron.ForwardUnit.Value)) {
                // Value is zero. We use zero pull on the whole neuron
                this.ForwardUnit.Gradient = 0;
                this.linearNeuron.ForwardUnit.Gradient = 0;
            } else {
                // Use whatever gradient is assigned from the top
                this.linearNeuron.ForwardUnit.Gradient = this.ForwardUnit.Gradient;
            }

            this.linearNeuron.Backward();
        }

        public override void Regularize() {
            base.Regularize();
            this.linearNeuron.Regularize();
        }

        public override void UpdateWeights() {
            base.UpdateWeights();
            this.linearNeuron.UpdateWeights();
        }

        public override float RegularizationCost {
            get {
                return this.linearNeuron.RegularizationCost;
            }
        }

        public override string ToString() {
            return this.linearNeuron.ToString();
        }

        public override float StepSize {
            get {
                return this.linearNeuron.StepSize;
            }

            set {
                this.linearNeuron.StepSize = value;
            }
        }
    }
}
