using System.Text;

using Common;

namespace Delver {
    public class NeuronLayerAdapter : NeuronLayer {
        private readonly SimpleList<Gate> neurons = new SimpleList<Gate>();
        private float stepSize;

        public int NeuronCount {
            get {
                return this.neurons.Count;
            }
        }

        public Gate GetNeuronAt(int index) {
            return this.neurons[index];
        }

        public virtual void Prepare() {
        }

        /// <summary>
        /// Adds a neuron
        /// </summary>
        /// <param name="neuron"></param>
        protected void Add(Gate neuron) {
            this.neurons.Add(neuron);
        }

        public virtual void Forward() {
            for (int i = 0; i < this.neurons.Count; ++i) {
                this.neurons[i].Forward();
            }
        }

        public virtual void ResetGradients() {
            for (int i = 0; i < this.neurons.Count; ++i) {
                this.neurons[i].ResetGradients();
            }
        }

        public void Backward() {
            for (int i = this.neurons.Count - 1; i >= 0; --i) {
                this.neurons[i].Backward();
            }
        }
        
        public void Regularize() {
            for (int i = 0; i < this.neurons.Count; ++i) {
                this.neurons[i].Regularize();
            }
        }

        public void UpdateParameters() {
            for (int i = 0; i < this.neurons.Count; ++i) {
                this.neurons[i].UpdateWeights();
            }
        }

        public string AsString {
            get {
                StringBuilder builder = new StringBuilder();
                for(int i = 0; i < this.neurons.Count; ++i) {
                    builder.Append('N').Append(i).Append(": ").Append(this.neurons[i].ToString()).Append("; ");
                }

                return builder.ToString();
            }
        }

        public float RegularizationCost {
            get {
                float cost = 0;
                for(int i = 0; i < this.NeuronCount; ++i) {
                    cost += this.neurons[i].RegularizationCost;
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
                for(int i = 0; i < this.NeuronCount; ++i) {
                    this.neurons[i].StepSize = this.stepSize;
                }
            }
        }
    }
}
