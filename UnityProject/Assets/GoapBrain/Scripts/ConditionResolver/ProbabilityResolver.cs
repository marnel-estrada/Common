using Common;

namespace GoapBrain {
    [Group("GoapBrain.General")]
    public class ProbabilityResolver : ConditionResolver {
        
        public NamedFloat probability { get; set; }

        protected override bool Resolve(GoapAgent agent) {
            return Comparison.TolerantLesserThanOrEquals(UnityEngine.Random.value, this.probability.Value);
        }

    }
}
