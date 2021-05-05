using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common {
    /// <summary>
    /// A wrapper class for a certain result T
    /// Usually used to wait for an asynchronous result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Result<T> {

        private bool assigned = false;
        private T value = default(T);

        /// <summary>
        /// Constructor
        /// </summary>
        public Result() {
            Reset();
        }

        public void Reset() {
            this.assigned = false;
            this.value = default(T);
        }

        public bool Assigned {
            get {
                return this.assigned;
            }
        }

        public T Value {
            get {
                return this.value;
            }

            set {
                this.value = value;
                this.assigned = true;
            }
        }

    }
}
