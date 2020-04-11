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

        public void Add(ref ComputeBufferSprite sprite) {
            this.internalInstance.Add(ref sprite);
        }

        public NativeArray<ComputeBufferSprite> SpritesMasterList {
            get {
                return this.internalInstance.spritesMasterList;
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

        public int SpriteCount {
            get {
                return this.internalInstance.SpriteCount;
            }
        } 

        /// <summary>
        /// We pass in mesh here because it's a common mesh
        /// </summary>
        /// <param name="quad"></param>
        public void Draw(Mesh quad) {
            this.internalInstance.Draw(quad);
        }
        
        public void Dispose() {
            this.internalInstance.Dispose();
        }

        public class InternalImplementation {
            // We don't set as readonly as it should be able to be changed at runtime
            private Material material;

            public NativeList<ComputeBufferSprite> spritesMasterList;

            // Buffers
            private readonly ComputeBuffer sizePivotBuffer;
            private readonly ComputeBuffer uvBuffer;
            private readonly ComputeBuffer matricesBuffer;
            private readonly ComputeBuffer colorBuffer;
            
            // Arrays
            public NativeArray<float4x4> matrices;
            public NativeArray<float4> sizePivots;
            public NativeArray<float4> uvs;
            public NativeArray<float4> colors;

            private NativeArray<uint> args;
            private readonly ComputeBuffer argsBuffer;

            public const int MAX_SPRITE_COUNT = 300000;

            public bool uvChanged;
            public bool colorChanged;
            public bool renderOrderChanged;
            
            private readonly int matricesBufferId;
            private readonly int uvBufferId;
            private readonly int colorsBufferId;
            private readonly int sizePivotBufferId;

            public int capacity;
            private int spriteCount;

            public InternalImplementation(Material material) {
                this.material = material;

                this.capacity = 1000;
                this.spritesMasterList = new NativeList<ComputeBufferSprite>(this.capacity, Allocator.Persistent);
                ExpandArrays(this.capacity);

                this.matricesBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, 64);
                this.sizePivotBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, 16);
                this.uvBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, 16);
                this.colorBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, 16);
                
                this.matricesBufferId = Shader.PropertyToID("matricesBuffer");
                this.uvBufferId = Shader.PropertyToID("uvBuffer");
                this.colorsBufferId = Shader.PropertyToID("colorsBuffer");
                this.sizePivotBufferId = Shader.PropertyToID("sizePivotBuffer");

                // Prepare args
                this.args = new NativeArray<uint>(new uint[] {
                    6, 0, 0, 0, 0
                }, Allocator.Persistent);
                this.argsBuffer = new ComputeBuffer(1, this.args.Length * sizeof(uint),
                    ComputeBufferType.IndirectArguments);
            }

            public int SpriteCount {
                get {
                    return this.spriteCount;
                }
            }

            public void Add(ref ComputeBufferSprite sprite) {
                if (this.spriteCount + 1 > MAX_SPRITE_COUNT) {
                    // The ultimate maximum has been reached
                    throw new Exception("MAX_SPRITE_COUNT has been reached");
                }

                if (this.spriteCount >= this.capacity) {
                    // Current sprites have exceeded capacity.
                    // We expand
                    ExpandArrays();
                }
                
                sprite.masterListIndex = this.spriteCount;
                this.spritesMasterList.Add(sprite);

                ++this.spriteCount;
            }

            private void ExpandArrays() {
                this.capacity *= 2;
                ExpandArrays(this.capacity);
            }
            
            private void ExpandArrays(int count) {
                if (this.matrices.IsCreated && this.matrices.Length >= count) {
                    // Current arrays can still accommodate the specified number of sprites
                    return;
                }
                
                // Dispose old
                if (this.matrices.IsCreated) {
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
            /// <param name="quad"></param>
            public void Draw(Mesh quad) {
                // Update the buffers
                this.matricesBuffer.SetData(this.matrices);
                this.material.SetBuffer(this.matricesBufferId, this.matricesBuffer);

                this.uvBuffer.SetData(this.uvs);
                this.material.SetBuffer(this.uvBufferId, this.uvBuffer);

                this.colorBuffer.SetData(this.colors);
                this.material.SetBuffer(this.colorsBufferId, this.colorBuffer);
            
                this.sizePivotBuffer.SetData(this.sizePivots);
                this.material.SetBuffer(this.sizePivotBufferId, this.sizePivotBuffer);
                
                this.args[1] = (uint) this.spriteCount;
                this.argsBuffer.SetData(this.args);

                Graphics.DrawMeshInstancedIndirect(quad, 0, this.material, BOUNDS, this.argsBuffer);
            }
            
            public void Dispose() {
                this.spritesMasterList.Dispose();
                
                this.matrices.Dispose();
                this.sizePivots.Dispose();
                this.uvs.Dispose();
                this.colors.Dispose();
                this.args.Dispose();
                
                this.matricesBuffer.Release();
                this.sizePivotBuffer.Release();
                this.uvBuffer.Release();
                this.colorBuffer.Release();
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