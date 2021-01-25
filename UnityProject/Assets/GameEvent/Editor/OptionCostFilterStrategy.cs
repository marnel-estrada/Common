using System.Collections.Generic;

using Common;

namespace GameEvent {
    public class OptionCostFilterStrategy : DataPoolFilterStrategy<EventData> {
        public OptionCostFilterStrategy(int labelWidth) : base("Cost Name", labelWidth) {
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
            List<ClassData> costs = option.Costs;
            for (int i = 0; i < costs.Count; ++i) {
                ClassData classData = costs[i];
                if (classData.ClassName.ToLower().Contains(this.FilterText.ToLower())) {
                    return true;
                }
            }

            return false;
        }
    }
}