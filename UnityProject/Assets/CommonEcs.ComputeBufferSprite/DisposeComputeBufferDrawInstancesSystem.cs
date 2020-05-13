using System.Collections.Generic;

using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A system that disposes the native collections used in ComputeBufferDrawInstance
    /// </summary>
    public class DisposeComputeBufferDrawInstancesSystem : SystemBase {
        private SharedComponentQuery<ComputeBufferDrawInstance> managerQuery;

        protected override void OnCreate() {
            this.managerQuery = new SharedComponentQuery<ComputeBufferDrawInstance>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
        }

        protected override void OnDestroy() {
            this.managerQuery.Update();
            IReadOnlyList<ComputeBufferDrawInstance> drawInstances = this.managerQuery.SharedComponents;
            
            // Note here that we start iteration from 1 because the first value is the default value
            for (int i = 1; i < drawInstances.Count; ++i) {
                drawInstances[i].Dispose();
            }
        }
    }
}