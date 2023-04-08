namespace Common.Ecs.Fsm {
    public partial class DummyPreparationJobSystem : FsmStatePreparationJobSystem<DummyComponent, DummyPreparationAction> {
        protected override DummyPreparationAction StatePreparationAction {
            get; 
        }
    }
}