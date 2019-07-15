namespace Delver {
    /// <summary>
    /// The gate interface
    /// </summary>
    public interface Gate {
        /// <summary>
        /// Number of required inputs
        /// </summary>
        int InputCount { get; }

        /// <summary>
        /// Sets an input Unit to the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="unit"></param>
        void SetInput(int index, Unit unit);

        /// <summary>
        /// Returns the input at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Unit GetInput(int index);

        /// <summary>
        /// Sets the input value at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        void SetInputValue(int index, float value);

        /// <summary>
        /// Prepares the gate
        /// Note that a gate may use other gates
        /// </summary>
        void Prepare();

        /// <summary>
        /// Resets the gradients of the units in the gate
        /// </summary>
        void ResetGradients();

        /// <summary>
        /// Performs the forward pass
        /// </summary>
        /// <returns></returns>
        void Forward();

        /// <summary>
        /// Performs the backward pass
        /// Gradients are stored in the Units added to it
        /// </summary>
        void Backward();

        /// <summary>
        /// Regularization routine
        /// </summary>
        void Regularize();

        /// <summary>
        /// Updates the weight/parameters
        /// </summary>
        void UpdateWeights();

        /// <summary>
        /// Returns the forward unit of the gate
        /// </summary>
        Unit ForwardUnit { get; }

        /// <summary>
        /// Returns the regularization cost of the gate
        /// </summary>
        float RegularizationCost { get; }

        /// <summary>
        /// Property for the step size or the learning rate
        /// It may be updated during the training
        /// </summary>
        float StepSize { get; set; }

        /// <summary>
        /// Getter/Setter for the weights of the gate
        /// </summary>
        float[] Weights { get; set; }
    }
}
