using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Common {
    /// <summary>
    /// Extension methods related to Components
    /// </summary>
    public static class ComponentExtensions {

        /// <summary>
        /// Retrieves a required component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static T GetRequiredComponent<T>(this Component self) where T : Component {
            T componentInstance = self.GetComponent<T>();
            Assertion.AssertNotNull(componentInstance);
            return componentInstance;
        }

    }
}
