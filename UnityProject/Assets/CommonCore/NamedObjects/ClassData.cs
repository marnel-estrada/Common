using System;
using System.Collections.Generic;

using UnityEngine;

namespace Common {
    /// <summary>
    /// This is usually used in editors to describe a class.
    /// </summary>
    [Serializable]
    public class ClassData {
        [SerializeField]
        private string className;

        [SerializeField]
        private NamedValueLibrary variables;

        // used in editor only
        [SerializeField]
        [HideInInspector]
        private bool showHints;

        // used by editor for caching its type so it won't resolve it in every render
        private Type classType;

        /// <summary>
        /// Constructor
        /// </summary>
        public ClassData() {
            this.variables = new NamedValueLibrary();
        }

        public string ClassName {
            get {
                return this.className;
            }

            set {
                this.className = value;
            }
        }

        public NamedValueLibrary Variables {
            get {
                return this.variables;
            }
        }

        public Type ClassType {
            get {
                return classType;
            }

            set {
                this.classType = value;
            }
        }

        public bool ShowHints {
            get {
                return showHints;
            }

            set {
                this.showHints = value;
            }
        }

        /// <summary>
        /// Copies the contents of this instance to the specified one
        /// </summary>
        /// <param name="other"></param>
        public void CopyTo(ClassData other) {
            other.ClassName = this.ClassName;
            this.variables.CopyTo(other.variables); // note here that the class has privilege access to the variable even if it's private

            other.ShowHints = this.ShowHints;

            // not important but let's copy anyway to optimize editor
            other.ClassType = this.ClassType;
        }
        
        /// <summary>
        /// Copies a list of ClassData
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void Copy(List<ClassData> source, List<ClassData> destination) {
            destination.Clear();

            for (int i = 0; i < source.Count; ++i) {
                ClassData dataCopy = new ClassData();
                source[i].CopyTo(dataCopy);
                
                destination.Add(dataCopy);
            }
        }
    }
}
