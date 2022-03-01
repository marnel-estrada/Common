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
    }
}