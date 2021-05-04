namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// A consideration used for tuning bonus. It sets a user specified bonus.
    /// </summary>
    public readonly struct BonusTuning : IConsiderationComponent {
        public readonly float bonusToSet;

        public BonusTuning(float bonusToSet) {
            this.bonusToSet = bonusToSet;
        }
    }
}