using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common {
    public interface GridFilter {

        /// <summary>
        /// Returns whether or not the specified row passes the filter
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        bool Passed(GridRow row);

    }
}
