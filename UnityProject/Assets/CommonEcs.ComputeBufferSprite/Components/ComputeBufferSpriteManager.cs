using System;
using System.Collections.Generic;
using Common;
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

        public void Draw(Bounds bounds) {
            this.internalInstance.Draw(bounds);
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
            private ComputeBuffer translationAndRotationBuffer;
            private NativeArray<float4> translationAndRotations; 
            
            private ComputeBuffer scaleBuffer;
            private NativeArray<float> scales;
        
            private ComputeBuffer colorBuffer;
            private NativeArray<Color> colors;
            
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
            private readonly int translationAndRotationsBufferId;
            private readonly int scalesBufferId;
            private readonly int colorsBufferId;

            public Internal(Material material, NativeArray<float4> uvValues, int initialCapacity) {
                this.material = material;
                this.capacity = initialCapacity;
                this.quad = MeshUtils.Quad(1.0f);

                const int floatSize = sizeof(float);
                const int float4Size = sizeof(float) * 4;
                
                this.uvBuffer = new ComputeBuffer(uvValues.Length, float4Size);
                this.uvValues = new NativeArray<float4>(uvValues.Length, Allocator.Persistent);
                this.uvValues.CopyFrom(uvValues);
                this.uvBuffer.SetData(this.uvValues);
                
                this.translationAndRotationBuffer = new ComputeBuffer(this.capacity, float4Size);
                this.translationAndRotations = new NativeArray<float4>(this.capacity, Allocator.Persistent);
                this.translationAndRotationBuffer.SetData(this.translationAndRotations);

                this.scaleBuffer = new ComputeBuffer(this.capacity, floatSize);
                this.scales = new NativeArray<float>(this.capacity, Allocator.Persistent);
                this.scaleBuffer.SetData(this.scales);

                this.colorBuffer = new ComputeBuffer(this.capacity, float4Size);
                this.colors = new NativeArray<Color>(this.capacity, Allocator.Persistent);
                this.colorBuffer.SetData(this.colors);
                
                // Prepare the shader IDs
                this.uvBufferId = Shader.PropertyToID("uvBuffer");
                this.translationAndRotationsBufferId = Shader.PropertyToID("translationAndRotationBuffer");
                this.scalesBufferId = Shader.PropertyToID("scaleBuffer");
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
                this.material.SetBuffer(this.translationAndRotationsBufferId, this.translationAndRotationBuffer);
                this.material.SetBuffer(this.scalesBufferId, this.scaleBuffer);
                this.material.SetBuffer(this.colorsBufferId, this.colorBuffer);

                for (int i = 0; i < this.uvIndicesBuffers.Count; i++) {
                    this.uvIndicesBuffers[i].SetBuffer(this.material);
                }
            }

            public void Dispose() {
                this.uvBuffer.Release();
                this.translationAndRotationBuffer.Release();
                this.scaleBuffer.Release();
                this.colorBuffer.Release();
                this.argsBuffer.Release();
                
                this.uvValues.Dispose();
                this.translationAndRotations.Dispose();
                this.scales.Dispose();
                this.colors.Dispose();
                this.inactiveList.Dispose();
                
                // Dispose UV indices
                for (int i = 0; i < this.uvIndicesBuffers.Count; i++) {
                    this.uvIndicesBuffers[i].Dispose();
                }
            }

            /// <summary>
            /// Adds a sprite with a single uv index.
            /// </summary>
            /// <param name="sprite"></param>
            /// <param name="uvIndex"></param>
            public void Add(ref ComputeBufferSprite sprite, int uvIndex) {
                // TODO Check if there are inactive sprite index
            }

            public void AddUvIndicesBuffer(string shaderPropertyId) {
                UvIndicesBuffer uvIndicesBuffer = new(shaderPropertyId, this.capacity);
                uvIndicesBuffer.SetBuffer(this.material); // Set the buffer to the material on add
                
                this.uvIndicesBuffers.Add(uvIndicesBuffer);
            }

            private void Expand() {
                this.capacity <<= 1; // Multiply by 2
                
                const int floatSize = sizeof(float);
                const int float4Size = sizeof(float) * 4;
                
                // Copy existing arrays to the new one
                NativeArray<float4> newTranslationAndRotations =
                    this.translationAndRotations.CopyAndExpand(this.capacity);
                this.translationAndRotations.Dispose();
                this.translationAndRotations = newTranslationAndRotations;
                
                this.translationAndRotationBuffer.Release();
                this.translationAndRotationBuffer = new ComputeBuffer(this.capacity, float4Size);
                this.translationAndRotationBuffer.SetData(this.translationAndRotations);

                NativeArray<float> newScales = this.scales.CopyAndExpand(this.capacity);
                this.scales.Dispose();
                this.scales = newScales;
                
                this.scaleBuffer.Release();
                this.scaleBuffer = new ComputeBuffer(this.capacity, floatSize);
                this.scaleBuffer.SetData(this.scales);

                NativeArray<Color> newColors = this.colors.CopyAndExpand(this.capacity);
                this.colors.Dispose();
                this.colors = newColors;
                
                this.colorBuffer.Release();
                this.colorBuffer = new ComputeBuffer(this.capacity, float4Size);
                this.colorBuffer.SetData(this.colors);
                
                // Expand UV indices as well
                for (int i = 0; i < this.uvIndicesBuffers.Count; i++) {
                    this.uvIndicesBuffers[i].Expand(this.capacity);
                }
                
                SetMaterialBuffers();
                
                // Update capacity in args
                this.args[1] = (uint)this.capacity;
                this.argsBuffer.SetData(this.args);
            }

            public void Draw(Bounds bounds) {
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