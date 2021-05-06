using Common;

using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    [Group("CommonEcs.UtilityBrain")]
    public class BonusTuningAssembler : SingleComponentConsiderationAssembler<BonusTuning> {
        // The bonus to set
        public NamedFloat bonus { get; set; }

        protected override void PrepareConsiderationComponent(ref EntityManager entityManager, in Entity agentEntity,
            in Entity considerationEntity) {
            entityManager.SetComponentData(considerationEntity, new BonusTuning(this.bonus.Value));
        }
    }
}