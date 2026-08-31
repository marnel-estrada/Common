using System;
using UnityEngine;

namespace Common {
    [Serializable]
    public struct PrebuiltAtlasEntry {
        public string name;
        public Rect uvRect;        // normalized
        public int originalWidth;
        public int originalHeight;
        public int uvIndex;
    }
}
