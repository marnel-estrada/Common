using System.Text;

using Common;

namespace Delver {
    public class NeuralNetwork {
        private readonly GenericNeuronLayer inputLayer;

        private readonly SimpleList<NeuronLayer> layers = new SimpleList<NeuronLayer>();
        private float stepSize;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="inputLayer"></param>
        public NeuralNetwork(int inputCount, int inputLayerNeuronCount, float stepSize, bool rectified) {
            this.InputCount = inputCount;
            this.InputLayerNeuronCount = inputLayerNeuronCount;
            this.stepSize = stepSize;
            this.Rectified = rectified;

            // Prepare the input layer
            this.inputLayer = new GenericNeuronLayer(inputCount);
            for (int i = 0; i < inputLayerNeuronCount; ++i) {
                Gate neuron = ResolveInputNeuron(inputCount, rectified);
                neuron.Prepare();
                this.inputLayer.AddNeuron(neuron);
            }

            this.layers.Add(this.inputLayer);

            this.stepSize = stepSize;
        }

        /// <summary>
        ///     Instantiates a neural network that's a copy of the specified one
        ///     This is more like a copy constructor
        /// </summary>
        /// <param name="source"></param>
        public NeuralNetwork(NeuralNetwork source) : this(source.InputCount, source.InputLayerNeuronCount,
            source.StepSize, source.Rectified) {
            // Add output layers
            for (int i = 1; i < source.LayerCount; ++i) {
                AddOutputLayer(source.GetLayerAt(i).NeuronCount);
            }

            // Copy each layer
            for (int i = 0; i < source.LayerCount; ++i) {
                CopyLayer(source.GetLayerAt(i), GetLayerAt(i)); // Ourselves is the destination
            }
        }

        /// <summary>
        ///     Returns the last output layer
        ///     Usually used to set the gradients for back propagation
        /// </summary>
        public NeuronLayer LastOutputLayer {
            get {
                return this.layers[this.layers.Count - 1];
            }
        }

        public int LayerCount {
            get {
                return this.layers.Count;
            }
        }

        public float RegularizationCost {
            get {
                float cost = 0;
                for (int i = 0; i < this.layers.Count; ++i) {
                    cost += this.layers[i].RegularizationCost;
                }

                return cost;
            }
        }

        public float StepSize {
            get {
                return this.stepSize;
            }

            set {
                this.stepSize = value;

                // Set to all layers
                for (int i = 0; i < this.layers.Count; ++i) {
                    this.layers[i].StepSize = value;
                }
            }
        }

        public int InputCount { get; }

        public int InputLayerNeuronCount { get; }

        public bool Rectified { get; }

        private Gate ResolveInputNeuron(int inputCount, bool rectified) {
            if (rectified) {
                return new ReluNeuron(inputCount, this.stepSize, true);
            }

            return new LinearNeuron(inputCount, this.stepSize);
        }

        /// <summary>
        ///     Adds an output layer to the network with a specified neuron count
        /// </summary>
        /// <param name="neuronCount"></param>
        public void AddOutputLayer(int neuronCount) {
            Assertion.IsTrue(this.layers.Count > 0);

            NeuronLayer previousLayer = this.layers[this.layers.Count - 1]; // Last layer
            OutputLayer layer = new OutputLayer(previousLayer, neuronCount, this.stepSize);
            layer.Prepare();

            this.layers.Add(layer);
        }

        /// <summary>
        ///     Performs a forward pass using the specified input
        /// </summary>
        public void Forward(NeuralInput input) {
            Assertion.IsTrue(this.inputLayer.InputCount == input.Count);
            for (int i = 0; i < this.inputLayer.InputCount; ++i) {
                this.inputLayer.SetInputValue(i, input.GetAt(i));
            }

            // Forward each layer
            for (int i = 0; i < this.layers.Count; ++i) {
                this.layers[i].Forward();
            }
        }

        /// <summary>
        ///     Resets the gradients
        /// </summary>
        public void ResetGradients() {
            for (int i = this.layers.Count - 1; i >= 0; --i) {
                this.layers[i].ResetGradients();
            }
        }

        /// <summary>
        ///     Performs the learning routines
        ///     This assumes that Forward() was already invoked and target gradients have been set in the last output layer
        /// </summary>
        public void Learn() {
            // Back propagate in reverse
            for (int i = this.layers.Count - 1; i >= 0; --i) {
                this.layers[i].Backward();
            }

            // Regularize
            for (int i = this.layers.Count - 1; i >= 0; --i) {
                this.layers[i].Regularize();
            }

            // Update parameters
            for (int i = this.layers.Count - 1; i >= 0; --i) {
                this.layers[i].UpdateParameters();
            }
        }

        /// <summary>
        ///     Returns the layer at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public NeuronLayer GetLayerAt(int index) {
            return this.layers[index];
        }

        public override string ToString() {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < this.layers.Count; ++i) {
                builder.Append("Layer").Append(i).Append('{').Append(this.layers[i].AsString).Append('}').Append('\n');
            }

            return builder.ToString();
        }

        /// <summary>
        ///     Returns a new instance that's a copy of the specified NeuralNetwork
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static NeuralNetwork Copy(NeuralNetwork source) {
            NeuralNetwork copy = new NeuralNetwork(source.InputCount, source.InputLayerNeuronCount, source.StepSize,
                source.Rectified);

            // Add output layers
            for (int i = 1; i < source.LayerCount; ++i) {
                copy.AddOutputLayer(source.GetLayerAt(i).NeuronCount);
            }

            // Copy each layer
            for (int i = 0; i < source.LayerCount; ++i) {
                CopyLayer(source.GetLayerAt(i), copy.GetLayerAt(i)); // Ourselves is the destination
            }

            return copy;
        }

        private static void CopyLayer(NeuronLayer sourceLayer, NeuronLayer destinationLayer) {
            Assertion.IsTrue(sourceLayer.NeuronCount == destinationLayer.NeuronCount);

            for (int i = 0; i < sourceLayer.NeuronCount; ++i) {
                Gate source = sourceLayer.GetNeuronAt(i);
                Gate destination = destinationLayer.GetNeuronAt(i);
                destination.Weights = source.Weights;
            }
        }
    }
}