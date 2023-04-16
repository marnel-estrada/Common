using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    public partial class BonusTuningSystem : ConsiderationBaseSystem<BonusTuning, BonusTuningSystem.Processor> {
        public struct Processor : IConsiderationProcess<BonusTuning> {
            public void BeforeChunkIteration(ArchetypeChunk chunk) {
            }
            
            public UtilityValue ComputeUtility(in Entity agentEntity, in BonusTuning bonusTuning, int chunkIndex, int queryIndex) {
                return new UtilityValue(0, bonusTuning.bonusToSet);
            }
        }

        protected override Processor PrepareProcessor() {
            return new Processor();
        }
    }
}