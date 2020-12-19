using Common;

namespace GoapBrainEcs {
    /// <summary>
    /// A set of atom action composers that can be run in sequence
    /// Each composer would prepare the necessary components that the atom action needs to execute
    /// The systems that filters these components added by the composer will be the one that will
    /// actually execute the action and store results in AtomAction.status
    /// </summary>
    public class AtomActionSet {
        private readonly SimpleList<AtomActionComposer> composers = new SimpleList<AtomActionComposer>(4);

        public AtomActionSet() {
        }

        public AtomActionSet(params AtomActionComposer[] composerArray) {
            for (int i = 0; i < composerArray.Length; ++i) {
                this.composers.Add(composerArray[i]);
            }
        }

        /// <summary>
        /// Adds a composer
        /// </summary>
        /// <param name="composer"></param>
        public void Add(AtomActionComposer composer) {
            this.composers.Add(composer);
        }

        public int Count {
            get {
                return this.composers.Count;
            }
        }

        public AtomActionComposer GetComposerAt(int index) {
            return this.composers[index];
        }
    }
}