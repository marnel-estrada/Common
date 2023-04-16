using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    public partial class RankTuningSystem : ConsiderationBaseSystem<RankTuning, RankTuningSystem.Processor> {
        protected override Processor PrepareProcessor() {
            return new Processor();
        }
        
        public struct Processor : IConsiderationProcess<RankTuning> {
            public void BeforeChunkIteration(ArchetypeChunk batchInChunk) {
            }

            public UtilityValue ComputeUtility(in Entity agentEntity, in RankTuning filterComponent, 
                int chunkIndex, int queryIndex) {
                return new UtilityValue(filterComponent.rankToSet, 0);
            }
        }
    }
}