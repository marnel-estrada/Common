using Common.Ecs.Fsm;

using Unity.Jobs;

[assembly: RegisterGenericJobType(typeof(FsmStatePreparationJobSystem<DummyComponent, DummyPreparationAction>.Job))]