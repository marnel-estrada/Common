using System.Collections.Generic;

using Common;

namespace GameEvent {
    public class EventRequirementFilterStrategy : DataPoolFilterStrategy<EventData> {
        public EventRequirementFilterStrategy(int labelWidth) : base("Event Requirement", labelWidth) {
        }

        public override bool IsFilterMet(EventData data) {
            List<ClassData> requirements = data.Requirements;
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