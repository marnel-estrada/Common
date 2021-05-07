using Common;

using CommonEcs.UtilityBrain;

namespace GoalSelector {
    /// <summary>
    /// This is the maintained instance when GoalData is parsed.
    /// </summary>
    public class Goal {
        private readonly string id;
        
        // Effect
        private readonly string conditionName;
        private readonly bool conditionValue;

        private readonly SimpleList<ConsiderationAssembler> considerationAssemblers = new SimpleList<ConsiderationAssembler>();

        public Goal(string id, string conditionName, bool conditionValue) {
            this.id = id;
            this.conditionName = conditionName;
            this.conditionValue = conditionValue;
        }

        public string Id {
            get {
                return this.id;
            }
        }

        public string ConditionName {
            get {
                return this.conditionName;
            }
        }

        public bool ConditionValue {
            get {
                return this.conditionValue;
            }
        }

        public void Add(ConsiderationAssembler assembler) {
            this.considerationAssemblers.Add(assembler);
        }

        public ReadOnlySimpleList<ConsiderationAssembler> Assemblers {
            get {
                return new ReadOnlySimpleList<ConsiderationAssembler>(this.considerationAssemblers);
            }
        }
    }
}