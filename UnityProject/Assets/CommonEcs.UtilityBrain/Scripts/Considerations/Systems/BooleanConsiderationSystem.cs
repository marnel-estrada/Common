using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    public abstract class BooleanConsiderationSystem<TConsiderationComponent, TCondition> : ConsiderationBaseSystem<TConsiderationComponent, BooleanConsiderationSystem<TConsiderationComponent, TCondition>.Processor>
        where TConsiderationComponent : struct, IConsiderationComponent
        where TCondition : struct, IConsiderationCondition<TConsiderationComponent> {
        // Must be implemented by deriving class
        protected abstract TCondition PrepareCondition();
        
        protected override Processor PrepareProcessor() {
            return new Processor() {
                condition = PrepareCondition(),
                booleanConsiderationType = GetComponentTypeHandle<BooleanConsiderationValues>()
            };
        }
        
        public struct Processor : IConsiderationProcess<TConsiderationComponent> {
            public TCondition condition;
            
            [ReadOnly]
            public ComponentTypeHandle<BooleanConsiderationValues> booleanConsiderationType;
            
            // We use NativeDisableContainerSafetyRestriction here so that Unity will not complain that
            // this is not set upon creation. The value of this variable would only be populated in 
            // BeforeChunkIteration()
            [NativeDisableContainerSafetyRestriction]
            private NativeArray<BooleanConsiderationValues> booleanConsiderationValuesList;
            
            public void BeforeChunkIteration(ArchetypeChunk batchInChunk, int batchIndex) {
                this.booleanConsiderationValuesList = batchInChunk.GetNativeArray(this.booleanConsiderationType);
            }

            public UtilityValue ComputeUtility(in Entity agentEntity, in TConsiderationComponent filterComponent,
                int indexOfFirstEntityInQuery, int iterIndex) {
                BooleanConsiderationValues considerationValues = this.booleanConsiderationValuesList[iterIndex];
                bool isConditionMet = this.condition.IsMet(agentEntity, filterComponent, indexOfFirstEntityInQuery, iterIndex);
                return isConditionMet ? considerationValues.trueValue : considerationValues.falseValue;
            }
        }
    }
}