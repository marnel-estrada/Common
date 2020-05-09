using System;

using Common;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Rendering;

namespace CommonEcs {
    public struct ComputeBufferDrawInstance : ISharedComponentData, IEquatable<ComputeBufferDrawInstance> {
        private readonly int id;
        private static readonly IdGenerator ID_GENERATOR = new IdGenerator(1);
        
        private readonly InternalImplementation internalInstance;
        
        // We chose this number because this will be use by Academia which has 81900
        // ground sprites from the beginning
        public const int INITIAL_CAPACITY = 128000;

        public ComputeBufferDrawInstance(Entity owner, Mesh mesh, Material material, int initialCapacity = INITIAL_CAPACITY, bool alwaysUpdate = false) : 
            this(owner, mesh, material, new Bounds(VectorUtils.ZERO, VectorUtils.ONE * 1000), initialCapacity, alwaysUpdate) {
        }
        
        public ComputeBufferDrawInstance(Entity owner, Mesh mesh, Material material, Bounds boundingBox, int initialCapacity = INITIAL_CAPACITY, bool alwaysUpdate = false) {
            this.id = ID_GENERATOR.Generate();
            this.internalInstance = new InternalImplementation(owner, mesh, material, boundingBox, initialCapacity, alwaysUpdate);
        }

        public void Add(ref ComputeBufferSprite sprite) {
            this.internalInstance.Add(ref sprite);
        }

        public void Remove(int masterListIndex) {
            this.internalInstance.Remove(masterListIndex);
        }

        public Entity Owner {
            get {
                return this.internalInstance.owner;
            }
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

        public bool SomethingChanged {
            get {
                return this.internalInstance.renderOrderChanged || this.internalInstance.transformChanged ||
                    this.internalInstance.uvChanged || this.internalInstance.colorChanged ||
                    this.internalInstance.sizePivotChanged;
            }
        }

        public bool AlwaysUpdateBuffers {
            get {
                return this.internalInstance.alwaysUpdate;
            }
        }

        public bool RenderOrderChanged {
            get {
                return this.internalInstance.renderOrderChanged;
            }

            set {
                this.internalInstance.renderOrderChanged = value;
            }
        }

        public bool TransformChanged {
            get {
                return this.internalInstance.transformChanged;
            }

            set {
                this.internalInstance.transformChanged = value;
            }
        }

        /// <summary>
        /// The camera where the draw instance would be rendered to
        /// </summary>
        public Camera Camera {
            get {
                return this.internalInstance.camera;
            }

            set {
                this.internalInstance.camera = value;
            }
        }
        
        public void UpdateBuffers() {
            this.internalInstance.UpdateBuffers();
        }

        /// <summary>
        /// We pass in mesh here because it's a common mesh
        /// </summary>
        /// <param name="quad"></param>
        public void Draw() {
            this.internalInstance.Draw();
        }
        
        public void Dispose() {
            this.internalInstance.Dispose();
        }

        public class InternalImplementation {
            // The entity where this draw instance is associated with
            public readonly Entity owner;

            // This is the mesh used by the draw instance
            // It could be different like square sprites or diamond sprites for isometric
            private Mesh mesh;
            
            // We don't set as readonly as it should be able to be changed at runtime
            private Material material;

            // The bounding box of the whole draw instance
            private Bounds boundingBox;

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

            public bool transformChanged;
            public bool uvChanged;
            public bool colorChanged;
            public bool sizePivotChanged;
            public bool renderOrderChanged;
            
            // This is used for layers that always moves
            // If set to true, it will no longer run IdentifyDrawInstanceChangedSystem
            public readonly bool alwaysUpdate;
            
            private readonly int transformBufferId;
            private readonly int rotationBufferId;
            private readonly int uvBufferId;
            private readonly int colorsBufferId;
            private readonly int sizePivotBufferId;

            public int capacity;
            private int spriteCount;
            
            // We're only managing the removed manager indeces here instead of the whole Sprite values
            private readonly SimpleList<int> inactiveList = new SimpleList<int>(100);

            // Camera to where the draw instance would be rendered to
            public Camera camera;

            public InternalImplementation(Entity owner, Mesh mesh, Material material, Bounds boundingBox, int initialCapacity, bool alwaysUpdate) {
                this.owner = owner;
                this.mesh = mesh;
                this.material = material;

                this.alwaysUpdate = alwaysUpdate;
                this.boundingBox = boundingBox;

                this.capacity = initialCapacity;
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
                // Cap capacity to MAX_SPRITE_COUNT
                this.capacity = math.min(this.capacity * 2, MAX_SPRITE_COUNT);
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

            public void UpdateBuffers() {
                // We update all buffers if render order is changed because the values for each sprite
                // might have moved
                if (this.renderOrderChanged || this.alwaysUpdate) {
                    UpdateAllBuffers();
                    this.renderOrderChanged = false;
                    return;
                }

                // Update transform only if it has changed
                if (this.transformChanged) {
                    UpdateTransformsBuffer();
                }
            }

            private void UpdateAllBuffers() {
                UpdateTransformsBuffer();
                UpdateUvBuffer();
                UpdateColorBuffer();
                UpdateSizePivotBuffer();

                // Note here that we use the masterList's length as the draw count as their may be
                // sprites in between the masterlist that are already destroyed
                this.args[1] = (uint) this.spritesMasterList.Length;
                this.argsBuffer.SetData(this.args);
            }

            private void UpdateSizePivotBuffer() {
                this.sizePivotBuffer.SetData(this.sizePivots);
                this.material.SetBuffer(this.sizePivotBufferId, this.sizePivotBuffer);

                this.sizePivotChanged = false;
            }

            private void UpdateColorBuffer() {
                this.colorBuffer.SetData(this.colors);
                this.material.SetBuffer(this.colorsBufferId, this.colorBuffer);

                this.colorChanged = false;
            }

            private void UpdateUvBuffer() {
                this.uvBuffer.SetData(this.uvs);
                this.material.SetBuffer(this.uvBufferId, this.uvBuffer);

                this.uvChanged = false;
            }

            private void UpdateTransformsBuffer() {
                this.transformBuffer.SetData(this.transforms);
                this.material.SetBuffer(this.transformBufferId, this.transformBuffer);

                this.rotationBuffer.SetData(this.rotations);
                this.material.SetBuffer(this.rotationBufferId, this.rotationBuffer);

                this.transformChanged = false;
            }

            /// <summary>
            /// We pass in mesh here because it's a common mesh
            /// </summary>
            /// <param name="quad"></param>
            public void Draw() {
                Graphics.DrawMeshInstancedIndirect(this.mesh, 0, this.material, this.boundingBox, this.argsBuffer, 
                    0, null, ShadowCastingMode.Off, false, 0, this.camera, LightProbeUsage.Off, null);
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