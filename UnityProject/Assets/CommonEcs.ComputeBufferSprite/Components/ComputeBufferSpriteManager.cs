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

        /// <summary>
        /// Adds a sprite. Returns the manager index of the sprite after adding.
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="uvIndex"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        public int Add(ref ComputeBufferSprite sprite, float3 position, quaternion rotation, float scale) {
            return this.internalInstance.Add(ref sprite, position, rotation, scale);
        }

        public int OccupiedCount => this.internalInstance.occupiedCount;

        /// <summary>
        /// Sets the uvIndex of the sprite.
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="managerIndex"></param>
        /// <param name="uvBufferIndex">Which UV buffer is it. Is it the first or the second?</param>
        /// <param name="value"></param>
        public void SetUvIndex(int managerIndex, int uvBufferIndex, int value) {
            this.internalInstance.SetUvIndex(managerIndex, uvBufferIndex, value);
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
        public NativeArray<int> ActiveArray => this.internalInstance.activeArray;
        public NativeArray<int> LayerOrderArray => this.internalInstance.layerOrderArray;
        public NativeArray<int> SortedIndices => this.internalInstance.sortedIndices;

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
            
            // Packed buffers. Each holds sections of capacity length so that the shader stays within
            // WebGPU's limit of 8 storage buffers per shader stage.
            // float4Buffer: translationsAndScales | rotations | colors
            // float2Buffer: sizes | pivots
            // intBuffer: sortedIndices | activeArray | layerOrderArray
            private ComputeBuffer float4Buffer;
            private ComputeBuffer float2Buffer;
            private ComputeBuffer intBuffer;
            
            // Matrix here is a compressed transform information
            // xy is the position, z is rotation, w is the scale
            // private ComputeBuffer translationAndScaleBuffer;
            public NativeArray<float4> translationsAndScales;
            public NativeArray<float4> rotations;
            public NativeArray<float2> sizes;
            public NativeArray<float2> pivots;
            public NativeArray<Color> colors;
            public NativeArray<int> activeArray;
            public NativeArray<int> layerOrderArray;

            // We need this to index in each buffer such that we don't need to sort each
            // The sorted order will be stored here
            // private ComputeBuffer sortedIndicesBuffer;
            public NativeArray<int> sortedIndices;
            
            private readonly uint[] args;
            private readonly ComputeBuffer argsBuffer;

            // We did it this way because there can be multiple UV buffers
            private readonly List<UvIndicesBuffer> uvIndicesBuffers = new(2);

            // We're only managing the removed manager indices here instead of the whole Sprite values
            private NativeList<int> inactiveList;

            private int capacity;
            
            // This is different from spriteCount. Note that when we remove a sprite, we don't remove it from the
            // arrays. We set the values at an index to not render anything. We use mainly use this to get the next
            // sprite index. This is also considered as activeSpriteCount + inactiveSpriteCount.
            internal int occupiedCount;

            private int spriteCount;

            // Shader variable IDs
            private readonly int uvBufferId;
            private readonly int float4BufferId;
            private readonly int float2BufferId;
            private readonly int intBufferId;
            private readonly int capacityId;

            public Internal(Material material, NativeArray<float4> uvValues, int initialCapacity) {
                this.material = material;
                this.capacity = math.max(initialCapacity, 2); // Prevents error when initialCapacity is zero
                this.quad = MeshUtils.Quad(1.0f);

                const int floatSize = sizeof(float);
                this.uvBuffer = new ComputeBuffer(uvValues.Length, floatSize * 4);
                this.uvValues = new NativeArray<float4>(uvValues.Length, Allocator.Persistent);
                this.uvValues.CopyFrom(uvValues);
                this.uvBuffer.SetData(this.uvValues);
                
                this.translationsAndScales = new NativeArray<float4>(this.capacity, Allocator.Persistent);
                this.rotations = new NativeArray<float4>(this.capacity, Allocator.Persistent);
                this.sizes = new NativeArray<float2>(this.capacity, Allocator.Persistent);
                this.pivots = new NativeArray<float2>(this.capacity, Allocator.Persistent);
                this.colors = new NativeArray<Color>(this.capacity, Allocator.Persistent);
                this.activeArray = new NativeArray<int>(this.capacity, Allocator.Persistent);
                this.layerOrderArray = new NativeArray<int>(this.capacity, Allocator.Persistent);
                this.sortedIndices = new NativeArray<int>(this.capacity, Allocator.Persistent);
                
                // Prepare the shader IDs
                this.uvBufferId = Shader.PropertyToID("uvBuffer");
                this.float4BufferId = Shader.PropertyToID("float4Buffer");
                this.float2BufferId = Shader.PropertyToID("float2Buffer");
                this.intBufferId = Shader.PropertyToID("intBuffer");
                this.capacityId = Shader.PropertyToID("_Capacity");

                CreatePackedBuffers();
                UploadPackedBuffers();
                SetMaterialParameters();

                this.args = new uint[] {
                    6, (uint)this.capacity, 0, 0, 0
                };
                this.argsBuffer = new ComputeBuffer(1, this.args.Length * sizeof(uint),
                    ComputeBufferType.IndirectArguments);
                this.argsBuffer.SetData(this.args);

                this.inactiveList = new NativeList<int>(10, Allocator.Persistent);
            }

            private void CreatePackedBuffers() {
                const int floatSize = sizeof(float);
                this.float4Buffer = new ComputeBuffer(this.capacity * 3, floatSize * 4);
                this.float2Buffer = new ComputeBuffer(this.capacity * 2, floatSize * 2);
                this.intBuffer = new ComputeBuffer(this.capacity * 3, sizeof(int));
            }

            private void UploadPackedBuffers() {
                int c = this.capacity;
                this.float4Buffer.SetData(this.translationsAndScales, 0, 0, c);
                this.float4Buffer.SetData(this.rotations, 0, c, c);
                this.float4Buffer.SetData(this.colors, 0, c * 2, c);
                
                this.float2Buffer.SetData(this.sizes, 0, 0, c);
                this.float2Buffer.SetData(this.pivots, 0, c, c);
                
                this.intBuffer.SetData(this.sortedIndices, 0, 0, c);
                this.intBuffer.SetData(this.activeArray, 0, c, c);
                this.intBuffer.SetData(this.layerOrderArray, 0, c * 2, c);
            }

            private void SetMaterialParameters() {
                this.material.SetBuffer(this.uvBufferId, this.uvBuffer);
                this.material.SetBuffer(this.float4BufferId, this.float4Buffer);
                this.material.SetBuffer(this.float2BufferId, this.float2Buffer);
                this.material.SetBuffer(this.intBufferId, this.intBuffer);
                this.material.SetInteger(this.capacityId, this.capacity);

                for (int i = 0; i < this.uvIndicesBuffers.Count; i++) {
                    this.uvIndicesBuffers[i].SetBuffer(this.material);
                }
            }

            public void Dispose() {
                this.uvBuffer.Release();
                this.float4Buffer.Release();
                this.float2Buffer.Release();
                this.intBuffer.Release();
                this.argsBuffer.Release();
                
                this.uvValues.Dispose();
                this.translationsAndScales.Dispose();
                this.rotations.Dispose();
                this.sizes.Dispose();
                this.pivots.Dispose();
                this.colors.Dispose();
                this.activeArray.Dispose();
                this.layerOrderArray.Dispose();
                this.sortedIndices.Dispose();
                
                this.inactiveList.Dispose();
                
                // Dispose UV indices
                for (int i = 0; i < this.uvIndicesBuffers.Count; i++) {
                    this.uvIndicesBuffers[i].Dispose();
                }
            }

            /// <summary>
            /// Adds a sprite. Returns the manager index of the sprite.
            /// </summary>
            /// <param name="sprite"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            /// <param name="scale"></param>
            public int Add(ref ComputeBufferSprite sprite, float3 position, quaternion rotation, float scale) {
                // Check if there are inactive sprite slots and use those first
                if (this.inactiveList.Length > 0) {
                    return AddByReusingInactive(ref sprite, position, rotation, scale);
                }

                // Expand if we're out of space
                while (this.occupiedCount >= this.capacity) {
                    Expand();
                }

                int managerIndex = this.occupiedCount;
                InternalAdd(ref sprite, managerIndex, position, rotation, scale);
                return managerIndex;
            }

            private int AddByReusingInactive(ref ComputeBufferSprite sprite, float3 position, quaternion rotation, float scale) {
                Assertion.IsTrue(this.inactiveList.Length > 0);
                
                int lastIndex = this.inactiveList.Length - 1;
                int reusedManagerIndex = this.inactiveList[lastIndex];
                this.inactiveList.RemoveAt(lastIndex);

                InternalAdd(ref sprite, reusedManagerIndex, position, rotation, scale);

                return reusedManagerIndex;
            }

            private void InternalAdd(ref ComputeBufferSprite sprite, int managerIndex, float3 position, quaternion rotation, float scale) {
                this.translationsAndScales[managerIndex] = new float4(position, scale);
                this.rotations[managerIndex] = rotation.value;
                this.sizes[managerIndex] = sprite.size;
                this.pivots[managerIndex] = sprite.pivot;
                this.colors[managerIndex] = sprite.color;

                // We do this for now but this should be sorted before rendering
                this.sortedIndices[managerIndex] = managerIndex;

                ++this.occupiedCount;
                ++this.spriteCount;
                
                // Update the amount of quads to draw in args
                this.args[1] = (uint)this.occupiedCount;
                this.argsBuffer.SetData(this.args);
            }

            /// <summary>
            /// Sets the uvIndex of the sprite.
            /// </summary>
            /// <param name="managerIndex"></param>
            /// <param name="uvBufferIndex">Which UV buffer is it. Is it the first or the second?</param>
            /// <param name="value"></param>
            public void SetUvIndex(int managerIndex, int uvBufferIndex, int value) {
                this.uvIndicesBuffers[uvBufferIndex].SetUvIndex(managerIndex, value);
            } 

            /// <summary>
            /// Removes the sprite at the specified manager index.
            /// We provide this method since the cleanup component of removed sprites
            /// would only have this index
            /// </summary>
            /// <param name="managerIndex"></param>
            public void Remove(int managerIndex) {
                // The inactive list should not have this index yet
                DotsAssert.IsFalse(this.inactiveList.Contains(managerIndex));
                
                this.translationsAndScales[managerIndex] = new float4(10000, 10000, 10000, 0);
                this.rotations[managerIndex] = quaternion.identity.value;
                this.sizes[managerIndex] = new float2();
                this.pivots[managerIndex] = new float2();
                this.colors[managerIndex] = new Color(0, 0, 0, 0);
                
                this.inactiveList.Add(managerIndex);

                --this.spriteCount;
                
                // Note here that we don't decrement occupiedCount. This number doesn't go down since the removed
                // sprite are still there.
            }

            public void AddUvIndicesBuffer(string shaderPropertyId) {
                UvIndicesBuffer uvIndicesBuffer = new(shaderPropertyId, this.capacity);
                uvIndicesBuffer.SetBuffer(this.material); // Set the buffer to the material on add
                
                this.uvIndicesBuffers.Add(uvIndicesBuffer);
            }

            private void Expand() {
                this.capacity <<= 1; // Multiply by 2
                
                // Copy existing arrays to the new one
                Expand(ref this.translationsAndScales);
                Expand(ref this.rotations);
                Expand(ref this.sizes);
                Expand(ref this.pivots);
                Expand(ref this.colors);
                Expand(ref this.activeArray);
                Expand(ref this.layerOrderArray);
                Expand(ref this.sortedIndices);
                
                // Recreate the packed buffers since the capacity changed
                this.float4Buffer.Release();
                this.float2Buffer.Release();
                this.intBuffer.Release();
                
                CreatePackedBuffers();
                UploadPackedBuffers();
                
                // Expand UV indices as well
                for (int i = 0; i < this.uvIndicesBuffers.Count; i++) {
                    this.uvIndicesBuffers[i].Expand(this.capacity);
                }
                
                SetMaterialParameters();
            }

            private void Expand<T>(ref NativeArray<T> array) where T : unmanaged {
                NativeArray<T> newArray = array.CopyAndExpand(this.capacity);
                array.Dispose();
                array = newArray;
            }

            public int UvIndicesBufferCount => this.uvIndicesBuffers.Count;
            
            public NativeArray<int> GetUvBufferIndices(int uvBufferIndex) {
                return this.uvIndicesBuffers[uvBufferIndex].Indices;
            }

            public void Draw(Bounds bounds) {
                UploadPackedBuffers();

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