using System;

using Common;

using Unity.Entities;

namespace CommonEcs {
    public readonly struct GetCellMatcher : IFuncOptionMatcher<Entity, Cell2D> {
        public readonly ComponentLookup<Cell2D> allCells;

        public GetCellMatcher(ComponentLookup<Cell2D> allCells) {
            this.allCells = allCells;
        }

        public Cell2D OnSome(Entity entity) {
            return this.allCells[entity];
        }

        public Cell2D OnNone() {
            throw new Exception("No entity");
        }
    }
}