using Common;

namespace Delver {
    /// <summary>
    /// Class that represents an input to a neural network
    /// </summary>
    public class NeuralInput {

        private SimpleList<float> inputs = new SimpleList<float>();

        /// <summary>
        /// Constructor
        /// </summary>
        public NeuralInput() {
        }

        /// <summary>
        /// Clears all input
        /// </summary>
        public void Clear() {
            this.inputs.Clear();
        }

        /// <summary>
        /// Adds an input
        /// </summary>
        /// <param name="input"></param>
        public void Add(float input) {
            this.inputs.Add(input);
        }

        /// <summary>
        /// Returns the number of inputs
        /// </summary>
        public int Count {
            get {
                return this.inputs.Count;
            }
        }

        /// <summary>
        /// Returns the input at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public float GetAt(int index) {
            return this.inputs[index];
        }

    }
}
