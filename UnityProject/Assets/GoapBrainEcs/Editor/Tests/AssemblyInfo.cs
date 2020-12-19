using CommonEcs;

using GoapBrainEcs;

using Unity.Entities;

[assembly: RegisterGenericComponentType(typeof(EcsHashMap<ushort, ByteBool>))]
[assembly: RegisterGenericComponentType(typeof(EcsHashMapEntry<ushort, ByteBool>))]

[assembly: RegisterGenericComponentType(typeof(EcsHashMap<byte, byte>))]
[assembly: RegisterGenericComponentType(typeof(EcsHashMapEntry<byte, byte>))]

[assembly: RegisterGenericComponentType(typeof(EcsHashMap<int, int>))]
[assembly: RegisterGenericComponentType(typeof(EcsHashMapEntry<int, int>))]

[assembly: RegisterGenericComponentType(typeof(BufferElement<Entity>))]
[assembly: RegisterGenericComponentType(typeof(BufferElement<Counter>))]