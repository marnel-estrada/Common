using System;
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

        private Action<Requirement> addRequirementMatcher;

        public EventInstance(EventData data) {
            this.data = data;
            
            this.addRequirementMatcher = delegate(Requirement requirement) {
                this.requirements.Add(requirement);
            };
            
            PrepareRequirements();
            PrepareOptions();
        }

        private void PrepareRequirements() {
            int count = this.data.Requirements.Count;
            for (int i = 0; i < count; ++i) {
                Option<Requirement> requirement = TypeUtils.Instantiate<Requirement>(this.data.Requirements[i], null);
                requirement.Match(this.addRequirementMatcher);
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

        public bool AreAllRequirementsMet {
            get {
                for (int i = 0; i < this.requirements.Count; ++i) {
                    if (!this.requirements[i].IsMet()) {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool HasSelectableOption {
            get {
                for (int i = 0; i < this.options.Count; ++i) {
                    OptionInstance option = this.options[i];
                    if (option.IsSelectable) {
                        // Found at least one option that can be selectable
                        return true;
                    }
                }

                return false;
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