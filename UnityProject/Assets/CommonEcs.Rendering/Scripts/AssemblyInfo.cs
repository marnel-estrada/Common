using CommonEcs;

using Unity.Entities;

[assembly: RegisterGenericComponentType(typeof(CollectSharedComponentsSystem<SpriteLayer>.Collected))]
[assembly: RegisterGenericComponentType(typeof(CollectSharedComponentsSystem<SpriteManager>.Collected))]

[assembly: RegisterGenericComponentType(typeof(SignalHandlerComponentSystem<SetSpriteLayerVesselsEnabled>.ProcessedBySystem))]