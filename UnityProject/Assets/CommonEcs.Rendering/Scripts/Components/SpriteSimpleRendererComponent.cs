using Unity.Entities;
using UnityEngine;

using Common;
    
namespace CommonEcs {
    /// <summary>
    /// Creates a mesh dynamically based on the specified settings
    /// </summary>
    public class SpriteSimpleRendererComponent : SharedComponentDataProxy<SimpleRenderer> {
        [SerializeField]
        private UnityEngine.Sprite sprite;

        [SerializeField]
        private Material material;

        [SerializeField] 
        private Color tint = ColorUtils.WHITE;

        // The width of the sprite in world units
        private float width;

        // The height of the sprite in world units
        private float height;
        
        private Mesh mesh;

        private void Awake() {
            if (this.mesh == null) {
                // Not prepared yet
                PrepareMesh();
            }
        }

        /// <summary>
        /// Prepares the mesh
        /// We did it this way because the mesh might be needed and it may not yet be available
        /// in Awake()
        /// </summary>
        public void PrepareMesh() {
            // Prepare the mesh
            this.mesh = new Mesh();

            // Copy vertices
            Vector3[] vertices = new Vector3[this.sprite.vertices.Length];
            for(int i = 0; i < vertices.Length; ++i) {
                vertices[i] = this.sprite.vertices[i];
            }
            this.mesh.vertices = vertices;
            
            // Compute width and height
            this.width = Mathf.Abs(this.sprite.vertices[1].x - this.sprite.vertices[0].x);
            this.height = Mathf.Abs(this.sprite.vertices[1].y - this.sprite.vertices[0].y);

            // Copy triangles
            int[] triangles = new int[this.sprite.triangles.Length];
            for(int i = 0; i < triangles.Length; ++i) {
                triangles[i] = this.sprite.triangles[i];
            }
            this.mesh.triangles = triangles;

            // Copy UV
            this.mesh.uv = this.sprite.uv;

            SimpleRenderer instance = new SimpleRenderer {
                mesh = this.mesh,
                material = this.material,
                tint = this.tint
            };
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
