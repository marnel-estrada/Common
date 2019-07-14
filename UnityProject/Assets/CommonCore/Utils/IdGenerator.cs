using UnityEngine;

namespace Common {
    /// <summary>
    /// A utility class that manages int IDs 
    /// </summary>
    public class IdGenerator {
        private const int DEFAULT_STARTING_ID = 1;
        
        private int counter = DEFAULT_STARTING_ID;

        /// <summary>
        /// Constructor
        /// </summary>
        public IdGenerator() {
        }

        /// <summary>
        /// Constructor with specified starting ID
        /// </summary>
        /// <param name="startingId"></param>
        public IdGenerator(int startingId) {
            this.counter = startingId;
        }

        /// <summary>
        /// Generates a new id
        /// </summary>
        /// <returns></returns>
        public int Generate() {
            int newId = this.counter;
            ++this.counter;

            return newId;
        }

        /// <summary>
        /// Saves the generated ID such that it won't generate the said id
        /// </summary>
        /// <param name="id"></param>
        public void SaveGenerated(int id) {
            this.counter = Mathf.Max(id + 1, this.counter);
        }

        /// <summary>
        /// Resets the generator
        /// </summary>
        public void Reset() {
            this.counter = DEFAULT_STARTING_ID;
        }
    }
}
