using Unity.Mathematics;

using UnityEngine;

namespace Common {
    public readonly struct PackedTextureEntry {
        public readonly Rect uvRect;
        public readonly Rect spriteRect;

        public readonly int atlasWidth;
        public readonly int atlasHeight;

        // Note that the passed uvRect may already be a scaled image
        // We keep a copy of the original dimension so we can still use the sprite as though it has
        // used its original size
        public readonly int originalWidth;
        public readonly int originalHeight;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="atlas"></param>
        /// <param name="uvRect"></param>
        public PackedTextureEntry(Rect uvRect, int atlasWidth, int atlasHeight, int originalWidth, int originalHeight) {
            this.uvRect = uvRect;

            this.atlasWidth = atlasWidth;
            this.atlasHeight = atlasHeight;
            
            this.spriteRect = new Rect(this.uvRect.x * this.atlasWidth, this.uvRect.y * this.atlasHeight, 
                this.uvRect.width * this.atlasWidth, this.uvRect.height * this.atlasHeight);

            this.originalWidth = originalWidth;
            this.originalHeight = originalHeight;
        }

        public float2 LowerLeftUv {
            get {
                return this.uvRect.min;
            }
        }

        public float2 UvSize {
            get {
                return this.uvRect.size;
            }
        }
    }
}