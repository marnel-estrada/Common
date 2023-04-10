using Unity.Entities;

namespace CommonEcs {    
    /// <summary>
    /// A system that collects all SpriteManager related jobs and completes them
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class SpriteManagerJobsFinisher : SystemBase {
        protected override void OnUpdate() {
            this.EntityManager.CompleteAllTrackedJobs();
        }
    }
}
