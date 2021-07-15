using System;

namespace Common {
    [Serializable]
    public struct StringCountData {
        public string stringId;
        public int count;

        public StringCountData(string stringId, int count) {
            this.stringId = stringId;
            this.count = count;
        }
    }
}