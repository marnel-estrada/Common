using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Delver {
    /// <summary>
    /// Represents a replay data for the QNetworkTest
    /// </summary>
    class QNetworkTestReplay {

        private readonly int state;
        private readonly int action; // This will also be the new state
        private readonly float reward;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        /// <param name="reward"></param>
        public QNetworkTestReplay(int state, int action, float reward) {
            this.state = state;
            this.action = action;
            this.reward = reward;
        }

        public int State {
            get {
                return state;
            }
        }

        public int Action {
            get {
                return action;
            }
        }

        public float Reward {
            get {
                return reward;
            }
        }

    }
}
