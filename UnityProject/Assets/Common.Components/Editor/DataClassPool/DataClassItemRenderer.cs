using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common;
using Common.Utils;

namespace Common {
    /// <summary>
    /// Interface for classes that render the items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface DataClassItemRenderer<T> where T : Identifiable, new() {

        /// <summary>
        /// Renders the item
        /// </summary>
        /// <param name="item"></param>
        void Render(DataClassPool<T> pool, T item);

    }
}
