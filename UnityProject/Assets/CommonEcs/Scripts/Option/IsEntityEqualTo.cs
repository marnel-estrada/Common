using Common;

using Unity.Entities;

namespace CommonEcs {
    public readonly struct IsEntityEqualTo : IFuncOptionMatcher<Entity, bool> {
        private readonly Entity entityToCompare;

        public IsEntityEqualTo(Entity entityToCompare) {
            this.entityToCompare = entityToCompare;
        }

        public bool OnSome(Entity value) {
            return value == this.entityToCompare;
        }

        public bool OnNone() {
            return false;
        }
    }
}