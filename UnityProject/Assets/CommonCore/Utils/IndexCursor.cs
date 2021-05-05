using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common {
    /// <summary>
    /// A utility class that handles int indexing
    /// </summary>
    public class IndexCursor {

        private readonly int count;
        private int currentIndex = 0;

        /// <summary>
        /// Constructor with specified count
        /// </summary>
        /// <param name="count"></param>
        public IndexCursor(int count) {
            this.count = count;
        }

        /// <summary>
        /// Resets the indexing
        /// </summary>
        public void Reset() {
            this.currentIndex = 0;
        }

        /// <summary>
        /// Moves to next index
        /// </summary>
        public void MoveNext() {
            this.currentIndex = (this.currentIndex + 1) % this.count;
        }

        /// <summary>
        /// Moves to the previous index
        /// </summary>
        public void MovePrevious() {
            int decremented = this.currentIndex - 1;
            this.currentIndex = decremented < 0 ? this.count - 1 : decremented;
        }

        public int CurrentIndex {
            get {
                return this.currentIndex;
            }

            set {
                Assertion.IsTrue(0 <= value && value < this.count);
                this.currentIndex = value;
            }
        }

    }
}
