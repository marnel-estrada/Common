using Common;

namespace GoalSelector {
    public class GoalSelector {
        private readonly SimpleList<Goal> goals = new SimpleList<Goal>();

        public void Add(Goal goal) {
            this.goals.Add(goal);
        }

        public ReadOnlySimpleList<Goal> Goals {
            get {
                return new ReadOnlySimpleList<Goal>(this.goals);
            }
        }
    }
}