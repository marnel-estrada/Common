using CommonEcs.UtilityBrain;

using Unity.Entities;

[assembly:RegisterGenericJobType(typeof(ConsiderationBaseSystem<BonusTuning, BonusTuningSystem.Processor>.Job))]
[assembly:RegisterGenericJobType(typeof(ConsiderationBaseSystem<RankTuning, RankTuningSystem.Processor>.Job))]