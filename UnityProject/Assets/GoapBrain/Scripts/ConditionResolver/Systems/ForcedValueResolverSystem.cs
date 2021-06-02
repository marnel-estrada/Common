using CommonEcs.Goap;

using Unity.Entities;

namespace GoapBrain {
    public class ForcedValueResolverSystem : ConditionResolverBaseSystem<ForcedValueResolver, ForcedValueResolverSystem.Processor> {
        public struct Processor : IConditionResolverProcess<ForcedValueResolver> {
            public void BeforeChunkIteration(ArchetypeChunk batchInChunk, int batchIndex) {
            }
            
            public bool IsMet(in Entity agentEntity, ref ForcedValueResolver resolverComponent, int indexOfFirstEntityInQuery, int iterIndex) {
                return resolverComponent.result;
            }
        }

        protected override Processor PrepareProcessor() {
            return new Processor();
        }
    }
}