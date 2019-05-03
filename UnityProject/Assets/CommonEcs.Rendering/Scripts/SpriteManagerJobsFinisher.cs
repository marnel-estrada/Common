using Unity.Entities;

namespace CommonEcs {    
    /// <summary>
    /// A system that collects all SpriteManager related jobs and completes them
    /// </summary>
    [UpdateAfter(typeof(CollectedCommandsSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SpriteManagerJobsFinisher : ComponentSystem {
        protected override void OnUpdate() {
            this.EntityManager.CompleteAllJobs();
        }
    }
}
