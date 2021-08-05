using Common;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// Base assembler for considerations that use BooleanConsiderationValues
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BooleanConsiderationAssembler<T> : ConsiderationAssembler 
        where T : struct, IConsiderationComponent {
        public NamedInt rankIfTrue { get; set; }
        public NamedFloat bonusIfTrue { get; set; }
        public NamedFloat multiplierIfTrue { get; set; }
        
        public NamedInt rankIfFalse { get; set; }
        public NamedFloat bonusIfFalse { get; set; }
        public NamedFloat multiplierIfFalse { get; set; }

        private EntityArchetype archetype;

        public override void Init(ref EntityManager entityManager) {
            base.Init(ref entityManager);
            this.archetype = entityManager.CreateArchetype(typeof(Consideration), 
                typeof(BooleanConsiderationValues), typeof(T));
        }

        public override Entity Prepare(ref EntityManager entityManager, in Entity agentEntity, in Entity optionEntity, int optionIndex,
            ref NativeList<Entity> linkedEntities) {
            Entity considerationEntity = entityManager.CreateEntity(this.archetype);
            entityManager.SetComponentData(considerationEntity, new Consideration(agentEntity, optionEntity, optionIndex));
            
            PrepareComponent(ref entityManager, agentEntity, considerationEntity);
            
            linkedEntities.Add(considerationEntity);

            return considerationEntity;
        }

        /// <summary>
        /// Prepare the filter component that is T.
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="agentEntity"></param>
        /// <param name="considerationEntity"></param>
        protected virtual void PrepareComponent(ref EntityManager entityManager, in Entity agentEntity,
            in Entity considerationEntity) {
            // Prepare BooleanConsiderationValues
            UtilityValue trueValue = new UtilityValue(this.rankIfTrue.Value, this.bonusIfTrue.Value, this.multiplierIfTrue.Value);
            UtilityValue falseValue = new UtilityValue(this.rankIfFalse.Value, this.bonusIfFalse.Value, this.multiplierIfFalse.Value);
            entityManager.SetComponentData(considerationEntity, 
                new BooleanConsiderationValues(trueValue, falseValue));
        }
    }
}