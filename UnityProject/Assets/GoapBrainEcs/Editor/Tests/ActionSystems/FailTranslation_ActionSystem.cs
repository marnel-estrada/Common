namespace GoapBrainEcs {
    public class FailTranslation_ActionSystem : AtomActionComponentSystem<FailTranslation> {
        protected override GoapStatus Start(ref AtomAction atomAction, ref FailTranslation actionComponent) {
            return GoapStatus.FAILED;
        }
    }
}