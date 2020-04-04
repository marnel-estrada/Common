using System;

using Common;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace CommonEcs {
    public struct ComputeBufferDrawInstance : ISharedComponentData, IEquatable<ComputeBufferDrawInstance> {
        private readonly int id;
        private static readonly IdGenerator ID_GENERATOR = new IdGenerator(1);
        
        private readonly InternalImplementation internalInstance;

        public ComputeBufferDrawInstance(Material material) {
            this.id = ID_GENERATOR.Generate();
            this.internalInstance = new InternalImplementation(material);
        }

        /// <summary>
        /// Expands the arrays if the current length can no longer accommodate the specified count
        /// </summary>
        /// <param name="count"></param>
        public void Expand(int count) {
            this.internalInstance.Expand(count);
        }

        public NativeList<ComputeBufferSprite> Sprites {
            get {
                return this.internalInstance.sprites;
            }
        }

        public NativeArray<float4> SizePivots {
            get {
                return this.internalInstance.sizePivots;
            }
        }

        public NativeArray<float4> Uvs {
            get {
                return this.internalInstance.uvs;
            }
        }

        public NativeArray<float4> Transforms {
            get {
                return this.internalInstance.transforms;
            }
        }

        public NativeArray<float> Rotations {
            get {
                return this.internalInstance.rotations;
            }
        }

        public NativeArray<float4> Colors {
            get {
                return this.internalInstance.colors;
            }
        }

        /// <summary>
        /// We pass in mesh here because it's a common mesh
        /// </summary>
        /// <param name="mesh"></param>
        public void Draw(Mesh mesh, uint drawCount) {
            this.internalInstance.Draw(mesh, drawCount);
        }

        public class InternalImplementation {
            private Material material;

            public NativeList<ComputeBufferSprite> sprites;

            // Buffers
            private ComputeBuffer sizeAnchorBuffer;
            private ComputeBuffer uvBuffer;
            private ComputeBuffer transformBuffer;
            private ComputeBuffer rotationBuffer;
            private ComputeBuffer colorBuffer;
            
            // Arrays
            public NativeArray<float4> sizePivots;
            public NativeArray<float4> uvs;
            public NativeArray<float4> transforms;
            public NativeArray<float> rotations;
            public NativeArray<float4> colors;

            private NativeArray<uint> args;
            private ComputeBuffer argsBuffer;

            public const int MAX_SPRITE_COUNT = 300000;

            public bool uvChanged;
            public bool colorChanged;
            public bool renderOrderChanged;

            public InternalImplementation(Material material) {
                this.material = material;

                this.sprites = new NativeList<ComputeBufferSprite>(Allocator.Persistent);

                this.sizeAnchorBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, 16);
                this.uvBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, 16);
                this.transformBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, 16);
                this.rotationBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, sizeof(float));
                this.colorBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, 16);

                // Prepare args
                this.args = new NativeArray<uint>(new uint[] {
                    6, 0, 0, 0, 0
                }, Allocator.Persistent);
                this.argsBuffer = new ComputeBuffer(1, this.args.Length * sizeof(uint),
                    ComputeBufferType.IndirectArguments);
            }
            
            public void Expand(int count) {
                if (this.uvs.IsCreated && this.uvs.Length >= count) {
                    // Current arrays can still accommodate the specified number of sprites
                    return;
                }
                
                // Dispose old
                if (this.uvs.IsCreated) {
                    this.sizePivots.Dispose();
                    this.uvs.Dispose();
                    this.transforms.Dispose();
                    this.rotations.Dispose();
                    this.colors.Dispose();
                }
                
                this.sizePivots = new NativeArray<float4>(count, Allocator.Persistent);
                this.uvs = new NativeArray<float4>(count, Allocator.Persistent);
                this.transforms = new NativeArray<float4>(count, Allocator.Persistent);
                this.rotations = new NativeArray<float>(count, Allocator.Persistent);
                this.colors = new NativeArray<float4>(count, Allocator.Persistent);
            }

            private static readonly Bounds BOUNDS = new Bounds(Vector2.zero, Vector3.one);

            /// <summary>
            /// We pass in mesh here because it's a common mesh
            /// </summary>
            /// <param name="mesh"></param>
            public void Draw(Mesh mesh, uint drawCount) {
                this.args[1] = drawCount;
                this.argsBuffer.SetData(this.args);

                Graphics.DrawMeshInstancedIndirect(mesh, 0, this.material, BOUNDS, this.argsBuffer);
            }
        }

        public bool Equals(ComputeBufferDrawInstance other) {
            return this.id == other.id;
        }

        public override bool Equals(object obj) {
            return obj is ComputeBufferDrawInstance other && Equals(other);
        }

        public override int GetHashCode() {
            return this.id;
        }

        public static bool operator ==(ComputeBufferDrawInstance left, ComputeBufferDrawInstance right) {
            return left.Equals(right);
        }

        public static bool operator !=(ComputeBufferDrawInstance left, ComputeBufferDrawInstance right) {
            return !left.Equals(right);
        }
    }
}