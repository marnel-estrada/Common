using CommonEcs.Goap;

using Unity.Entities;

namespace GoapBrain {
    public class ForcedValueResolverSystem : ConditionResolverBaseSystem<ForcedValueResolver, ForcedValueResolverSystem.Processor> {
        public struct Processor : IConditionResolverProcess<ForcedValueResolver> {
            public bool IsMet(in Entity agentEntity, ref ForcedValueResolver resolverComponent) {
                return resolverComponent.result;
            }
        }

        protected override Processor PrepareProcessor() {
            return new Processor();
        }
    }
}