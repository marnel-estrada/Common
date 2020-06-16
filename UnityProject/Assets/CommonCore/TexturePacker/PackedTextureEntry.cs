using Unity.Mathematics;

using UnityEngine;

namespace Common {
    public struct PackedTextureEntry {
        private readonly Rect uvRect;
        private readonly Rect spriteRect;

        private readonly int atlasWidth;
        private readonly int atlasHeight;

        // Note that the passed uvRect may already be a scaled image
        // We keep a copy of the original dimension so we can still use the sprite as though it has
        // used its original size
        private readonly int originalWidth;
        private readonly int originalHeight;

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

        public Rect UvRect {
            get {
                return uvRect;
            }
        }

        public Rect SpriteRect {
            get {
                return spriteRect;
            }
        }

        public int OriginalWidth {
            get {
                return this.originalWidth;
            }
        }

        public int OriginalHeight {
            get {
                return this.originalHeight;
            }
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