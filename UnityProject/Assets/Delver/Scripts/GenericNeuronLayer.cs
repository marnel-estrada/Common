using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common.Utils;

namespace Delver {
    public class GenericNeuronLayer : NeuronLayerAdapter {

        private int inputCount;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inputCount"></param>
        public GenericNeuronLayer(int inputCount) {
            this.inputCount = inputCount;
        }

        /// <summary>
        /// Sets a value at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetInputValue(int index, float value) {
            for(int i = 0; i < this.NeuronCount; ++i) {
                this.GetNeuronAt(i).SetInputValue(index, value);
            }
        }

        /// <summary>
        /// Adds a neuron
        /// </summary>
        /// <param name="neuron"></param>
        public void AddNeuron(Gate neuron) {
            Add(neuron);
        }

        public int InputCount {
            get {
                return inputCount;
            }
        }

    }
}
