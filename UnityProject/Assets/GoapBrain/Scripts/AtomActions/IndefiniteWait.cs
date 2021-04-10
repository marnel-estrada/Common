using Common;

namespace GoapBrain {
    [Group("GoapBrain.General")]
    class IndefiniteWait : GoapAtomAction {

        public override GoapResult Start(GoapAgent agent) {
            return GoapResult.RUNNING;
        }

        public override GoapResult Update(GoapAgent agent) {
            return GoapResult.RUNNING;
        }

    }
}
