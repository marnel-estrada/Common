using System.Collections.Generic;

using Common;

namespace GameEvent {
    public class OptionRequirementFilterStrategy : DataPoolFilterStrategy<EventData> {
        public OptionRequirementFilterStrategy(int labelWidth) : base("Option Requirement", labelWidth) {
        }

        public override bool IsFilterMet(EventData data) {
            List<OptionData> options = data.Options;
            for (int i = 0; i < options.Count; ++i) {
                OptionData option = options[i];
                if (IsFilterMet(option)) {
                    return true;
                }
            }

            return false;
        }

        private bool IsFilterMet(OptionData option) {
            List<ClassData> requirements = option.Requirements;
            for (int i = 0; i < requirements.Count; ++i) {
                ClassData classData = requirements[i];
                if (classData.ClassName.ToLower().Contains(this.FilterText.ToLower())) {
                    return true;
                }
            }

            return false;
        }
    }
}