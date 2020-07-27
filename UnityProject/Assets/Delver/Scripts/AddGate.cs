using Common;

namespace Delver {
    class AddGate : GateAdapter {
        /// <summary>
        /// Constructor
        /// </summary>
        public AddGate() : base(2) {
        }

        public override void Backward() {
            GetInputAt(0).Gradient += 1 * this.ForwardUnit.Gradient;
            GetInputAt(1).Gradient += 1 * this.ForwardUnit.Gradient;
        }

        public override void Forward() {
            Assertion.IsTrue(this.InputCount == 2);
            this.ForwardUnit.Value = GetInputAt(0).Value + GetInputAt(1).Value;
        }

        public override string ToString() {
            return GetInput(0).Value + " + " + GetInput(1).Value;
        }
    }
}
