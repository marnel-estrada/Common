using System;
using System.Collections.Generic;
using Common;
using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CommonEcs {
    public readonly struct ComputeBufferSpriteManager : ISharedComponentData, IEquatable<ComputeBufferSpriteManager> {
        private readonly Internal internalInstance;
        
        private readonly int id;
        private static readonly IdGenerator ID_GENERATOR = new(1);

        public ComputeBufferSpriteManager(Material material, NativeArray<float4> uvValues, int initialCapacity) {
            this.internalInstance = new Internal(material, uvValues, initialCapacity);
            this.id = ID_GENERATOR.Generate();
        }

        public void Dispose() {
            this.internalInstance.Dispose();
        }

        public void AddUvIndicesBuffer(string shaderPropertyId) {
            this.internalInstance.AddUvIndicesBuffer(shaderPropertyId);
        }

        public void Add(ref ComputeBufferSprite sprite, float3 position) {
            Add(ref sprite, position, quaternion.identity, 1.0f);
        }
        
        public void Add(ref ComputeBufferSprite sprite, float3 position, quaternion rotation) {
            Add(ref sprite, position, rotation, 1.0f);
        }

        /// <summary>
        /// Adds a sprite.
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="uvIndex"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        public void Add(ref ComputeBufferSprite sprite, float3 position, quaternion rotation, float scale) {
            this.internalInstance.Add(ref sprite, position, rotation, scale);
        }

        /// <summary>
        /// Sets the uvIndex of the sprite.
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="uvBufferIndex">Which UV buffer is it. Is it the first or the second?</param>
        /// <param name="value"></param>
        public void SetUvIndex(ref ComputeBufferSprite sprite, int uvBufferIndex, int value) {
            this.internalInstance.SetUvIndex(ref sprite, uvBufferIndex, value);
        }

        public int UvIndicesBufferCount => this.internalInstance.UvIndicesBufferCount;

        public NativeArray<int> GetUvBufferIndices(int uvBufferIndex) {
            return this.internalInstance.GetUvBufferIndices(uvBufferIndex);
        }

        public void Draw(Bounds bounds) {
            this.internalInstance.Draw(bounds);
        }

        public NativeArray<float4> TranslationsAndScales => this.internalInstance.translationsAndScales;
        public NativeArray<float4> Rotations => this.internalInstance.rotations;
        public NativeArray<float2> Sizes => this.internalInstance.sizes;
        public NativeArray<float2> Pivots => this.internalInstance.pivots;
        public NativeArray<Color> Colors => this.internalInstance.colors;

        public void Remove(int managerIndex) {
            this.internalInstance.Remove(managerIndex);
        }
        
        private class Internal {
            private readonly Material material;
            private readonly Mesh quad; // The single quad that we need
            
            // uvBuffer contains float4 values in which xy is the uv dimension and zw is the texture offset
            // These two are readonly as they won't change
            private readonly ComputeBuffer uvBuffer;
            private NativeArray<float4> uvValues;
            
            // Matrix here is a compressed transform information
            // xy is the position, z is rotation, w is the scale
            private ComputeBuffer translationAndScaleBuffer;
            public NativeArray<float4> translationsAndScales;

            public ComputeBuffer rotationBuffer;
            public NativeArray<float4> rotations;

            private ComputeBuffer sizeBuffer;
            public NativeArray<float2> sizes;

            private ComputeBuffer pivotBuffer;
            public NativeArray<float2> pivots;
        
            private ComputeBuffer colorBuffer;
            public NativeArray<Color> colors;
            
            private readonly uint[] args;
            private readonly ComputeBuffer argsBuffer;

            // We did it this way because there can be multiple UV buffers
            private readonly List<UvIndicesBuffer> uvIndicesBuffers = new(2);

            // We're only managing the removed manager indices here instead of the whole Sprite values
            private NativeList<int> inactiveList;

            private int capacity;
            private int spriteCount;

            // Buffer IDs
            private readonly int uvBufferId;
            private readonly int translationAndScaleBufferId;
            private readonly int rotationBufferId;
            private readonly int sizeBufferId;
            private readonly int pivotBufferId;
            private readonly int colorsBufferId;

            public Internal(Material material, NativeArray<float4> uvValues, int initialCapacity) {
                this.material = material;
                this.capacity = math.max(initialCapacity, 2); // Prevents error when initialCapacity is zero
                this.quad = MeshUtils.Quad(1.0f);

                const int floatSize = sizeof(float);
                const int float2Size = floatSize * 2; 
                const int float4Size = floatSize * 4;
                
                this.uvBuffer = new ComputeBuffer(uvValues.Length, float4Size);
                this.uvValues = new NativeArray<float4>(uvValues.Length, Allocator.Persistent);
                this.uvValues.CopyFrom(uvValues);
                this.uvBuffer.SetData(this.uvValues);
                
                this.translationAndScaleBuffer = new ComputeBuffer(this.capacity, float4Size);
                this.translationsAndScales = new NativeArray<float4>(this.capacity, Allocator.Persistent);
                this.translationAndScaleBuffer.SetData(this.translationsAndScales);

                this.rotationBuffer = new ComputeBuffer(this.capacity, float4Size);
                this.rotations = new NativeArray<float4>(this.capacity, Allocator.Persistent);
                this.rotationBuffer.SetData(this.rotations);

                this.sizeBuffer = new ComputeBuffer(this.capacity, float2Size);
                this.sizes = new NativeArray<float2>(this.capacity, Allocator.Persistent);
                this.sizeBuffer.SetData(this.sizes);

                this.pivotBuffer = new ComputeBuffer(this.capacity, float2Size);
                this.pivots = new NativeArray<float2>(this.capacity, Allocator.Persistent);
                this.pivotBuffer.SetData(this.pivots);

                this.colorBuffer = new ComputeBuffer(this.capacity, float4Size);
                this.colors = new NativeArray<Color>(this.capacity, Allocator.Persistent);
                this.colorBuffer.SetData(this.colors);
                
                // Prepare the shader IDs
                this.uvBufferId = Shader.PropertyToID("uvBuffer");
                this.translationAndScaleBufferId = Shader.PropertyToID("translationAndScaleBuffer");
                this.rotationBufferId = Shader.PropertyToID("rotationBuffer");
                this.sizeBufferId = Shader.PropertyToID("sizeBuffer");
                this.pivotBufferId = Shader.PropertyToID("pivotBuffer");
                this.colorsBufferId = Shader.PropertyToID("colorsBuffer");

                SetMaterialBuffers();

                this.args = new uint[] {
                    6, (uint)this.capacity, 0, 0, 0
                };
                this.argsBuffer = new ComputeBuffer(1, this.args.Length * sizeof(uint),
                    ComputeBufferType.IndirectArguments);
                this.argsBuffer.SetData(this.args);

                this.inactiveList = new NativeList<int>(10, Allocator.Persistent);
            }

            private void SetMaterialBuffers() {
                this.material.SetBuffer(this.uvBufferId, this.uvBuffer);
                this.material.SetBuffer(this.translationAndScaleBufferId, this.translationAndScaleBuffer);
                this.material.SetBuffer(this.rotationBufferId, this.rotationBuffer);
                this.material.SetBuffer(this.sizeBufferId, this.sizeBuffer);
                this.material.SetBuffer(this.pivotBufferId, this.pivotBuffer);
                this.material.SetBuffer(this.colorsBufferId, this.colorBuffer);

                for (int i = 0; i < this.uvIndicesBuffers.Count; i++) {
                    this.uvIndicesBuffers[i].SetBuffer(this.material);
                }
            }

            public void Dispose() {
                this.uvBuffer.Release();
                this.translationAndScaleBuffer.Release();
                this.rotationBuffer.Release();
                this.sizeBuffer.Release();
                this.pivotBuffer.Release();
                this.colorBuffer.Release();
                this.argsBuffer.Release();
                
                this.uvValues.Dispose();
                this.translationsAndScales.Dispose();
                this.rotations.Dispose();
                this.sizes.Dispose();
                this.pivots.Dispose();
                this.colors.Dispose();
                this.inactiveList.Dispose();
                
                // Dispose UV indices
                for (int i = 0; i < this.uvIndicesBuffers.Count; i++) {
                    this.uvIndicesBuffers[i].Dispose();
                }
            }

            /// <summary>
            /// Adds a sprite.
            /// </summary>
            /// <param name="sprite"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            /// <param name="scale"></param>
            public void Add(ref ComputeBufferSprite sprite, float3 position, quaternion rotation, float scale) {
                // Check if there are inactive sprite slots and use those first
                if (this.inactiveList.Length > 0) {
                    AddByReusingInactive(ref sprite, position, rotation, scale);
                    return;
                }

                // Expand if we're out of space
                while (this.spriteCount >= this.capacity) {
                    Expand();
                }

                sprite.managerIndex = ValueTypeOption<int>.Some(this.spriteCount);
                InternalAdd(ref sprite, position, rotation, scale);
            }

            private void AddByReusingInactive(ref ComputeBufferSprite sprite, float3 position, quaternion rotation, float scale) {
                Assertion.IsTrue(this.inactiveList.Length > 0);
                
                int lastIndex = this.inactiveList.Length - 1;
                int reusedIndex = this.inactiveList[lastIndex];
                this.inactiveList.RemoveAt(lastIndex);
                sprite.managerIndex = ValueTypeOption<int>.Some(reusedIndex);

                InternalAdd(ref sprite, position, rotation, scale);
            }

            private void InternalAdd(ref ComputeBufferSprite sprite, float3 position, quaternion rotation, float scale) {
                int managerIndex = sprite.managerIndex.ValueOrError();
                this.translationsAndScales[managerIndex] = new float4(position, scale);
                this.rotations[managerIndex] = rotation.value;
                this.sizes[managerIndex] = sprite.size;
                this.pivots[managerIndex] = sprite.pivot;
                this.colors[managerIndex] = sprite.color;

                ++this.spriteCount;
            }

            /// <summary>
            /// Sets the uvIndex of the sprite.
            /// </summary>
            /// <param name="sprite"></param>
            /// <param name="uvBufferIndex">Which UV buffer is it. Is it the first or the second?</param>
            /// <param name="value"></param>
            public void SetUvIndex(ref ComputeBufferSprite sprite, int uvBufferIndex, int value) {
                this.uvIndicesBuffers[uvBufferIndex].SetUvIndex(sprite.managerIndex.ValueOrError(), value);
            } 

            /// <summary>
            /// Removes the sprite at the specified manager index.
            /// We provide this method since the cleanup component of removed sprites
            /// would only have this index
            /// </summary>
            /// <param name="managerIndex"></param>
            public void Remove(int managerIndex) {
                // The inactive list should not have this index yet
                Assert.IsFalse(this.inactiveList.Contains(managerIndex));
                
                this.translationsAndScales[managerIndex] = new float4(10000, 10000, 10000, 0);
                this.rotations[managerIndex] = quaternion.identity.value;
                this.sizes[managerIndex] = new float2();
                this.pivots[managerIndex] = new float2();
                this.colors[managerIndex] = new Color(0, 0, 0, 0);
                
                this.inactiveList.Add(managerIndex);

                --this.spriteCount;
            }

            public void AddUvIndicesBuffer(string shaderPropertyId) {
                UvIndicesBuffer uvIndicesBuffer = new(shaderPropertyId, this.capacity);
                uvIndicesBuffer.SetBuffer(this.material); // Set the buffer to the material on add
                
                this.uvIndicesBuffers.Add(uvIndicesBuffer);
            }

            private void Expand() {
                this.capacity <<= 1; // Multiply by 2
                
                const int floatSize = sizeof(float);
                const int float2Size = floatSize * 2;
                const int float4Size = floatSize * 4;
                
                // Copy existing arrays to the new one
                Expand(ref this.translationsAndScales, ref this.translationAndScaleBuffer, float4Size);
                
                Expand(ref this.rotations, ref this.rotationBuffer, float4Size);

                Expand(ref this.sizes, ref this.sizeBuffer, float2Size);
                
                Expand(ref this.pivots, ref this.pivotBuffer, float2Size);

                Expand(ref this.colors, ref this.colorBuffer, float4Size);
                
                // Expand UV indices as well
                for (int i = 0; i < this.uvIndicesBuffers.Count; i++) {
                    this.uvIndicesBuffers[i].Expand(this.capacity);
                }
                
                SetMaterialBuffers();
                
                // Update capacity in args
                this.args[1] = (uint)this.capacity;
                this.argsBuffer.SetData(this.args);
            }

            private void Expand<T>(ref NativeArray<T> array, ref ComputeBuffer computeBuffer, int stride) where T : unmanaged {
                NativeArray<T> newArray = array.CopyAndExpand(this.capacity);
                array.Dispose();
                array = newArray;
                
                computeBuffer.Release();
                computeBuffer = new ComputeBuffer(this.capacity, stride);
                computeBuffer.SetData(array);
            }

            public int UvIndicesBufferCount => this.uvIndicesBuffers.Count;
            
            public NativeArray<int> GetUvBufferIndices(int uvBufferIndex) {
                return this.uvIndicesBuffers[uvBufferIndex].Indices;
            }

            public void Draw(Bounds bounds) {
                this.translationAndScaleBuffer.SetData(this.translationsAndScales);
                this.rotationBuffer.SetData(this.rotations);
                this.sizeBuffer.SetData(this.sizes);
                this.pivotBuffer.SetData(this.pivots);
                this.colorBuffer.SetData(this.colors);

                // Update the data of indices as well
                for (int i = 0; i < this.uvIndicesBuffers.Count; i++) {
                    this.uvIndicesBuffers[i].SetBufferData();
                }
                
                Graphics.DrawMeshInstancedIndirect(this.quad, 0, this.material, bounds, this.argsBuffer);
            }
        }

        public bool Equals(ComputeBufferSpriteManager other) {
            return this.id == other.id;
        }

        public override bool Equals(object obj) {
            return obj is ComputeBufferSpriteManager other && Equals(other);
        }

        public override int GetHashCode() {
            return this.id;
        }
    }
}