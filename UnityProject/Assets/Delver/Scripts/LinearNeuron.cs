using System.Text;

using UnityEngine;

using Common;

namespace Delver {
    /// <summary>
    /// This neuron encapsulates ax + by + ... + c
    /// </summary>
    class LinearNeuron : GateAdapter {
        private readonly int inputCount;

        // This represents the a, b, c, ... of the equation
        private readonly SimpleList<Unit> weights = new SimpleList<Unit>();

        private readonly SimpleList<Gate> multiplyGates = new SimpleList<Gate>();
        private readonly SimpleList<Gate> addGates = new SimpleList<Gate>();

        /// <summary>
        /// Constructor with input count
        /// </summary>
        /// <param name="inputCount"></param>
        public LinearNeuron(int inputCount, float stepSize, float standardDeviationScale = 1) : base(inputCount) {
            this.inputCount = inputCount;
            Assertion.IsTrue(this.inputCount > 0);

            this.StepSize = stepSize;
            Assertion.IsTrue(this.StepSize > 0);

            // Used for initial parameters
            float standardDeviation = standardDeviationScale / Mathf.Sqrt(this.inputCount);

            // Populate the parameters
            // Note that we add one more for the last factor that is not paired with an input
            for(int i = 0; i < this.inputCount; ++i) {
                Unit unit = new Unit();
                unit.Value = Gaussian.NextValue(0, standardDeviation);
                this.weights.Add(unit); 
            }

            // Add the bias parameter
            // Note here that we are using standard deviation of 1 instead that of the biases
            {
                Unit unit = new Unit();
                unit.Value = Gaussian.NextValue(0, standardDeviationScale);
                this.weights.Add(unit);
            }
        }

        private const int WEIGHT_INDEX = 0;

        public override void Prepare() {
            Assertion.IsTrue(this.InputCount == this.inputCount);

            // prepare multiply gates
            for(int i = 0; i < this.inputCount; ++i) {
                Unit parameter = this.weights[i];

                Gate multiply = new MultiplyGate();
                multiply.SetInput(WEIGHT_INDEX, parameter); // input at 0 is the parameter, input at 1 will be the neural input
                this.multiplyGates.Add(multiply);
            }

            // prepare add gates
            Gate lastAddGate = null;
            for(int i = 0; i < this.inputCount - 1; ++i) {
                Unit a = this.multiplyGates[i].ForwardUnit;
                if(lastAddGate != null) {
                    a = lastAddGate.ForwardUnit;
                }

                Unit b = this.multiplyGates[i + 1].ForwardUnit;

                lastAddGate = PrepareAdditionGate(a, b);
            }

            // Add the last factor which is the last parameter
            if (this.inputCount == 1) {
                // There's only one multiply gate when input count is 1
                PrepareAdditionGate(this.multiplyGates[0].ForwardUnit, this.weights[this.weights.Count - 1]);
            } else {
                PrepareAdditionGate(lastAddGate.ForwardUnit, this.weights[this.weights.Count - 1]);
            }

            Assertion.IsTrue(this.addGates.Count == this.inputCount);
        }

        private Gate PrepareAdditionGate(Unit a, Unit b) {
            Gate gate = new AddGate();
            gate.SetInput(0, a);
            gate.SetInput(1, b);

            this.addGates.Add(gate);
            return gate;
        }

        public override void Backward() {
            Assertion.IsTrue(this.weights[0].Gradient < 10000, "Gradient is too large. You might have forgotten to reset them.");

            // Addition first
            Backward(this.addGates);

            // Then multiplication
            Backward(this.multiplyGates);
        }

        private static void Backward(SimpleList<Gate> gateList) {
            for(int i = gateList.Count - 1; i >= 0; --i) {
                gateList[i].Backward();
            }
        }

        private const int INPUT_INDEX = 1;

        public override void Forward() {
            // multiplication first
            // Set the values from the input first
            for(int i = 0; i < this.multiplyGates.Count; ++i) {
                // Note that we don't just set the value here
                // We set the unit itself
                // These units may come from another layer so they need their gradients to be updated as well
                Unit input = GetInputAt(i);
                this.multiplyGates[i].SetInput(INPUT_INDEX, input); // Note that 0 is the parameter, 1 is the input
                this.multiplyGates[i].Forward();
            }

            // then addition
            for (int i = 0; i < this.addGates.Count; ++i) {
                this.addGates[i].Forward();
            }
        }

        public override Unit ForwardUnit {
            get {
                // The forward unit is the last of addition
                return this.addGates[this.addGates.Count - 1].ForwardUnit;
            }
        }

        /// <summary>
        /// Updates the parameters based on their gradient
        /// </summary>
        public override void UpdateWeights() {
            for(int i = 0; i < this.weights.Count; ++i) {
                Unit unit = this.weights[i];
                unit.Value += this.StepSize * unit.Gradient;
            }
        }

        public int ParameterCount {
            get {
                return this.weights.Count;
            }
        }

        public Unit GetParameterAt(int index) {
            return this.weights[index];
        }

        /// <summary>
        /// Resets the gradients of the parameters of the neuron
        /// </summary>
        public override void ResetGradients() {
            // Reset gradients of inputs too
            // The inputs may be from a hidden layer
            base.ResetGradients();

            for(int i = 0; i < this.weights.Count; ++i) {
                this.weights[i].Gradient = 0;
            }

            // Reset gradients of multiplication and addition units
            for(int i = 0; i < this.multiplyGates.Count; ++i) {
                this.multiplyGates[i].ResetGradients();
            }

            for (int i = 0; i < this.addGates.Count; ++i) {
                this.addGates[i].ResetGradients();
            }
        }

        /// <summary>
        /// Adds the regularization pull to the parameters except for the last factor
        /// </summary>
        public override void Regularize() {
            for(int i = 0; i < this.weights.Count - 1; ++i) {
                this.weights[i].Gradient += -this.weights[i].Value;
            }
        }

        /// <summary>
        /// Prints the parameters for debugging purposes
        /// </summary>
        public void PrintParameters() {
            Debug.Log("Parameters");
            for(int i = 0; i < this.weights.Count; ++i) {
                Debug.Log(i + ": " + this.weights[i].Value);
            }
        }

        /// <summary>
        /// Prints the equation
        /// Assumes that Forward() has been invoked
        /// </summary>
        public void PrintEquation() {
            StringBuilder builder = new StringBuilder();

            for(int i = 0; i < this.multiplyGates.Count; ++i) {
                Unit parameter = this.multiplyGates[i].GetInput(WEIGHT_INDEX);
                Unit input = this.multiplyGates[i].GetInput(INPUT_INDEX);
                builder.Append("(").Append(parameter.Value).Append(" * ").Append(input.Value).Append(") + ");
            }

            // Add the last factor
            builder.Append(this.weights[this.weights.Count - 1].Value);

            // Add the result
            builder.Append(" = ").Append(this.ForwardUnit.Value);

            Debug.Log(builder.ToString());
        }

        public override string ToString() {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < this.multiplyGates.Count; ++i) {
                builder.Append(this.multiplyGates[i]).Append(" + ");
            }

            // Add the last factor
            builder.Append(this.weights[this.weights.Count - 1].Value);

            return builder.ToString();
        }

        /// <summary>
        /// Regularization cost is the sum of squares of parameters/weights
        /// Note here that we don't include the bias which is the last extra parameter
        /// </summary>
        public override float RegularizationCost {
            get {
                float cost = 0;

                for(int i = 0; i < this.inputCount; ++i) {
                    cost += this.weights[i].Value * this.weights[i].Value;
                }

                return cost;
            }
        }

        public override float[] Weights {
            get {
                float[] weightsAsArray = new float[this.weights.Count];
                for(int i = 0; i < this.weights.Count; ++i) {
                    weightsAsArray[i] = this.weights[i].Value;
                }

                return weightsAsArray;
            }

            set {
                // Set the weights
                Assertion.IsTrue(value.Length == this.weights.Count);
                for(int i = 0; i < value.Length; ++i) {
                    this.weights[i].Value = value[i];
                }
            }
        }
    }
}
