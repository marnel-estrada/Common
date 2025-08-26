using CommonEcs;
using CommonEcs.Goap;
using Unity.Entities;

[assembly:RegisterGenericComponentType(typeof(DynamicBufferHashMap<ConditionHashId, bool>))]
[assembly:RegisterGenericComponentType(typeof(DynamicBufferHashMap<ConditionHashId, bool>.Entry))]