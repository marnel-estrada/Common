using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    public class BonusTuningSystem : ConsiderationBaseSystem<BonusTuning, BonusTuningSystem.Processor> {
        public struct Processor : IConsiderationProcess<BonusTuning> {
            public void BeforeChunkIteration(ArchetypeChunk batchInChunk) {
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