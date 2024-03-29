using CommonEcs.Goap;

using Unity.Entities;

namespace GoapBrain {
    public partial class ForcedValueResolverSystem : ConditionResolverBaseSystem<ForcedValueResolver, ForcedValueResolverSystem.Processor> {
        public struct Processor : IConditionResolverProcess<ForcedValueResolver> {
            public void BeforeChunkIteration(ArchetypeChunk chunk) {
            }
            
            public bool IsMet(in Entity agentEntity, ref ForcedValueResolver resolverComponent, int chunkIndex, int queryIndex) {
                return resolverComponent.result;
            }
        }

        protected override Processor PrepareProcessor() {
            return new Processor();
        }
    }
}