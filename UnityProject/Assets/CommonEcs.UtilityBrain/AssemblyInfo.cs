using CommonEcs.UtilityBrain;

using Unity.Jobs;

[assembly:RegisterGenericJobType(typeof(ComputeConsiderationsJob<BonusTuning, BonusTuningSystem.Processor>))]
[assembly:RegisterGenericJobType(typeof(ComputeConsiderationsJob<RankTuning, RankTuningSystem.Processor>))]