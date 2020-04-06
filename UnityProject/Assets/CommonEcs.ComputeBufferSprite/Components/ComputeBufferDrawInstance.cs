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

        public NativeArray<float4x4> Matrices {
            get {
                return this.internalInstance.matrices;
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
            private ComputeBuffer sizePivotBuffer;
            private ComputeBuffer uvBuffer;
            private ComputeBuffer matricesBuffer;
            private ComputeBuffer colorBuffer;
            
            // Arrays
            public NativeArray<float4x4> matrices;
            
            public NativeArray<float4> sizePivots;
            public NativeArray<float4> uvs;
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

                this.matricesBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, sizeof(float));
                this.sizePivotBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, 16);
                this.uvBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, 16);
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
                    this.matrices.Dispose();
                    this.sizePivots.Dispose();
                    this.uvs.Dispose();
                    this.colors.Dispose();
                }
                
                this.sizePivots = new NativeArray<float4>(count, Allocator.Persistent);
                this.uvs = new NativeArray<float4>(count, Allocator.Persistent);
                this.matrices = new NativeArray<float4x4>(count, Allocator.Persistent);
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