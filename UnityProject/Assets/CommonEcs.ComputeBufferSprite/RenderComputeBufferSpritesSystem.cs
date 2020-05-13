using System.Collections.Generic;

using Unity.Entities;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RenderComputeBufferSpritesSystem : SystemBase {
        private SharedComponentQuery<ComputeBufferDrawInstance> managerQuery;

        protected override void OnCreate() {
            this.managerQuery = new SharedComponentQuery<ComputeBufferDrawInstance>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
            this.EntityManager.CompleteAllJobs();
            
            this.managerQuery.Update();
            IReadOnlyList<ComputeBufferDrawInstance> drawInstances = this.managerQuery.SharedComponents;
            
            // Note here that we start iteration from 1 because the first value is the default value
            // Sorting the draw instances won't matter here. Sort the rendering using the Render Queue of the
            // material instead
            for (int i = 1; i < drawInstances.Count; ++i) {
                ComputeBufferDrawInstance drawInstance = drawInstances[i];
                drawInstance.UpdateBuffers();   
                drawInstance.Draw();
            }
        }
    }
}