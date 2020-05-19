using System.Collections.Generic;

using UnityEngine;

namespace Common {
    public class TexturePacker {
        private const int BUFFER_SIZE = 100;

        // Contains the associated names of the added texture so we can easily query its entry after packing
        private readonly SimpleList<string> names = new SimpleList<string>(BUFFER_SIZE);

        // This contains the textures to pack
        // Used a list here so we could easily convert to array during packing
        private readonly List<Texture2D> textures = new List<Texture2D>(BUFFER_SIZE);

        private readonly SimpleList<Vector2Int> originalDimensions = new SimpleList<Vector2Int>(BUFFER_SIZE);

        // Keeps track of the packed entries
        private readonly Dictionary<string, PackedTextureEntry> entriesMap =
            new Dictionary<string, PackedTextureEntry>(BUFFER_SIZE);

        private Texture2D atlas;

        /// <summary>
        /// Constructor
        /// </summary>
        public TexturePacker() {
        }

        /// <summary>
        /// Constructor with specified atlas
        /// </summary>
        /// <param name="atlas"></param>
        public TexturePacker(Texture2D atlas) {
            this.atlas = atlas;
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
            this.atlas = new Texture2D(2, 2, TextureFormat.ARGB32, false); // Will expand on packing
            Rect[] rects = this.atlas.PackTextures(this.textures.ToArray(), 1, 8192, false);

            // Populate entries
            this.entriesMap.Clear();
            Assertion.IsTrue(this.names.Count == this.textures.Count);
            for (int i = 0; i < this.names.Count; ++i) {
                int originalWidth = this.originalDimensions[i].x;
                int originalHeight = this.originalDimensions[i].y;
                this.entriesMap[this.names[i]] =
                    new PackedTextureEntry(this.atlas, rects[i], originalWidth, originalHeight);
            }

            // Clear the memory held by the individual textures
            this.textures.Clear();
            this.names.Clear();
        }

        /// <summary>
        /// Compresses the atlas
        /// </summary>
        public void Compress() {
            this.atlas.Compress(false);
            this.atlas.Apply(false, true);
        }

        /// <summary>
        /// Returns the entry with the specified key
        /// Returns null if there's no such entry
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public PackedTextureEntry GetEntry(string key) {
            return this.entriesMap[key];
        }

        public Texture2D Atlas {
            get {
                return this.atlas;
            }
        }
    }
}