using Common;

namespace GoapBrain {
    [Group("GoapBrain.General")]
    class ConstantResolver : ConditionResolver {

        public NamedBool boolValue { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value"></param>
        public ConstantResolver(bool value) {
            this.boolValue = new NamedBool();
            this.boolValue.Name = "boolValue";
            this.boolValue.Value = value;
        }

        protected override bool Resolve(GoapAgent agent) {
            return this.boolValue.Value;
        }

    }
}
