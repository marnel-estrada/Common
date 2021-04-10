using System.Collections.Generic;

using Common;

namespace GoapBrain {
    /// <summary>
    /// Handles the state of conditions during planning
    /// </summary>
    class PlanningConditionsHandler {
        private readonly Dictionary<ConditionId, bool> finalValues = new Dictionary<ConditionId, bool>();
        private readonly ListDictionary<object, PlanningConditionsCheckpoint> checkpoints = new ListDictionary<object, PlanningConditionsCheckpoint>();

        /// <summary>
        /// Clears the handler
        /// </summary>
        public void Clear() {
            this.finalValues.Clear();

            // Recycle the instances since they are pooled
            for(int i = 0; i < this.checkpoints.Count; ++i) {
                this.checkpointPool.Recycle(this.checkpoints.GetAt(i));
            }
            this.checkpoints.Clear();
        }

        /// <summary>
        /// Adds a checkpoint
        /// </summary>
        /// <param name="id"></param>
        public void AddCheckpoint(object id) {
            PlanningConditionsCheckpoint checkpoint = ResolveNewCheckpoint(this.checkpoints.Count);
            this.checkpoints.Add(id, checkpoint);
        }

        /// <summary>
        /// Returns whether or not the specified checkpoint was already added
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsCheckpoint(object id) {
            return this.checkpoints.ContainsKey(id);
        }

        private readonly Pool<PlanningConditionsCheckpoint> checkpointPool = new Pool<PlanningConditionsCheckpoint>();

        private PlanningConditionsCheckpoint ResolveNewCheckpoint(int index) {
            PlanningConditionsCheckpoint newCheckpoint = this.checkpointPool.Request();
            newCheckpoint.Init(index);
            return newCheckpoint;
        }

        /// <summary>
        /// Commits the specified change
        /// </summary>
        /// <param name="conditionId"></param>
        /// <param name="previousValue"></param>
        /// <param name="updatedValue"></param>
        public void CommitChange(ConditionId conditionId, bool previousValue, bool updatedValue) {
            this.LatestCheckpoint.Add(conditionId, previousValue, updatedValue);
            this.finalValues[conditionId] = updatedValue;
        }

        private PlanningConditionsCheckpoint LatestCheckpoint {
            get {
                return this.checkpoints.GetAt(this.checkpoints.Count - 1);
            }
        }

        /// <summary>
        /// Removes the checkpoint and also reverts the changes by that checkpoint
        /// </summary>
        /// <param name="id"></param>
        public void RemoveCheckpoint(object id) {
            Option<PlanningConditionsCheckpoint> checkpoint = this.checkpoints.Find(id);
            checkpoint.Match(new RemoveCheckpointMatcher(this));
        }
        
        private readonly struct RemoveCheckpointMatcher : IOptionMatcher<PlanningConditionsCheckpoint> {
            private readonly PlanningConditionsHandler handler;

            public RemoveCheckpointMatcher(PlanningConditionsHandler handler) {
                this.handler = handler;
            }

            public void OnSome(PlanningConditionsCheckpoint checkpoint) {
                // Revert values from the last checkpoint until to this checkpoint
                for(int i = this.handler.checkpoints.Count - 1; i >= checkpoint.Index; --i) {
                    this.handler.checkpoints.GetAt(i).Revert(this.handler.finalValues);
                }

                // Then remove them
                while(this.handler.checkpoints.Count > checkpoint.Index) {
                    // Remove the last item until up to the specified checkpoint
                    this.handler.checkpointPool.Recycle(this.handler.checkpoints.GetAt(this.handler.checkpoints.Count - 1)); // Recycle because its pooled
                    this.handler.checkpoints.RemoveAt(this.handler.checkpoints.Count - 1);
                }
            }

            public void OnNone() {
            }
        }

        /// <summary>
        /// Returns whether or not the handler has the specified condition
        /// </summary>
        /// <param name="conditionId"></param>
        /// <returns></returns>
        public bool HasCondition(ConditionId conditionId) {
            return this.finalValues.ContainsKey(conditionId);
        }

        /// <summary>
        /// Returns the value of the specified condition
        /// </summary>
        /// <param name="conditionId"></param>
        /// <returns></returns>
        public bool GetValue(ConditionId conditionId) {
            return this.finalValues[conditionId];
        }
    }
}
