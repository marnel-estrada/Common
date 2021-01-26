using System.Collections.Generic;

using UnityEngine;

namespace Common {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class CombinedMesh : MonoBehaviour {

        private Mesh mesh;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        private readonly Dictionary<Transform, Mesh> meshMap = new Dictionary<Transform, Mesh>();

        private readonly Dictionary<Transform, MeshPartHandle> handleMap = new Dictionary<Transform, MeshPartHandle>();

        private Transform selfTransform;

        /// <summary>
        /// Clears the combiner
        /// </summary>
        public void Clear() {
            this.meshMap.Clear();
            this.handleMap.Clear();
        }

        /// <summary>
        /// Adds a mesh to be combined
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="mesh"></param>
        public MeshPartHandle Add(Transform owner, Mesh mesh) {
            Assertion.IsTrue(!this.meshMap.ContainsKey(owner)); // Should not contain the specified owner yet

            this.meshMap[owner] = mesh;

            MeshPartHandle handle = new MeshPartHandle(mesh.vertices.Length);
            this.handleMap[owner] = handle;

            return handle;
        }

        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Color> colors = new List<Color>();
        private readonly List<Vector3> normals = new List<Vector3>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        private readonly List<Vector2> uvs2 = new List<Vector2>();
        private readonly List<int> triangles = new List<int>();

        // Cache array so we could easily set new UV values
        private Vector2[] uvArray;
        private Vector2[] uv2Array;
        private Color[] colorsArray;

        /// <summary>
        /// Builds the combined mesh
        /// </summary>
        public void Build() {
            this.vertices.Clear();
            this.colors.Clear();
            this.normals.Clear();
            this.uvs.Clear();
            this.uvs2.Clear();
            this.triangles.Clear();

            foreach (KeyValuePair<Transform, Mesh> entry in this.meshMap) {
                AddToBuild(entry.Key, entry.Value);
            }
            
            this.mesh = new Mesh();
            this.mesh.vertices = this.vertices.ToArray();
            this.mesh.triangles = this.triangles.ToArray();
            this.mesh.normals = this.normals.ToArray();

            this.colorsArray = this.colors.ToArray();
            this.mesh.colors = this.colorsArray;

            this.uvArray = this.uvs.ToArray(); 
            this.mesh.uv = this.uvArray;

            this.uv2Array = this.uvs2.ToArray();
            this.mesh.uv2 = this.uv2Array;

            this.meshFilter = GetComponent<MeshFilter>();
            Assertion.NotNull(this.meshFilter);
            this.meshFilter.mesh = this.mesh;

            ResolveMeshRenderer();
        }

        private void ResolveMeshRenderer() {
            this.meshRenderer = GetComponent<MeshRenderer>();
            Assertion.NotNull(this.meshRenderer);
        }

        private void AddToBuild(Transform owner, Mesh mesh) {
            MeshPartHandle handle = this.handleMap[owner];
            handle.StartIndex = this.vertices.Count;

            this.colors.AddRange(mesh.colors);
            this.normals.AddRange(mesh.normals);
            this.uvs.AddRange(mesh.uv);

            // Special case for UV2
            // Other meshes don't have it so we use zeroes
            if(mesh.uv2.Length == 0) {
                for(int i = 0; i < mesh.vertices.Length; ++i) {
                    this.uvs2.Add(VectorUtils.ZERO_2D);
                }
            } else {
                Assertion.IsTrue(mesh.uv.Length == mesh.uv2.Length);
                this.uvs2.AddRange(mesh.uv2);
            }

            // Adjust the triangle indeces
            for(int i = 0; i < mesh.triangles.Length; ++i) {
                this.triangles.Add(mesh.triangles[i] + handle.StartIndex);
            }

            if(this.selfTransform == null) {
                this.selfTransform = this.transform; // Cache
            }

            // Transform the vertices from its owner
            for(int i = 0; i < mesh.vertices.Length; ++i) {
                Vector3 transformedVertex = this.selfTransform.InverseTransformPoint(owner.TransformPoint(mesh.vertices[i]));
                this.vertices.Add(transformedVertex);
            }
        }

        /// <summary>
        /// Sets the material
        /// </summary>
        /// <param name="material"></param>
        public void SetMaterial(Material material) {
            ResolveMeshRenderer();
            this.meshRenderer.material = material;
        }

        /// <summary>
        /// Sets the sorting layer
        /// </summary>
        /// <param name="sortingLayerName"></param>
        public void SetSortingLayer(string sortingLayerName) {
            ResolveMeshRenderer();
            this.meshRenderer.sortingLayerName = sortingLayerName;
        }

        /// <summary>
        /// Sets the UVs of the mesh
        /// </summary>
        /// <param name="uvs"></param>
        public void SetUvs(MeshPartHandle handle, Vector2[] uvs) {
            for(int i = 0; i < handle.VertexCount; ++i) {
                this.uvArray[handle.StartIndex + i] = uvs[i];
            }
            this.meshFilter.mesh.uv = this.uvArray;
        }

        /// <summary>
        /// Sets the uv2
        /// </summary>
        /// <param name="uvs"></param>
        public void SetUvs2(MeshPartHandle handle, Vector2[] uvs) {
            for (int i = 0; i < handle.VertexCount; ++i) {
                this.uv2Array[handle.StartIndex + i] = uvs[i];
            }
            this.meshFilter.mesh.uv2 = this.uv2Array;
        }

        /// <summary>
        /// Sets the alpha to the vertex color of all vertices in the combined mesh
        /// </summary>
        /// <param name="alpha"></param>
        public void SetAlpha(float alpha) {
            for(int i = 0; i < this.colorsArray.Length; ++i) {
                this.colorsArray[i].a = alpha;
            }
            this.meshFilter.mesh.colors = this.colorsArray;
        }

        public Transform SelfTransform {
            get {
                if(this.selfTransform == null) {
                    this.selfTransform = this.transform;
                }

                return selfTransform;
            }
        }

    }
}
