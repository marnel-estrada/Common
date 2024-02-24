using Unity.Entities;

namespace Common {
    public struct SweepPruneItem {
        public Aabb2 box;
        public readonly Entity entity;
        public readonly int masterListIndex;

        public SweepPruneItem(Entity entity, Aabb2 box, int masterListIndex) {
            this.entity = entity;
            this.box = box;
            this.masterListIndex = masterListIndex;
        }

        public bool IsNone => this.entity == Entity.Null;
        
        public SweepPruneItem AsNone => new(Entity.Null, this.box, this.masterListIndex);
    }
}