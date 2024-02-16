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

        // Needs the masterListIndex so it retains its position
        public static SweepPruneItem NoneItem(int masterListIndex) {
            return new SweepPruneItem(Entity.Null, Aabb2.EmptyBounds(), masterListIndex);
        }
    }
}