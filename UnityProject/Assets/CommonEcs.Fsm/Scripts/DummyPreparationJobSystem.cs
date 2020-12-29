namespace Common.Ecs.Fsm {
    public class DummyPreparationJobSystem : FsmStatePreparationJobSystem<DummyComponent, DummyPreparationAction> {
        protected override DummyPreparationAction StatePreparationAction {
            get; 
        }
    }
}