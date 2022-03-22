using CommonEcs.DotsFsm;

using Unity.Jobs;

[assembly:RegisterGenericJobType(typeof(DotsFsmActionSystem<MoveTo, MoveToSystem.Execution>.ExecuteActionJob))]