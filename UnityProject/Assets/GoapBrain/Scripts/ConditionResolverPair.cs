namespace GoapBrain {
    public readonly struct ConditionResolverPair {
        public readonly string conditionName;
        public readonly ConditionResolverAssembler assembler;

        public ConditionResolverPair(string conditionName, ConditionResolverAssembler assembler) {
            this.conditionName = conditionName;
            this.assembler = assembler;
        }
    }
}