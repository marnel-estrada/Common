using Common;

namespace GoapBrain {
    public class ActionListSearchResult {
        private readonly SimpleList<GoapAction> actions = new SimpleList<GoapAction>();
        private bool successful;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionListSearchResult() {
            // Default to true
            this.successful = true;
        }

        /// <summary>
        /// Clears the search result
        /// </summary>
        public void Clear() {
            this.actions.Clear();
            this.successful = true;
        }

        public bool Successful {
            get {
                return successful;
            }
        }

        /// <summary>
        /// Marks the search result as successful
        /// </summary>
        public void MarkAsSuccessful() {
            this.successful = true;
        }

        /// <summary>
        /// Marks the search result as a failure
        /// </summary>
        public void MarkAsFailed() {
            this.successful = false;
        }

        /// <summary>
        /// Adds an action to the search result
        /// </summary>
        /// <param name="action"></param>
        public void Add(GoapAction action) {
            this.actions.Add(action);
        }

        /// <summary>
        /// Adds all the entries in the other result list
        /// </summary>
        /// <param name="other"></param>
        public void AddAll(ActionListSearchResult other) {
            int count = other.actions.Count;
            for (int i = 0; i < count; ++i) {
                this.actions.Add(other.actions[i]);
            }
        }

        public int Count {
            get {
                return this.actions.Count;
            }
        }

        /// <summary>
        /// Returns the action at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GoapAction GetActionAt(int index) {
            return this.actions[index];
        }
    }
}
