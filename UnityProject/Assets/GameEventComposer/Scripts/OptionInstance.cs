using System.Collections.Generic;

using Common;

namespace GameEvent {
    public class OptionInstance {
        private readonly OptionData data;
        
        private readonly List<Requirement> requirements = new List<Requirement>(1);
        private readonly List<Cost> costs = new List<Cost>(1);
        private readonly List<Effect> effects = new List<Effect>(1);

        public OptionInstance(OptionData data) {
            this.data = data;

            PrepareClassInstances(data.Requirements, this.requirements);
            PrepareClassInstances(data.Costs, this.costs);
            PrepareClassInstances(data.Effects, this.effects);
        }

        private static void PrepareClassInstances<T>(List<ClassData> dataList, List<T> instanceList) {
            int count = dataList.Count;
            for (int i = 0; i < count; ++i) {
                ClassData classData = dataList[i];
                T instance = TypeUtils.Instantiate<T>(classData, null);
                instanceList.Add(instance);
            }
        }

        public IReadOnlyList<Requirement> Requirements {
            get {
                return this.requirements;
            }
        }

        public IReadOnlyList<Cost> Costs {
            get {
                return this.costs;
            }
        }

        public IReadOnlyList<Effect> Effects {
            get {
                return this.effects;
            }
        }

        public OptionData Data {
            get {
                return this.data;
            }
        }
    }
}