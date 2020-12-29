using Common.Ecs.Fsm;

using Unity.Entities;

[assembly: RegisterGenericJobType(typeof(FsmStatePreparationJobSystem<DummyComponent, DummyPreparationAction>))]