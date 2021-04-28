using Common;

using Unity.Entities;

namespace GoapBrain {
    public class ForcedValueResolverAssembler : SingleComponentResolverAssembler<ForcedValueResolver> {
        public NamedBool result { get; set; }

        protected override void PrepareResolver(ref EntityManager entityManager, in Entity agentEntity, in Entity resolverEntity) {
            entityManager.SetComponentData(resolverEntity, new ForcedValueResolver(this.result.Value));
        }
    }
}