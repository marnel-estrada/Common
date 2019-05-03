using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common {
    /// <summary>
    /// Common interface for objects that can be recycled
    /// </summary>
    public interface IRecyclable {

        /// <summary>
        /// Recycles the object
        /// </summary>
        void Recycle();

    }
}
