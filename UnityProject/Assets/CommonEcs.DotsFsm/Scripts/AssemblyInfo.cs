using CommonEcs.DotsFsm;

using Unity.Entities;

[assembly:RegisterGenericJobType(typeof(DotsFsmActionSystem<MoveTo, MoveToSystem.Execution>.ExecuteActionJob))]