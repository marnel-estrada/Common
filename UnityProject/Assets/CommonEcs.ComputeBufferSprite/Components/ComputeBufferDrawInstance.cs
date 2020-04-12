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

        public void Remove(int masterListIndex) {
            this.internalInstance.Remove(masterListIndex);
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

        public bool RenderOrderChanged {
            get {
                return this.internalInstance.renderOrderChanged;
            }

            set {
                this.internalInstance.renderOrderChanged = false;
            }
        }
        
        public void SetDataToBuffers() {
            this.internalInstance.SetDataToBuffers();
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
            private readonly ComputeBuffer transformBuffer;
            private readonly ComputeBuffer rotationBuffer; 
            
            private readonly ComputeBuffer sizePivotBuffer;
            private readonly ComputeBuffer uvBuffer;
            private readonly ComputeBuffer colorBuffer;
            
            // Arrays
            public NativeArray<float4> transforms;
            public NativeArray<float> rotations;
            public NativeArray<float4> sizePivots;
            public NativeArray<float4> uvs;
            public NativeArray<float4> colors;

            private NativeArray<uint> args;
            private readonly ComputeBuffer argsBuffer;

            // We set only to this number as there's a certain limit to the ComputeBuffer 
            // ArgumentException: ComputeBuffer.SetData() : Accessing 16384000 bytes at offset 0
            // for Compute Buffer of size 16000000 bytes is not possible.
            public const int MAX_SPRITE_COUNT = 512000;

            public bool uvChanged;
            public bool colorChanged;
            public bool renderOrderChanged;
            
            private readonly int transformBufferId;
            private readonly int rotationBufferId;
            private readonly int uvBufferId;
            private readonly int colorsBufferId;
            private readonly int sizePivotBufferId;

            public int capacity;
            private int spriteCount;

            // We chose this number because this will be use by Academia which has 81900
            // ground sprites from the beginning
            private const int INITIAL_CAPACITY = 128000;
            
            // We're only managing the removed manager indeces here instead of the whole Sprite values
            private readonly SimpleList<int> inactiveList = new SimpleList<int>(100);

            public InternalImplementation(Material material) {
                this.material = material;

                this.capacity = INITIAL_CAPACITY;
                this.spritesMasterList = new NativeList<ComputeBufferSprite>(this.capacity, Allocator.Persistent);
                ExpandArrays(this.capacity);

                this.transformBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, 16);
                this.rotationBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, sizeof(float));
                this.sizePivotBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, 16);
                this.uvBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, 16);
                this.colorBuffer = new ComputeBuffer(MAX_SPRITE_COUNT, 16);
                
                this.transformBufferId = Shader.PropertyToID("transformBuffer");
                this.rotationBufferId = Shader.PropertyToID("rotationBuffer");
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
                
                if (this.inactiveList.Count > 0) {
                    AddByReusingInactive(ref sprite);
                    return;
                }
                
                // At this point, we add by using a new entry in the list
                sprite.masterListIndex = this.spriteCount;
                this.spritesMasterList.Add(sprite);

                ++this.spriteCount;
                this.renderOrderChanged = true;
            }
            
            // Adds a new sprite by reusing an existing index in the master list
            private void AddByReusingInactive(ref ComputeBufferSprite sprite) {
                Assertion.Assert(this.inactiveList.Count > 0);

                // We really only need the manager index from the removed sprites
                int lastIndex = this.inactiveList.Count - 1;
                int reusedIndex = this.inactiveList[lastIndex];
                this.inactiveList.RemoveAt(lastIndex);
                sprite.masterListIndex = reusedIndex;
                this.spritesMasterList[reusedIndex] = sprite;
                
                ++this.spriteCount;
                this.renderOrderChanged = true;
            }

            /// <summary>
            /// We only provide remove by master list index because systems may not always have a
            /// reference to the ComputeBufferSprite of the destroyed entity.
            /// An AddRegistry may remember the master list index, though.
            /// </summary>
            /// <param name="masterListIndex"></param>
            public void Remove(int masterListIndex) {
                // The inactive list should not have this index yet
                Assertion.Assert(!this.inactiveList.Contains(masterListIndex));
                
                // We empty out the sprite in the specified index so that nothing will be rendered
                this.spritesMasterList[masterListIndex] = new ComputeBufferSprite();
                
                this.inactiveList.Add(masterListIndex);

                --this.spriteCount;
                this.renderOrderChanged = true;
            }

            private void ExpandArrays() {
                this.capacity *= 2;
                ExpandArrays(this.capacity);
            }
            
            private void ExpandArrays(int count) {
                if (this.transforms.IsCreated && this.transforms.Length >= count) {
                    // Current arrays can still accommodate the specified number of sprites
                    return;
                }
                
                // Dispose old
                if (this.transforms.IsCreated) {
                    this.transforms.Dispose();
                    this.rotations.Dispose();
                    this.sizePivots.Dispose();
                    this.uvs.Dispose();
                    this.colors.Dispose();
                }
                
                this.transforms = new NativeArray<float4>(count, Allocator.Persistent);
                this.rotations = new NativeArray<float>(count, Allocator.Persistent);
                this.sizePivots = new NativeArray<float4>(count, Allocator.Persistent);
                this.uvs = new NativeArray<float4>(count, Allocator.Persistent);
                this.colors = new NativeArray<float4>(count, Allocator.Persistent);
            }

            public void SetDataToBuffers() {
                // Update the buffers
                this.transformBuffer.SetData(this.transforms);
                this.material.SetBuffer(this.transformBufferId, this.transformBuffer);
                
                this.rotationBuffer.SetData(this.rotations);
                this.material.SetBuffer(this.rotationBufferId, this.rotationBuffer);

                this.uvBuffer.SetData(this.uvs);
                this.material.SetBuffer(this.uvBufferId, this.uvBuffer);

                this.colorBuffer.SetData(this.colors);
                this.material.SetBuffer(this.colorsBufferId, this.colorBuffer);
            
                this.sizePivotBuffer.SetData(this.sizePivots);
                this.material.SetBuffer(this.sizePivotBufferId, this.sizePivotBuffer);
                
                // Note here that we use the masterList's length as the draw count as their may be
                // sprites in between the masterlist that are already destroyed
                this.args[1] = (uint) this.spritesMasterList.Length;
                this.argsBuffer.SetData(this.args);
            }

            private static readonly Bounds BOUNDS = new Bounds(Vector2.zero, Vector3.one);

            /// <summary>
            /// We pass in mesh here because it's a common mesh
            /// </summary>
            /// <param name="quad"></param>
            public void Draw(Mesh quad) {
                Graphics.DrawMeshInstancedIndirect(quad, 0, this.material, BOUNDS, this.argsBuffer);
            }
            
            public void Dispose() {
                this.spritesMasterList.Dispose();
                
                this.transforms.Dispose();
                this.rotations.Dispose();
                this.sizePivots.Dispose();
                this.uvs.Dispose();
                this.colors.Dispose();
                this.args.Dispose();
                
                this.transformBuffer.Release();
                this.rotationBuffer.Release();
                this.sizePivotBuffer.Release();
                this.uvBuffer.Release();
                this.colorBuffer.Release();
                this.argsBuffer.Release();
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