using System.Collections.Generic;

using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    [UpdateAfter(typeof(IdentifyDrawInstanceChangedSystem))]
    [UpdateBefore(typeof(RenderComputeBufferSpritesSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ResetComputeBufferSpritesChangedFlagsSystem : SystemBase {
        private SharedComponentQuery<ComputeBufferDrawInstance> drawInstanceQuery;
        
        protected override void OnCreate() {
            this.drawInstanceQuery = new SharedComponentQuery<ComputeBufferDrawInstance>(this, this.EntityManager);
        }
        
        protected override void OnUpdate() {
            this.Dependency = OnUpdate(this.Dependency);
        }

        private JobHandle OnUpdate(JobHandle inputDeps) {
            JobHandle handle = inputDeps;
            
            this.drawInstanceQuery.Update();
            IReadOnlyList<ComputeBufferDrawInstance> drawInstances = this.drawInstanceQuery.SharedComponents;

            for (int i = 1; i < drawInstances.Count; ++i) {
                ComputeBufferDrawInstance drawInstance = drawInstances[i];
                if (!drawInstance.SomethingChanged) {
                    // Nothing changed
                    // No need to reset sprites under this drawInstance
                    continue;
                }
                
                // Schedule reset job
                handle = this.Entities.WithSharedComponentFilter(drawInstance).ForEach(delegate(ref ComputeBufferSprite sprite) {
                    sprite.ResetChangedFlags();
                }).ScheduleParallel(handle);
            }
            
            return handle;
        }
    }
}