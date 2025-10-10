using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common {
    [Serializable]
    public class RangeDataWithStringList {
        public float min;
        public float max;

        [SerializeField]
        public List<string>? stringList;

        public RangeDataWithStringList CreateCopy() {
            RangeDataWithStringList copy = new() {
                min = this.min,
                max = this.max
            };

            if (this.stringList != null) {
                copy.stringList = new List<string>(this.stringList);
            }

            return copy;
        }
    }
}