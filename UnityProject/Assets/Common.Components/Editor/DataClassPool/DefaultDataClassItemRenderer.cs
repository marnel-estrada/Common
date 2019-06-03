using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using Common;
using Common.Utils;

namespace Common {
    /// <summary>
    /// The generic renderer for an item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultDataClassItemRenderer<T> : DataClassItemRenderer<T> where T : Identifiable, new() {

        private GenericObjectRenderer renderer = new GenericObjectRenderer(typeof(T));

        /// <summary>
        /// Constructor
        /// </summary>
        public DefaultDataClassItemRenderer() {
        }

        /// <summary>
        /// Mey be overridden by deriving class to implement custom rendering
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="item"></param>
        public virtual void Render(DataClassPool<T> pool, T item) {
            this.renderer.Render(item);
        }

    }
}
