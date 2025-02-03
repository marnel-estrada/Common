using CommonEcs.UtilityBrain;

using Unity.Jobs;

[assembly:RegisterGenericJobType(typeof(ConsiderationBaseSystem<BonusTuning, BonusTuningSystem.Processor>.ComputeConsiderationsJob))]
[assembly:RegisterGenericJobType(typeof(ConsiderationBaseSystem<RankTuning, RankTuningSystem.Processor>.ComputeConsiderationsJob))]