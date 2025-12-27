using System;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Common {
    public class TexturePacker {
        private const int BUFFER_SIZE = 100;

        // Contains the associated names of the added texture so we can easily query its entry after packing
        private readonly SimpleList<string> names = new(BUFFER_SIZE);

        // This contains the textures to pack
        // Used a list here so we could easily convert to array during packing
        private readonly List<Texture2D> textures = new(BUFFER_SIZE);

        private readonly SimpleList<Vector2Int> originalDimensions = new(BUFFER_SIZE);

        // Keeps track of the packed entries
        private NativeParallelHashMap<int, PackedTextureEntry> entriesMap;

        // The UV array that can be used into a ComputeBufferSpriteManager
        private NativeArray<float4> uvs; 

        private Texture2D? atlas;

        /// <summary>
        /// Constructor
        /// </summary>
        public TexturePacker() {
            this.entriesMap = new NativeParallelHashMap<int, PackedTextureEntry>(BUFFER_SIZE, Allocator.Persistent);
        }

        public void Dispose() {
            if (this.entriesMap.IsCreated) {
                this.entriesMap.Dispose();
            }

            if (this.uvs.IsCreated) {
                this.uvs.Dispose();
            }
        }

        /// <summary>
        /// Adds a texture to pack with original width and height
        /// Note that the specified texture might be scaled down
        /// </summary>
        /// <param name="key"></param>
        /// <param name="texture"></param>
        /// <param name="originalWidth"></param>
        /// <param name="originalHeight"></param>
        public void Add(string key, Texture2D texture, int originalWidth, int originalHeight) {
            this.names.Add(key);
            this.textures.Add(texture);
            this.originalDimensions.Add(new Vector2Int(originalWidth, originalHeight));
        }

        /// <summary>
        /// Packs the added texture
        /// </summary>
        public void Pack() {
            // Will expand on packing
            this.atlas = new Texture2D(2, 2, TextureFormat.ARGB32, false) {
                filterMode = FilterMode.Point // Very important to avoid seams
            };
            Rect[] rects = this.atlas.PackTextures(this.textures.ToArray(), 0, 8192, false);

            // Populate entries
            this.entriesMap.Clear();
            Assertion.IsTrue(this.names.Count == this.textures.Count);
            
            // Prepare the UVs as well. We only prepare it here so we know the length.
            this.uvs = new NativeArray<float4>(rects.Length, Allocator.Persistent);
            
            for (int i = 0; i < this.names.Count; ++i) {
                int originalWidth = this.originalDimensions[i].x;
                int originalHeight = this.originalDimensions[i].y;
                Rect uvRect = rects[i];
                this.uvs[i] = new float4(uvRect.width, uvRect.height, uvRect.x, uvRect.y);
                int hashcode = new FixedString64Bytes(this.names[i]).GetHashCode();
                this.entriesMap[hashcode] =
                    new PackedTextureEntry(uvRect, this.atlas.width, this.atlas.height, 
                        originalWidth, originalHeight, i);
            }

            // Clear the memory held by the individual textures
            this.textures.Clear();
            this.names.Clear();
        }

        /// <summary>
        /// Compresses the atlas
        /// </summary>
        public void Compress() {
            if (!this.atlas) {
                throw new Exception("Atlas is not prepared yet.");
            }
            
            this.atlas.Compress(false);
            this.atlas.Apply(false, false);
        }

        /// <summary>
        /// Returns the entry with the specified key
        /// Returns null if there's no such entry
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public PackedTextureEntry GetEntry(string key) {
            if (this.entriesMap.TryGetValue(new FixedString64Bytes(key).GetHashCode(), out PackedTextureEntry entry)) {
                return entry;
            }

            throw new Exception($"PackedTextureEntry for {key} can't be found.");
        }

        public PackedTextureEntry GetEntry(FixedString64Bytes key) {
            if (this.entriesMap.TryGetValue(key.GetHashCode(), out PackedTextureEntry entry)) {
                return entry;
            }

            throw new Exception($"PackedTextureEntry for {key} can't be found.");
        }
        
        public PackedTextureEntry GetEntry(int entryIdHashCode) {
            if (this.entriesMap.TryGetValue(entryIdHashCode, out PackedTextureEntry entry)) {
                return entry;
            }

            throw new Exception($"PackedTextureEntry for {entryIdHashCode} can't be found.");
        }

        public Texture2D Atlas {
            get {
                if (this.atlas == null) {
                    throw new Exception("Atlas is not prepared yet.");
                }
                
                return this.atlas;
            }
        }

        public PackedTextureEntryResolver Resolver => new(this.entriesMap);

        public NativeArray<float4> Uvs => this.uvs;
    }
}