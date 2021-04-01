using Unity.Entities;

namespace CommonEcs.Goap {
    [InternalBufferCapacity(20)]
    public readonly struct ResolvedAction : IBufferElementData {
        public readonly int actionId;
        
        // Needed for moving to next atom action such that we don't need to query from GoapDomain
        // It's already big as it is. We can't add a hashmap for mapping an action ID to its action.
        public readonly int atomActionCount;

        public ResolvedAction(int actionId, int atomActionCount) {
            this.actionId = actionId;
            this.atomActionCount = atomActionCount;
        }
    }
}