using Unity.Entities;

namespace CommonEcs.DotsFsm {
    public struct DotsFsmAction : IComponentData {
        public readonly Entity stateOwner;
        
        public bool entered;
        public bool exited;

        public DotsFsmAction(Entity stateOwner) {
            this.stateOwner = stateOwner;
            this.entered = false;
            this.exited = false;
        }
    }
}