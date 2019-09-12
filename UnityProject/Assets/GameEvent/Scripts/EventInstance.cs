using System.Collections.Generic;

using Common;

namespace GameEvent {
    /// <summary>
    /// The instance used during event selection
    /// This will hold the actual Requirement classes
    /// </summary>
    public class EventInstance {
        private readonly EventData data;
        
        private readonly List<Requirement> requirements = new List<Requirement>(1);
        private readonly List<OptionInstance> options = new List<OptionInstance>(3);

        public EventInstance(EventData data) {
            this.data = data;
            PrepareRequirements();
            PrepareOptions();
        }

        private void PrepareRequirements() {
            int count = this.data.Requirements.Count;
            for (int i = 0; i < count; ++i) {
                Requirement requirement = TypeUtils.Instantiate<Requirement>(this.data.Requirements[i], null);
                this.requirements.Add(requirement);
            }
        }

        private void PrepareOptions() {
            int count = this.data.Options.Count;
            for (int i = 0; i < count; ++i) {
                // Note here that OptionInstance parses the requirements, costs, and effects
                OptionInstance option = new OptionInstance(this.data.Options[i]);
                this.options.Add(option);
            }
        }

        public EventData Data {
            get {
                return this.data;
            }
        }

        public IReadOnlyList<Requirement> Requirements {
            get {
                return this.requirements;
            }
        }

        public IReadOnlyList<OptionInstance> Options {
            get {
                return this.options;
            }
        }

        public int IntId {
            get {
                return this.data.IntId;
            }
        }
    }
}