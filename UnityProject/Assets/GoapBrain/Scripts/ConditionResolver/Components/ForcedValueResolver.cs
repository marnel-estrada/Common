using CommonEcs.Goap;

namespace GoapBrain {
    /// <summary>
    /// This is a common condition resolver that uses a specified bool value as a result 
    /// </summary>
    public readonly struct ForcedValueResolver : IConditionResolverComponent {
        public readonly bool result;

        public ForcedValueResolver(bool result) {
            this.result = result;
        }
    }
}