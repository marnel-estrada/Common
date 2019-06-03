using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Common;

namespace Common {
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyGroup : Attribute {

        private readonly string name;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public PropertyGroup(string name) {
            this.name = name;
        }

        public string Name {
            get {
                return name;
            }
        }

    }
}
