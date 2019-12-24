﻿using System;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

using Common;

namespace CommonEcs {
    public struct SpriteManager : ISharedComponentData, IEquatable<SpriteManager> {
        private readonly Internal internalInstance;
        
        private readonly int id;
        private static readonly IdGenerator ID_GENERATOR = new IdGenerator(1);

        /// <summary>
        /// Initializes the sprite manager
        /// </summary>
        /// <param name="allocationCount"></param>
        public SpriteManager(int allocationCount) {
            this.internalInstance = new Internal();
            this.internalInstance.Init(allocationCount);
            this.id = ID_GENERATOR.Generate();
        }

        /// <summary>
        /// Another version of Init() that accepts an EntityCommandBuffer
        /// </summary>
        /// <param name="allocationCount"></param>
        /// <param name="commands"></param>
        public SpriteManager(int allocationCount, EntityCommandBuffer commands) {
            this.internalInstance = new Internal();
            this.internalInstance.Init(allocationCount, commands);
            this.id = ID_GENERATOR.Generate();
        }

        /// <summary>
        /// Updates the mesh
        /// </summary>
        public void UpdateMesh() {
            this.internalInstance.UpdateMesh();
        }

        public bool MeshChanged {
            get {
                return this.internalInstance.MeshChanged;
            }
        }

        /// <summary>
        /// Adds the specified sprite into the manager (into the big mesh)
        /// </summary>
        /// <param name="sprite"></param>
        public void Add(ref Sprite sprite, float4x4 matrix) {
            this.internalInstance.Add(ref sprite, matrix);
        }

        /// <summary>
        /// Removes the specified sprite from the manager
        /// </summary>
        /// <param name="sprite"></param>
        public void Remove(Sprite sprite) {
            this.internalInstance.Remove(sprite);
        }

        /// <summary>
        /// Removes the sprite with the specified manager index
        /// </summary>
        /// <param name="managerIndex"></param>
        public void Remove(int managerIndex) {
            this.internalInstance.Remove(managerIndex);
        }

        /// <summary>
        /// Sets the material for the whole mesh
        /// </summary>
        /// <param name="material"></param>
        public void SetMaterial(Material material) {
            this.internalInstance.material = material;
            
            // We set these flags to true so it will be rendered in SpriteManagerRenderer
            this.internalInstance.verticesChanged = true;
            this.internalInstance.renderOrderChanged = true;
            this.internalInstance.uvChanged = true;
            this.internalInstance.colorsChanged = true;
        }

        public Mesh Mesh {
            get {
                return this.internalInstance.mesh;
            }
        }

        public Material Material {
            get {
                return this.internalInstance.material;
            }
        }

        public NativeArray<Vector3> NativeVertices {
            get {
                return this.internalInstance.nativeVertices;
            }
        }

        public NativeList<SortedSpriteEntry> SortList {
            get {
                return this.internalInstance.sortList;
            }
        }

        public NativeArray<int> NativeTriangles {
            get {
                return this.internalInstance.nativeTriangles;
            }
        }

        public NativeArray<Vector2> NativeUv {
            get {
                return this.internalInstance.nativeUv;
            }
        }

        public NativeArray<Vector2> NativeUv2 {
            get {
                return this.internalInstance.nativeUv2;
            }
        }

        public NativeArray<Color> NativeColors {
            get {
                return this.internalInstance.nativeColors;
            }
        }

        public int Layer {
            get {
                return this.internalInstance.layer;
            }

            set {
                this.internalInstance.layer = value;
            }
        }

        public int SortingLayer {
            get {
                return this.internalInstance.sortingLayer;
            }

            set {
                this.internalInstance.sortingLayer = value;
            }
        }
        
        public int SortingLayerId {
            get {
                return this.internalInstance.sortingLayerId;
            }

            set {
                this.internalInstance.sortingLayerId = value;
            }
        }

        public bool VerticesChanged {
            get {
                return this.internalInstance.verticesChanged;
            }

            set {
                this.internalInstance.verticesChanged = value;
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

        public bool UvChanged {
            get {
                return this.internalInstance.uvChanged;
            }

            set {
                this.internalInstance.uvChanged = value;
            }
        }

        public bool ColorsChanged {
            get {
                return this.internalInstance.colorsChanged;
            }

            set {
                this.internalInstance.colorsChanged = value;
            }
        }

        /// <summary>
        /// Returns the number of sprites added in this manager
        /// </summary>
        public int Count {
            get {
                return this.internalInstance.spriteCount;
            }
        }

        /// <summary>
        /// Sets whether or not to always update mesh
        /// This is used in cases where the sprite manager is used for non static sprites
        /// In this case, there's no use trying to determine when to update mesh
        /// Note that identifying a changed SpriteManager is accomplished by creating a
        /// new entity with a Changed tag component.
        /// </summary>
        public bool AlwaysUpdateMesh {
            get {
                return this.internalInstance.alwaysUpdateMesh;
            }

            set {
                this.internalInstance.alwaysUpdateMesh = value;
            }
        }

        public bool UseMeshRenderer {
            get {
                return this.internalInstance.useMeshRenderer;
            }

            set {
                this.internalInstance.useMeshRenderer = value;
            }
        }
        
        public bool HasAvailableSpace {
            get {
                return this.internalInstance.HasAvailableSpace;
            }
        }

        public Entity Owner {
            get {
                return this.internalInstance.owner;
            }

            set {
                // Prevent overwriting of owner entity
                Assertion.Assert(this.internalInstance.owner == Entity.Null);
                this.internalInstance.owner = value;
                Assertion.Assert(this.internalInstance.owner != Entity.Null);
            }
        }

        public Entity SpriteLayerEntity {
            get {
                return this.internalInstance.spriteLayerEntity;
            }

            set {
                this.internalInstance.spriteLayerEntity = value;
            }
        }

        public string Name {
            get {
                return this.internalInstance.name;
            }

            set {
                this.internalInstance.name = value;
            }
        }

        public bool Enabled {
            get {
                return this.internalInstance.enabled;
            }

            set {
                this.internalInstance.enabled = value;
            }
        }

        public void ResetFlags() {
            this.internalInstance.ResetFlags();
        }

        public bool Prepared {
            get {
                return this.internalInstance != null && this.internalInstance.owner != Entity.Null;
            }
        }

        public bool HasInternalInstance {
            get {
                return this.internalInstance != null;
            }
        }

        /// <summary>
        /// Disposes the SpriteManager
        /// </summary>
        public void Dispose() {
            this.internalInstance.nativeVertices.Dispose();
            this.internalInstance.sortList.Dispose();
            this.internalInstance.nativeTriangles.Dispose();
            this.internalInstance.nativeUv.Dispose();
            this.internalInstance.nativeUv2.Dispose();
            this.internalInstance.nativeColors.Dispose();
        }

        // We use an internal implementation so that when the struct gets accessed in a SharedComponentDataArray
        // we don't have to set to the array if we want to modify it.
        // Note that SharedComponentDataArray has no setter.
        private class Internal {
            public string name;
            
            // The entity that where the SpriteManager component is attached
            public Entity owner;

            public Entity spriteLayerEntity;

            public Mesh mesh;
            public Material material;
            public int buffer;

            // We maintain an array and a native one so we don't need to use ToArray() which throws garbage
            // We copy into the normal array instead
            public NativeArray<Vector3> nativeVertices;
            public Vector3[] vertices;

            public NativeArray<int> nativeTriangles;
            public int[] triangles;

            public NativeArray<Vector2> nativeUv;
            public Vector2[] uv;

            public NativeArray<Vector2> nativeUv2;
            public Vector2[] uv2;

            public NativeArray<Color> nativeColors;
            public Color[] colors;

            // This is the amount of sprites to expand whenever the current capacity is reached
            public int allocationCount;

            private int capacity;
            public int spriteCount;

            public NativeList<SortedSpriteEntry> sortList;

            public int layer;

            // We're only managing the removed manager indeces here instead of the whole Sprite values
            private readonly SimpleList<int> inactiveList = new SimpleList<int>(100);

            // Note that this is the layer value and not the ID
            public int sortingLayer;

            // This is the ID
            public int sortingLayerId;

            // Default to true so the values are copied on the first time
            public bool verticesChanged = true;
            public bool renderOrderChanged = true;
            public bool uvChanged = true;
            public bool colorsChanged = true;

            public bool vertexCountChanged = true;

            public bool alwaysUpdateMesh;
            public bool useMeshRenderer;

            public bool enabled = true; // Enabled by default

            /// <summary>
            /// Initializes the sprite manager
            /// </summary>
            /// <param name="allocationCount"></param>
            /// <param name="owner"></param>
            public void Init(int allocationCount) {
                InitVariables(allocationCount);

                // We add a dummy sprite that has zero width and height so we just prevent the artifacting
                AddInvisibleSprite();
            }

            /// <summary>
            /// Another version of Init() that accepts an EntityCommandBuffer
            /// This is used when a SpriteManager is prepared inside systems instead of outside ECS
            /// </summary>
            /// <param name="allocationCount"></param>
            /// <param name="commands"></param>
            public void Init(int allocationCount, EntityCommandBuffer commands) {
                InitVariables(allocationCount);
                
                // Note here that we can't add invisible sprite for this version since we
                // can't determine the owner
            }

            private void InitVariables(int allocationCount) {
                this.allocationCount = allocationCount;
                this.capacity = allocationCount;

                // We multiply by 4 because each sprite has 4 vertices
                int vertexCount = this.capacity * 4;
                this.nativeVertices = new NativeArray<Vector3>(vertexCount, Allocator.Persistent);
                this.vertices = new Vector3[vertexCount];

                this.nativeUv = new NativeArray<Vector2>(vertexCount, Allocator.Persistent);
                this.uv = new Vector2[vertexCount];

                this.nativeUv2 = new NativeArray<Vector2>(vertexCount, Allocator.Persistent);
                this.uv2 = new Vector2[vertexCount];

                this.nativeColors = new NativeArray<Color>(vertexCount, Allocator.Persistent);
                this.colors = new Color[vertexCount];

                // Multiply by 6 because there are 6 indeces per quad (2 triangles)
                int trianglesLength = this.capacity * 6;
                this.nativeTriangles = new NativeArray<int>(trianglesLength, Allocator.Persistent);
                this.triangles = new int[trianglesLength];

                this.mesh = new Mesh();

                this.sortList = new NativeList<SortedSpriteEntry>(allocationCount, Allocator.Persistent);

                this.spriteCount = 0;
            }

            private void AddInvisibleSprite() {
                EntityManager entityManager = World.Active.EntityManager;
                Entity spriteEntity = entityManager.CreateEntity();
                
                Sprite sprite = new Sprite();
                sprite.Init(this.owner, 0, 0, new float2(0.5f, 0.5f));
                sprite.SetUv(new float2(0, 0), new float2(0, 0));
                sprite.SetColor(new Color(0, 0, 0, 0)); // Black transparent
                entityManager.AddComponentData(spriteEntity, sprite);

                // Set as static so it will not be transformed
                entityManager.AddComponentData(spriteEntity, new Static());

                // Note here that we don't need to call Add()
                // We just let AddSpriteToManagerSystem do the adding.
            }

            /// <summary>
            /// Updates the mesh
            /// </summary>
            public void UpdateMesh() {
                // We clear only when vertex count changed
                if (this.vertexCountChanged) {
                    this.mesh.Clear();
                }

                bool forceUpdate = this.vertexCountChanged || this.alwaysUpdateMesh;

                // Set array values only if any of the values changed
                // We don't use NativeArray.ToArray() since it throws garbage. We just copy instead.
                if (forceUpdate || this.verticesChanged) {
                    this.mesh.SetVertices(this.nativeVertices);
                    this.mesh.RecalculateBounds();
                }

                if (forceUpdate || this.renderOrderChanged) {
                    this.mesh.SetIndices(this.nativeTriangles, MeshTopology.Triangles, 0);
                }

                if (forceUpdate || this.uvChanged) {
                    this.mesh.SetUVs(0, this.nativeUv);
                    this.mesh.SetUVs(1, this.nativeUv2);
                }

                if (forceUpdate || this.colorsChanged) {
                    this.mesh.SetColors(this.nativeColors);
                }

                // Resetting of flags is done in ResetSpriteManagerFlagsSystem
                // We did it this way because another system might need to use the flags
                // like for example, the system that would set the mesh to MeshRenderers
            }

            public bool MeshChanged {
                get {
                    return this.vertexCountChanged || this.alwaysUpdateMesh || this.verticesChanged ||
                        this.renderOrderChanged || this.uvChanged || this.colorsChanged;
                }
            }

            public void ResetFlags() {
                this.verticesChanged = false;
                this.renderOrderChanged = false;
                this.uvChanged = false;
                this.colorsChanged = false;
                this.vertexCountChanged = false;
            }

            // Mesh has a limit of 65535 vertices
            // Divided by 4, we get 16,383
            // Note that we're adding an invisible sprite to fix the artifact when adding new ones
            // Thus we minus one.
            private const int MAX_SPRITE_COUNT = 16382;

            /// <summary>
            /// Adds the specified sprite into the manager (into the big mesh)
            /// </summary>
            /// <param name="sprite"></param>
            /// <param name="matrix"></param>
            public void Add(ref Sprite sprite, float4x4 matrix) {
                Assertion.Assert(this.owner == sprite.spriteManagerEntity);
                
                // Check if there are inactive Sprites and use those instances first
                if (this.inactiveList.Count > 0) {
                    AddByReusingInactive(ref sprite, matrix);

                    return;
                }

                // Should not exceed the max sprite count
                Assertion.Assert(this.HasAvailableSpace);

                while (this.spriteCount >= this.capacity) {
                    // We're out of space
                    // We expand the arrays
                    Expand();
                }

                sprite.managerIndex = this.spriteCount;
                InternalAdd(ref sprite, ref matrix);
            }

            public bool HasAvailableSpace {
                get {
                    return this.spriteCount + 1 <= MAX_SPRITE_COUNT;
                }
            }

            // Adds a new sprite by reusing an existing sprite
            private void AddByReusingInactive(ref Sprite newSprite, float4x4 matrix) {
                Assertion.Assert(this.inactiveList.Count > 0);

                // We really only need the manager index from the removed sprites
                int lastIndex = this.inactiveList.Count - 1;
                int reusedIndex = this.inactiveList[lastIndex];
                this.inactiveList.RemoveAt(lastIndex);
                newSprite.managerIndex = reusedIndex;
                InternalAdd(ref newSprite, ref matrix);
            }

            private void InternalAdd(ref Sprite sprite, ref float4x4 matrix) {
                int index1 = sprite.managerIndex * 4;
                int index2 = index1 + 1;
                int index3 = index2 + 1;
                int index4 = index3 + 1;

                // Set the indeces
                sprite.index1 = index1;
                sprite.index2 = index2;
                sprite.index3 = index3;
                sprite.index4 = index4;

                // Transform vertices
                sprite.Transform(ref matrix);
                this.nativeVertices[sprite.index1] = sprite.transformedV1;
                this.nativeVertices[sprite.index2] = sprite.transformedV2;
                this.nativeVertices[sprite.index3] = sprite.transformedV3;
                this.nativeVertices[sprite.index4] = sprite.transformedV4;

                // Set the colors
                this.nativeColors[index1] = sprite.color;
                this.nativeColors[index2] = sprite.color;
                this.nativeColors[index3] = sprite.color;
                this.nativeColors[index4] = sprite.color;

                // Set the UVs
                this.nativeUv[index1] = sprite.uv_1;
                this.nativeUv[index2] = sprite.uv_2;
                this.nativeUv[index3] = sprite.uv_3;
                this.nativeUv[index4] = sprite.uv_4;

                // Set UV2
                this.nativeUv2[index1] = sprite.uv2_1;
                this.nativeUv2[index2] = sprite.uv2_2;
                this.nativeUv2[index3] = sprite.uv2_3;
                this.nativeUv2[index4] = sprite.uv2_4;

                // Set the triangle indeces
                int triangle1 = sprite.managerIndex * 6;
                int triangle2 = triangle1 + 1;
                int triangle3 = triangle2 + 1;
                int triangle4 = triangle3 + 1;
                int triangle5 = triangle4 + 1;
                int triangle6 = triangle5 + 1;

                // Lower left triangle
                this.nativeTriangles[triangle1] = index1;
                this.nativeTriangles[triangle2] = index3;
                this.nativeTriangles[triangle3] = index2;

                // Upper right triangle
                this.nativeTriangles[triangle4] = index3;
                this.nativeTriangles[triangle5] = index4;
                this.nativeTriangles[triangle6] = index2;

                sprite.active.Value = true;

                sprite.verticesChanged.Value = true;
                sprite.renderOrderChanged.Value = true;
                sprite.uvChanged.Value = true;
                sprite.colorChanged.Value = true;

                ++this.spriteCount;

                // Set these to true so that the new array values will be copied
                this.vertexCountChanged = true;
                this.verticesChanged = true;
                this.renderOrderChanged = true;
                this.uvChanged = true;
                this.colorsChanged = true;
            }

            /// <summary>
            /// Removes the specified sprite
            /// </summary>
            /// <param name="sprite"></param>
            public void Remove(Sprite sprite) {
                Assertion.Assert(this.owner == sprite.spriteManagerEntity);

                // We don't really remove. We just keep it in a temporary list of inactive sprite
                // When a sprite is added, we check if there are sprites in inactive list and we use that instead
                sprite.active.Value = false;
                Remove(sprite.managerIndex);
            }

            /// <summary>
            /// Removes the sprite with the specified managerIndex
            /// </summary>
            /// <param name="managerIndex"></param>
            public void Remove(int managerIndex) {
                // The inactive list should not have this index yet
                Assertion.Assert(!this.inactiveList.Contains(managerIndex), managerIndex.ToString());

                int index1 = managerIndex * 4;
                int index2 = index1 + 1;
                int index3 = index2 + 1;
                int index4 = index3 + 1;

                // Set vertices to zero so no quad is transformed
                this.nativeVertices[index1] = VectorUtils.ZERO;
                this.nativeVertices[index2] = VectorUtils.ZERO;
                this.nativeVertices[index3] = VectorUtils.ZERO;
                this.nativeVertices[index4] = VectorUtils.ZERO;

                this.verticesChanged = true;
                this.vertexCountChanged = true;

                this.inactiveList.Add(managerIndex);

                --this.spriteCount;
            }

            private void Expand() {
                this.capacity += this.allocationCount;

                // We multiply by 4 because each sprite has 4 vertices
                int vertexCount = this.capacity * 4;

                // Copy existing to the new one
                NativeArray<Vector3> newVertices = CopyAndExpand(this.nativeVertices, vertexCount);
                this.nativeVertices.Dispose();
                this.nativeVertices = newVertices;
                this.vertices = new Vector3[vertexCount];

                // We wrap in a block so we don't do a mistake of assigning the wrong NativeArray
                // for a certain UV
                {
                    NativeArray<Vector2> newUv = CopyAndExpand(this.nativeUv, vertexCount);
                    this.nativeUv.Dispose();
                    this.nativeUv = newUv;
                    this.uv = new Vector2[vertexCount];
                }

                {
                    NativeArray<Vector2> newUv2 = CopyAndExpand(this.nativeUv2, vertexCount);
                    this.nativeUv2.Dispose();
                    this.nativeUv2 = newUv2;
                    this.uv2 = new Vector2[vertexCount];
                }

                NativeArray<Color> newColors = CopyAndExpand(this.nativeColors, vertexCount);
                this.nativeColors.Dispose();
                this.nativeColors = newColors;
                this.colors = new Color[vertexCount];

                // Multiply by 6 because there are 6 indeces per quad (2 triangles)
                NativeArray<int> newTriangles = CopyAndExpand(this.nativeTriangles, this.capacity * 6);
                this.nativeTriangles.Dispose();
                this.nativeTriangles = newTriangles;
                this.triangles = new int[this.nativeTriangles.Length];
            }

            private static NativeArray<T> CopyAndExpand<T>(NativeArray<T> original, int newLength) where T : struct {
                Assertion.Assert(newLength > original.Length);

                NativeArray<T> newArray = new NativeArray<T>(newLength, Allocator.Persistent);
                NativeSlice<T> newArraySlice = new NativeSlice<T>(newArray, 0, original.Length);
                NativeSlice<T> originalSlice = new NativeSlice<T>(original);
                newArraySlice.CopyFrom(originalSlice);

                return newArray;
            }
        }

        public bool Equals(SpriteManager other) {
            return this.id == other.id;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            return obj is SpriteManager other && Equals(other);
        }

        public override int GetHashCode() {
            return this.id;
        }

        public static bool operator ==(SpriteManager left, SpriteManager right) {
            return left.Equals(right);
        }

        public static bool operator !=(SpriteManager left, SpriteManager right) {
            return !left.Equals(right);
        }
    }
}