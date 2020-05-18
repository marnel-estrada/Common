using Unity.Mathematics;

using UnityEngine;

namespace Common {
    public class PackedTextureEntry {
        private readonly Texture2D atlas;
        private readonly Rect uvRect;
        private readonly Rect spriteRect;

        private readonly Vector2[] uvs = new Vector2[4];

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

            // Prepare UVs
            this.uvs[0] = this.uvRect.min;
            this.uvs[1].Set(this.uvRect.xMax, this.uvRect.yMin);
            this.uvs[2].Set(this.uvRect.xMin, this.uvRect.yMax);
            this.uvs[3] = this.uvRect.max;

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

        public Vector2[] Uvs {
            get {
                return this.uvs;
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
                return new float2(this.uvs[0].x, this.uvs[0].y);
            }
        }

        public float2 UvDimension {
            get {
                Vector2 difference = this.uvs[3] - this.uvs[0];
                return new float2(difference.x, difference.y);
            }
        }
    }
}