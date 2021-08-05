using Common;

using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    [Group("CommonEcs.UtilityBrain")]
    public class RankTuningAssembler : SingleComponentConsiderationAssembler<RankTuning> {
        // The rank to set
        public NamedInt rank { get; set; }

        protected override void PrepareConsiderationComponent(ref EntityManager entityManager, in Entity agentEntity,
            in Entity considerationEntity) {
            entityManager.SetComponentData(considerationEntity, new RankTuning(this.rank.Value));
        }
    }
}