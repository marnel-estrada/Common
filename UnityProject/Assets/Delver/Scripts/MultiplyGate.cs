using Common;

namespace Delver {
    /// <summary>
    /// Multiplies two units
    /// </summary>
    class MultiplyGate : GateAdapter {
        /// <summary>
        /// Constructor
        /// </summary>
        public MultiplyGate() : base(2) {
        }

        public override void Backward() {
            Unit a = GetInputAt(0);
            Unit b = GetInputAt(1);
            Assertion.IsTrue(a != b);

            a.Gradient += b.Value * this.ForwardUnit.Gradient;
            b.Gradient += a.Value * this.ForwardUnit.Gradient;
        }

        public override void Forward() {
            Assertion.IsTrue(this.InputCount == 2);
            this.ForwardUnit.Value = GetInputAt(0).Value * GetInputAt(1).Value;
        }

        public override string ToString() {
            Unit a = GetInputAt(0);
            Unit b = GetInputAt(1);

            return "(" + a.Value + " * " + b.Value + ")";
        }
    }
}
