using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    public struct UtilityOption : IComponentData {
        // Note here that the interpretation of the utility value here is different.
        // It's maxRank, totalBonus, and final multiplier
        public UtilityValue value;

        // The pointer entity to its parent UtilityBrain entity.
        public readonly Entity utilityBrainEntity;

        // This is just an integer but we wrap in a struct to avoid confusion
        public readonly OptionId id;

        // This is the index to it's parent UtilityBrain's DynamicBuffer of UtilityValue which
        // contains the value for each option.
        public readonly int brainIndex;

        // Not all options should execute. They only execute when this is true.
        // This will be replaced by enable/disable component when it it available.
        public bool shouldExecute;

        public UtilityOption(OptionId id, in Entity utilityBrainEntity, int brainIndex) : this() {
            this.id = id;
            this.utilityBrainEntity = utilityBrainEntity;
            this.brainIndex = brainIndex;
        }

        public UtilityOption(int id, in Entity utilityBrainEntity, int brainIndex) : 
            this(new OptionId(id), utilityBrainEntity, brainIndex) {
        }

        public UtilityOption(in FixedString64 textId, in Entity utilityBrainEntity, int brainIndex) : this(
            textId.GetHashCode(), utilityBrainEntity, brainIndex) {
        }
    }
}