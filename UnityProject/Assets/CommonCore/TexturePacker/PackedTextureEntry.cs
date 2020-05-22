using Unity.Mathematics;

using UnityEngine;

namespace Common {
    public class PackedTextureEntry {
        private readonly Texture2D atlas;
        private readonly Rect uvRect;
        private readonly Rect spriteRect;

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
        public PackedTextureEntry(Texture2D atlas, Rect uvRect, int originalWidth, int originalHeight) {
            this.atlas = atlas;
            this.uvRect = uvRect;

            this.spriteRect = new Rect(this.uvRect.x * this.atlas.width, this.uvRect.y * this.atlas.height, 
                this.uvRect.width * this.atlas.width, this.uvRect.height * this.atlas.height);

            this.originalWidth = originalWidth;
            this.originalHeight = originalHeight;
        }
        
        public Texture2D Atlas {
            get {
                return atlas;
            }
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