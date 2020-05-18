using UnityEngine;

namespace Common {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class ProceduralTexturedQuad : MonoBehaviour {
        [SerializeField]
        private Vector2 topLeftUv = new Vector2();

        [SerializeField]
        private Vector2 topRightUv = new Vector2();

        [SerializeField]
        private Vector2 bottomRightUv = new Vector2();

        [SerializeField]
        private Vector2 bottomLeftUv = new Vector2();

        [SerializeField]
        private Texture texture;

        [SerializeField]
        private Color modulationColor = Color.white;

        [SerializeField]
        private float alphaCutoff;

        [SerializeField]
        private Material material;

        [SerializeField]
        private Mesh mesh = null;

        void Start() {
            GenerateMesh();
        }

        public void GenerateMesh() {
            // compose quad procedurally
            this.mesh = new Mesh();
            Vector3[] vertices = {
                new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(0.5f, -0.5f, 0),
                new Vector3(-0.5f, -0.5f, 0)
            };
            mesh.vertices = vertices;

            Vector2[] textureCoordinates = {
                this.topLeftUv, this.topRightUv, this.bottomRightUv, this.bottomLeftUv
            };
            mesh.uv = textureCoordinates;

            int[] triangleIndeces = {
                1, 0, 3, 1, 3, 2
            };
            mesh.triangles = triangleIndeces;

            // assign to filter
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            if (this.material == null) {
                // no material set, we create a default one
                this.material = new Material(Shader.Find("NoCullingTransparent"));
                this.material.SetFloat("_Cutoff", alphaCutoff);
            }

            // set material
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            renderer.material = this.material;

            if (this.texture != null) {
                // set automatically if assigned in editor
                SetTexture(this.texture);
            }

            SetModulationColor(this.modulationColor);
        }

        /**
        * Returns whether or not the world textured quad has a texture assigned to it.
        */
        public bool HasTexture() {
            return this.texture != null;
        }

        /**
        * Sets the texture to the world textured quad.
        */
        public virtual void SetTexture(Texture texture) {
            Assertion.NotNull(texture);
            this.texture = texture;
            this.material.mainTexture = texture;
        }

        /**
        * Sets the modulation color of the texture.
        */
        public void SetModulationColor(Color color) {
            this.material.color = color;
        }
    }
}

