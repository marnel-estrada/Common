using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common {
    /// <summary>
    /// A common interface to require classes to have an ID
    /// </summary>
    public interface Identifiable {

        /// <summary>
        /// Returns an ID that identifies the instance
        /// </summary>
        string Id { get; set; }

    }
}
