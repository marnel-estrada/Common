using CommonEcs;
using CommonEcs.DotsFsm;

using Unity.Entities;

[assembly: RegisterGenericComponentType(typeof(SignalHandlerBatchJobSystem<SendEvent, DotsFsmSendEventHandlerSystem.Processor>.ProcessedBySystem))]
[assembly: RegisterGenericJobType(typeof(SignalHandlerBatchJobSystem<SendEvent, DotsFsmSendEventHandlerSystem.Processor>.ProcessJob))]