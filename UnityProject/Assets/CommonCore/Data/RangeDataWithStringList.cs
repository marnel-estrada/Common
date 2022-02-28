using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common {
    [Serializable]
    public class RangeDataWithStringList {
        public int min;
        public int max;

        [SerializeField]
        public List<string>? stringList;
    }
}