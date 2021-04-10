using Common;

namespace Delver {
    /// <summary>
    /// A specialized layer that accepts a previous layer
    /// </summary>
    class ReluOutputLayer : OutputLayer {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="previousLayer"></param>
        /// <param name="outputNeuronCount"></param>
        public ReluOutputLayer(NeuronLayer previousLayer, int outputNeuronCount, float stepSize) : base(previousLayer, outputNeuronCount, stepSize) {
        }

        protected override void PrepareNeurons(int outputNeuronCount, float stepSize) {
            Assertion.IsTrue(outputNeuronCount > 0);
            for (int i = 0; i < outputNeuronCount; ++i) {
                Gate neuron = new ReluNeuron(this.PreviousLayer.NeuronCount, stepSize);
                neuron.Prepare();
                Add(neuron);
            }
        }
    }
}