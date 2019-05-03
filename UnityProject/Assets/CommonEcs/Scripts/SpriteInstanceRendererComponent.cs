using Unity.Entities;
using Unity.Rendering;

using UnityEngine;

namespace Common {
    /// <summary>
    /// Creates a mesh dynamically based on the specified settings
    /// </summary>
    public class SpriteInstanceRendererComponent : SharedComponentDataProxy<RenderMesh> {
        [SerializeField]
        private Sprite sprite;
    
        [SerializeField]
        private Material material; 
    
        // The width of the sprite in world units
        private float width;
    
        // The height of the sprite in world units
        private float height;
    
        private void Awake() {
            // Prepare the mesh
            Mesh mesh = new Mesh();
            
            // Copy vertices
            Vector3[] vertices = new Vector3[this.sprite.vertices.Length];
            for(int i = 0; i < vertices.Length; ++i) {
                vertices[i] = this.sprite.vertices[i];
            }
            mesh.vertices = vertices;
            
            // Compute width and height
            this.width = Mathf.Abs(this.sprite.vertices[1].x - this.sprite.vertices[0].x);
            this.height = Mathf.Abs(this.sprite.vertices[1].y - this.sprite.vertices[0].y);
    
            // Copy triangles
            int[] triangles = new int[this.sprite.triangles.Length];
            for(int i = 0; i < triangles.Length; ++i) {
                triangles[i] = this.sprite.triangles[i];
            }
            mesh.triangles = triangles;
    
            // Copy UV
            mesh.uv = this.sprite.uv;
    
            RenderMesh instance = new RenderMesh();
            instance.mesh = mesh;
            instance.material = this.material;
            this.Value = instance;
        }
    
        public float Height {
            get {
                return this.height;
            }
        }
    
        public float Width {
            get {
                return this.width;
            }
        }
    }
}
