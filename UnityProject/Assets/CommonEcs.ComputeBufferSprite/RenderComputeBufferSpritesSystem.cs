using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RenderComputeBufferSpritesSystem : SystemBase {
        private SharedComponentQuery<ComputeBufferDrawInstance> managerQuery;
        private Mesh quad;

        protected override void OnCreate() {
            this.managerQuery = new SharedComponentQuery<ComputeBufferDrawInstance>(this, this.EntityManager);
            this.quad = CreateQuad();
        }

        protected override void OnUpdate() {
            this.EntityManager.CompleteAllJobs();
            
            this.managerQuery.Update();
            IReadOnlyList<ComputeBufferDrawInstance> drawInstances = this.managerQuery.SharedComponents;
            
            // Note here that we start iteration from 1 because the first value is the default value
            for (int i = 1; i < drawInstances.Count; ++i) {
                ComputeBufferDrawInstance drawInstance = drawInstances[i];
                if (drawInstance.RenderOrderChanged) {
                    // We set this to false so it won't be sorted and set to the buffer on the next frame
                    // if there were no changes to the render order
                    drawInstance.SetDataToBuffers();
                    drawInstance.RenderOrderChanged = false;
                }
                
                drawInstance.Draw(this.quad);
            }
        }
        
        private static Mesh CreateQuad() {
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(0, 0, 0);
            vertices[1] = new Vector3(1, 0, 0);
            vertices[2] = new Vector3(0, 1, 0);
            vertices[3] = new Vector3(1, 1, 0);
            mesh.vertices = vertices;

            int[] tri = new int[6];
            tri[0] = 0;
            tri[1] = 2;
            tri[2] = 1;
            tri[3] = 2;
            tri[4] = 3;
            tri[5] = 1;
            mesh.triangles = tri;

            Vector3[] normals = new Vector3[4];
            normals[0] = -Vector3.forward;
            normals[1] = -Vector3.forward;
            normals[2] = -Vector3.forward;
            normals[3] = -Vector3.forward;
            mesh.normals = normals;

            Vector2[] uv = new Vector2[4];
            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(1, 0);
            uv[2] = new Vector2(0, 1);
            uv[3] = new Vector2(1, 1);
            mesh.uv = uv;

            return mesh;
        }
    }
}