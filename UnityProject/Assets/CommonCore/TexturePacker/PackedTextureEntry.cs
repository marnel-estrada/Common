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
        /// <param name="uvRect"></param>
        /// <param name="atlasWidth"></param>
        /// <param name="atlasHeight"></param>
        /// <param name="originalWidth"></param>
        /// <param name="originalHeight"></param>
        public PackedTextureEntry(Rect uvRect, int atlasWidth, int atlasHeight,
                                  int originalWidth, int originalHeight) {
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

        public bool Equals(PackedTextureEntry other) {
            return this.uvRect.Equals(other.uvRect) &&
                   this.spriteRect.Equals(other.spriteRect) &&
                   this.atlasWidth == other.atlasWidth &&
                   this.atlasHeight == other.atlasHeight &&
                   this.originalWidth == other.originalWidth &&
                   this.originalHeight == other.originalHeight;
        }

        public override bool Equals(object? obj) {
            return obj is PackedTextureEntry other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = this.uvRect.GetHashCode();
                hashCode = (hashCode * 397) ^ this.spriteRect.GetHashCode();
                hashCode = (hashCode * 397) ^ this.atlasWidth;
                hashCode = (hashCode * 397) ^ this.atlasHeight;
                hashCode = (hashCode * 397) ^ this.originalWidth;
                hashCode = (hashCode * 397) ^ this.originalHeight;
                return hashCode;
            }
        }

        public static bool operator ==(PackedTextureEntry left, PackedTextureEntry right) {
            return left.Equals(right);
        }

        public static bool operator !=(PackedTextureEntry left, PackedTextureEntry right) {
            return !left.Equals(right);
        }
    }
}