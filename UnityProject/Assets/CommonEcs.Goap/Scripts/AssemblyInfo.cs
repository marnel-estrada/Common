using CommonEcs;
using CommonEcs.Goap;
using Unity.Entities;

[assembly:RegisterGenericComponentType(typeof(DynamicBufferHashMap<ConditionId, bool>))]
[assembly:RegisterGenericComponentType(typeof(DynamicBufferHashMap<ConditionId, bool>.Entry<bool>))]