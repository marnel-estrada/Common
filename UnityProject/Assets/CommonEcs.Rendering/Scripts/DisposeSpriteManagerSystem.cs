namespace CommonEcs {
    using System.Collections.Generic;
    
    using Unity.Entities;

    /// <summary>
    /// A system that disposes the NativeArray<Vector3> instance of each SpriteManager
    /// </summary>
    [UpdateAfter(typeof(CollectedCommandsSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class DisposeSpriteManagerSystem : ComponentSystem {

        private readonly List<SpriteManager> managers = new List<SpriteManager>(1);
        private readonly List<int> managerIndeces = new List<int>(1);
        
        protected override void OnUpdate() {
        }

        protected override void OnDestroyManager() {
            this.managers.Clear();
            this.managerIndeces.Clear();
            this.EntityManager.GetAllUniqueSharedComponentData(this.managers, this.managerIndeces);
            
            for (int i = 1; i < this.managers.Count; ++i) {
                this.managers[i].Dispose();
            }
        }
        
    }
}
