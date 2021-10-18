using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Common {
    public class TexturePacker {
        private const int BUFFER_SIZE = 100;

        // Contains the associated names of the added texture so we can easily query its entry after packing
        private readonly SimpleList<string> names = new SimpleList<string>(BUFFER_SIZE);

        // This contains the textures to pack
        // Used a list here so we could easily convert to array during packing
        private readonly List<Texture2D> textures = new List<Texture2D>(BUFFER_SIZE);

        // These are textures that will be drawn above and blended with the regular textures
        private readonly List<Texture2D> splattedTextures = new List<Texture2D>(BUFFER_SIZE);
        private readonly SimpleList<string> splattedTexturesNames = new SimpleList<string>(BUFFER_SIZE);

        private readonly SimpleList<Vector2Int> originalDimensions = new SimpleList<Vector2Int>(BUFFER_SIZE);

        // Keeps track of the packed entries
        private NativeHashMap<int, PackedTextureEntry> entriesMap;
        private Texture2D? atlas;

        // TODO refactor maybe?
        private NativeHashMap<int, PackedTextureEntry> splattedEntriesMap;
        private Texture2D? splattedAtlas;

        /// <summary>
        /// Constructor
        /// </summary>
        public TexturePacker() {
            this.entriesMap = new NativeHashMap<int, PackedTextureEntry>(BUFFER_SIZE, Allocator.Persistent);
            this.splattedEntriesMap = new NativeHashMap<int, PackedTextureEntry>(BUFFER_SIZE, Allocator.Persistent);
        }

        public void Dispose() {
            if (this.entriesMap.IsCreated) {
                this.entriesMap.Dispose();
            }

            if (this.splattedEntriesMap.IsCreated) {
                this.splattedEntriesMap.Dispose();
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
        /// <param name="splatted"></param>
        public void Add(string key, Texture2D texture, int originalWidth, int originalHeight, bool splatted) {
            this.names.Add(key);
            this.textures.Add(texture);
            this.originalDimensions.Add(new Vector2Int(originalWidth, originalHeight));

            // Add also to the splatted list if the texture is meant to be blended with the regular textures
            // TODO refactor code so that we don't add this to the textures list. Note that the other lists are index-dependent to each other
            if (splatted) {
                this.splattedTexturesNames.Add(key);
                this.splattedTextures.Add(texture);
            }
        }

        /// <summary>
        /// Packs the added texture
        /// </summary>
        public void Pack() {
            this.atlas = new Texture2D(2, 2, TextureFormat.ARGB32, false); // Will expand on packing
            this.atlas.filterMode = FilterMode.Point; // Very important to avoid seams
            Rect[] rects = this.atlas.PackTextures(this.textures.ToArray(), 0, 8192, false);

            // Populate entries
            this.entriesMap.Clear();
            Assertion.IsTrue(this.names.Count == this.textures.Count);
            for (int i = 0; i < this.names.Count; ++i) {
                int originalWidth = this.originalDimensions[i].x;
                int originalHeight = this.originalDimensions[i].y;
                int hashcode = new FixedString64(this.names[i]).GetHashCode();
                this.entriesMap[hashcode] =
                    new PackedTextureEntry(rects[i], this.atlas.width, this.atlas.height, originalWidth, originalHeight);
            }

            PackSplattedAtlas();

            // Clear the memory held by the individual textures
            this.textures.Clear();
            this.textures.Clear();
            this.splattedTextures.Clear();
            this.splattedTexturesNames.Clear();
        }

        private void PackSplattedAtlas() {
            // TODO The splatted atlas is just the same for now
            this.splattedAtlas = new Texture2D(2, 2, TextureFormat.ARGB32, false); // Will expand on packing
            this.splattedAtlas.filterMode = FilterMode.Point; // Very important to avoid seams
            // TODO might be better to make this smaller than the actual atlas since not all objects are going to be splatted
            Rect[] rects = this.splattedAtlas.PackTextures(this.splattedTextures.ToArray(), 0, 8192, false);

            // Populate entries
            this.splattedEntriesMap.Clear();
            for (int i = 0; i < this.splattedTextures.Count; ++i) {
                int hashcode = new FixedString64(this.splattedTexturesNames[i]).GetHashCode();
                PackedTextureEntry textureEntry = this.entriesMap[hashcode];
                this.splattedEntriesMap[hashcode] = new PackedTextureEntry(rects[i], this.splattedAtlas.width,
                    this.splattedAtlas.height, textureEntry.originalWidth, textureEntry.originalHeight);
            }
        }

        /// <summary>
        /// Compresses the atlas
        /// </summary>
        public void Compress() {
            if (this.atlas == null) {
                throw new Exception("Atlas is not prepared yet.");
            }

            this.atlas.Compress(false);
            this.atlas.Apply(false, true);

            if (this.splattedAtlas == null) {
                throw new Exception("Atlas is not prepared yet.");
            }

            this.splattedAtlas.Compress(false);
            this.splattedAtlas.Apply(false, true);
        }

        /// <summary>
        /// Returns the entry with the specified key
        /// Returns null if there's no such entry
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public PackedTextureEntry GetEntry(string key) {
            if (this.entriesMap.TryGetValue(new FixedString64(key).GetHashCode(), out PackedTextureEntry entry)) {
                return entry;
            }

            throw new Exception($"PackedTextureEntry for {key} can't be found.");
        }

        public PackedTextureEntry GetSplattedEntry(string key) {
            if (this.splattedEntriesMap.TryGetValue(new FixedString64(key).GetHashCode(), out PackedTextureEntry entry)) {
                return entry;
            }

            throw new Exception($"PackedTextureEntry for {key} can't be found.");
        }

        public Texture2D Atlas {
            get {
                if (this.atlas == null) {
                    throw new Exception("Atlas is not prepared yet.");
                }

                return this.atlas;
            }
        }

        public Texture2D SplattedAtlas {
            get {
                if (this.splattedAtlas == null) {
                    throw new Exception("Atlas is not prepared yet.");
                }

                return this.splattedAtlas;
            }
        }

        public PackedTextureEntryResolver Resolver {
            get {
                return new PackedTextureEntryResolver(this.entriesMap);
            }
        }

        public PackedTextureEntryResolver SplattedResolver {
            get {
                return new PackedTextureEntryResolver(this.splattedEntriesMap);
            }
        }
    }
}