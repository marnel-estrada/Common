using Unity.Collections;

namespace CommonEcs.Goap {
    /// <summary>
    /// Wraps a NativeHashMap of texts used in GOAP. This is so we don't pass around the NativeHashMap
    /// when we only want to resolve the text from IDs.
    /// </summary>
    public struct GoapTextResolver {
        private NativeHashMap<int, FixedString64Bytes> textMap;

        public GoapTextResolver(in NativeHashMap<int, FixedString64Bytes> textMap) {
            this.textMap = textMap;
        }

        public FixedString64Bytes GetText(int id) {
            return this.textMap[id];
        }
    }
}