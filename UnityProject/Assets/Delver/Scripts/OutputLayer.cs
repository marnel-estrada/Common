using Common;

namespace Delver {
    /// <summary>
    /// A specialized layer that accepts a previous layer
    /// </summary>
    public class OutputLayer : NeuronLayerAdapter {
        private readonly NeuronLayer previousLayer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="previousLayer"></param>
        /// <param name="outputNeuronCount"></param>
        public OutputLayer(NeuronLayer previousLayer, int outputNeuronCount, float stepSize) {
            this.previousLayer = previousLayer;
            PrepareNeurons(outputNeuronCount, stepSize);
        }

        protected NeuronLayer PreviousLayer {
            get {
                return previousLayer;
            }
        }

        protected virtual void PrepareNeurons(int outputNeuronCount, float stepSize) {
            Assertion.Assert(outputNeuronCount > 0);
            for(int i = 0; i < outputNeuronCount; ++i) {
                Gate neuron = new LinearNeuron(this.previousLayer.NeuronCount, stepSize);
                neuron.Prepare();
                Add(neuron);
            }
        }

        public override void Prepare() {
            // Use the previous layer's forward units as input to this layer
            for(int i = 0; i < this.previousLayer.NeuronCount; ++i) {
                Unit forwardUnit = this.previousLayer.GetNeuronAt(i).ForwardUnit;
                SetInputUnit(i, forwardUnit);
            }
        }

        private void SetInputUnit(int index, Unit unit) {
            for(int i = 0; i < this.NeuronCount; ++i) {
                this.GetNeuronAt(i).SetInput(index, unit);
            }
        }
    }
}
