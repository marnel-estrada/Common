using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Common;

namespace Common {
    /// <summary>
    /// The object returned when a mesh is added to a combined mesh
    /// This is used to set some parts of the combined mesh like UV
    /// </summary>
    public class MeshPartHandle {

        private int startIndex;
        private readonly int vertexCount;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vertexCount"></param>
        public MeshPartHandle(int vertexCount) {
            this.vertexCount = vertexCount;
        }

        public int VertexCount {
            get {
                return this.vertexCount;
            }
        }

        public int StartIndex {
            get {
                return this.startIndex;
            }

            set {
                this.startIndex = value;
            }
        }

    }
}
