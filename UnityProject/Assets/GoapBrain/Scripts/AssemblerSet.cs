using Common;

using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace GoapBrain {
    /// <summary>
    /// Contains the list of AtomActionAssemblers and ConditionResolverAssemblers.
    /// </summary>
    public class AssemblerSet {
        private readonly SimpleList<AtomActionAssembler> actionAssemblers = new SimpleList<AtomActionAssembler>();

        private readonly SimpleList<ConditionResolverPair> conditionResolverPairs =
            new SimpleList<ConditionResolverPair>();

        private readonly int id;

        public AssemblerSet(int id) {
            this.id = id;
        }

        public int Id {
            get {
                return this.id;
            }
        }

        public void Add(AtomActionAssembler actionAssembler) {
            this.actionAssemblers.Add(actionAssembler);
        }

        public void Add(string conditionName, ConditionResolverAssembler assembler) {
            this.conditionResolverPairs.Add(new ConditionResolverPair(conditionName, assembler));
        }

        /// <summary>
        /// Runs each assembler to the specified agent entity
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="agentEntity"></param>
        /// <param name="linkedEntities"></param>
        public void Prepare(ref EntityManager entityManager, in Entity agentEntity, in Entity plannerEntity,
            ref NativeList<Entity> linkedEntities) {
            // Run for condition resolvers
            for (int i = 0; i < this.conditionResolverPairs.Count; ++i) {
                ConditionResolverPair resolverPair = this.conditionResolverPairs[i];
                Entity resolverEntity = resolverPair.assembler.Prepare(ref entityManager, resolverPair.conditionName, agentEntity, plannerEntity);
                linkedEntities.Add(resolverEntity);
            }
            
            // Run for atom actions
            for (int i = 0; i < this.actionAssemblers.Count; ++i) {
                this.actionAssemblers[i].Prepare(ref entityManager, agentEntity, ref linkedEntities);
            }
        }
    }
}