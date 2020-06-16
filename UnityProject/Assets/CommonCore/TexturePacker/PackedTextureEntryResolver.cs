using System;

using Unity.Collections;

namespace Common {
    /// <summary>
    /// Wraps a NativeHashMap so we don't pass it around as is
    /// </summary>
    public struct PackedTextureEntryResolver {
        private NativeHashMap<FixedString64, PackedTextureEntry> entriesMap;

        public PackedTextureEntryResolver(NativeHashMap<FixedString64, PackedTextureEntry> entriesMap) {
            this.entriesMap = entriesMap;
        }

        public PackedTextureEntry GetEntry(FixedString64 entryId) {
            if (this.entriesMap.TryGetValue(entryId, out PackedTextureEntry entry)) {
                return entry;
            }
            
            throw new Exception($"PackedTextureEntry can't be found {entryId.ToString()}");
        }
    }
}