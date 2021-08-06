using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    public class RankTuningSystem : ConsiderationBaseSystem<RankTuning, RankTuningSystem.Processor> {
        protected override Processor PrepareProcessor() {
            return new Processor();
        }
        
        public struct Processor : IConsiderationProcess<RankTuning> {
            public void BeforeChunkIteration(ArchetypeChunk batchInChunk, int batchIndex) {
            }

            public UtilityValue ComputeUtility(in Entity agentEntity, in RankTuning filterComponent, int indexOfFirstEntityInQuery,
                int iterIndex) {
                return new UtilityValue(filterComponent.rankToSet, 0);
            }
        }
    }
}