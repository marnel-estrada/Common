using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// A utility for ease of handling with conditions map which is managed by a DynamicBufferHashMap
    /// </summary>
    public struct ConditionsMap {
        private readonly Entity plannerEntity;
        private DynamicBufferHashMap<ConditionHashId, bool> map;
        private DynamicBuffer<DynamicBufferHashMap<ConditionHashId, bool>.Entry> bucket;

        /// <summary>
        /// Initializes the conditions map for the specified planner entity.
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="plannerEntity"></param>
        public static void Init(ref EntityManager entityManager, in Entity plannerEntity) {
            DynamicBuffer<DynamicBufferHashMap<ConditionHashId, bool>.Entry> bucket = 
                entityManager.GetBuffer<DynamicBufferHashMap<ConditionHashId, bool>.Entry>(plannerEntity);
            DynamicBufferHashMap<ConditionHashId, bool>.Init(ref bucket);
        }

        public ConditionsMap(in Entity plannerEntity, DynamicBufferHashMap<ConditionHashId, bool> map, 
            DynamicBuffer<DynamicBufferHashMap<ConditionHashId, bool>.Entry> bucket) {
            this.plannerEntity = plannerEntity;
            this.map = map;
            this.bucket = bucket;
        }

        public ConditionsMap(ref EntityManager entityManager, in Entity plannerEntity) : this(plannerEntity, 
            entityManager.GetComponentData<DynamicBufferHashMap<ConditionHashId, bool>>(plannerEntity),
            entityManager.GetBuffer<DynamicBufferHashMap<ConditionHashId, bool>.Entry>(plannerEntity)) {
        }

        public int AddOrSet(in ConditionHashId id, bool value) {
            return this.map.AddOrSet(ref this.bucket, id, value);
        }
        
        public int AddOrSet(in FixedString64Bytes id, bool value) {
            return AddOrSet(new ConditionHashId(id), value);
        }

        /// <summary>
        /// Looks for the value of the specified condition ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ValueTypeOption<bool> Find(in ConditionHashId id) {
            return this.map.Find(this.bucket, id);
        }
        
        public ValueTypeOption<bool> Find(in FixedString64Bytes id) {
            return Find(new ConditionHashId(id));
        }

        /// <summary>
        /// Modifies the DynamicBufferHashMap component.
        /// </summary>
        /// <param name="entityManager"></param>
        public void Commit(ref EntityManager entityManager) {
            entityManager.SetComponentData(this.plannerEntity, this.map);
        }

        public void Clear() {
            this.map.Clear(ref this.bucket);
        }
        
        /// <summary>
        /// We provide a getter because a job may need it to modify the value
        /// </summary>
        public DynamicBufferHashMap<ConditionHashId, bool> Map {
            get {
                return this.map;
            }
        }

        public static void ResetValues(ref DynamicBuffer<DynamicBufferHashMap<ConditionHashId, bool>.Entry> bucket) {
            DynamicBufferHashMap<ConditionHashId, bool>.ResetValues(ref bucket);
        }
    }
}