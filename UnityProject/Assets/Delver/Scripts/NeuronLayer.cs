using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Delver {
    /// <summary>
    /// Common interface for neuron layers
    /// </summary>
    public interface NeuronLayer {

        /// <summary>
        /// Returns the number of neurons in the layer
        /// </summary>
        int NeuronCount { get; }

        /// <summary>
        /// Returns the neuron at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Gate GetNeuronAt(int index);

        /// <summary>
        /// Prepares the layer
        /// </summary>
        void Prepare();

        /// <summary>
        /// Resets the gradients
        /// </summary>
        void ResetGradients();

        /// <summary>
        /// Performs a forward pass
        /// </summary>
        void Forward();

        /// <summary>
        /// Performs a backward pass
        /// </summary>
        void Backward();

        /// <summary>
        /// Regularization routine
        /// </summary>
        void Regularize();

        /// <summary>
        /// Updates the weight/parameters
        /// </summary>
        void UpdateParameters();

        /// <summary>
        /// Returns the regularization cost of the layer
        /// </summary>
        float RegularizationCost { get; }

        /// <summary>
        /// Sets the step size to all neurons in the layer
        /// </summary>
        float StepSize { get; set; }

        /// <summary>
        /// Returns the string representation of the layer
        /// </summary>
        string AsString { get; }

    }
}
