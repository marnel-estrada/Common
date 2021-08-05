namespace CommonEcs.UtilityBrain {
    public struct RankTuning : IConsiderationComponent {
        public readonly int rankToSet;

        public RankTuning(int rankToSet) {
            this.rankToSet = rankToSet;
        }
    }
}