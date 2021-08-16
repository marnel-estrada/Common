using System;

using Unity.Collections;

namespace Common {
    /// <summary>
    /// Wraps a NativeHashMap so we don't pass it around as is
    /// </summary>
    public struct PackedTextureEntryResolver {
        // Integer is used as key here to save memory and for requesting code to be able to use
        // integer
        private NativeHashMap<int, PackedTextureEntry> entriesMap;

        public PackedTextureEntryResolver(NativeHashMap<int, PackedTextureEntry> entriesMap) {
            this.entriesMap = entriesMap;
        }

        public PackedTextureEntry GetEntry(in FixedString64 entryId) {
            if (this.entriesMap.TryGetValue(entryId.GetHashCode(), out PackedTextureEntry entry)) {
                return entry;
            }
            
            throw new Exception($"PackedTextureEntry can't be found {entryId.ToString()}");
        }

        public PackedTextureEntry GetEntry(int hashcode) {
            if (this.entriesMap.TryGetValue(hashcode, out PackedTextureEntry entry)) {
                return entry;
            }
            
            throw new Exception($"PackedTextureEntry can't be found {hashcode}");
        }
    }
}