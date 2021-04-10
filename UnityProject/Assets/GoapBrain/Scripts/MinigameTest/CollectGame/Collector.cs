using Common;

using UnityEngine;

namespace GoapBrain {
    class Collector : GoapBrainTest {

        [SerializeField]
        private float speed = 1;

        [SerializeField]
        private Transform processorTransform;

        [SerializeField]
        private ShinyObjectPool pool;

        // Actions
        private const string GET_SHINY_OBJECT = "GetShinyObject";
        private const string PROCESS_SHINY_OBJECT = "ProcessShinyObject";
        private const string ROAM = "Roam";

        // Conditions
        private const string HAS_UNCLAIMED_SHINY_OBJECT = "HasUnclaimedShinyObject";
        private const string HAS_SHINY_OBJECT = "HasShinyObject";
        private const string SHINY_OBJECT_INCREASED = "ShinyObjectIncreased";

        private ShinyObject claimedObject;

        void Start() {
            Agent.ClearGoals();
            Agent.AddGoal(SHINY_OBJECT_INCREASED, true);
            Agent.Replan();
        }

        internal ShinyObjectPool Pool {
            get {
                return pool;
            }

            set {
                this.pool = value;
            }
        }

        public Transform Processor {
            get {
                return processorTransform;
            }

            set {
                this.processorTransform = value;
            }
        }

        /// <summary>
        /// Reserves an unclaimed shiny object
        /// </summary>
        public void ReserveUnclaimed() {
            // Get unclaimed
            ShinyObject target = this.pool.GetUnclaimed();
            target.Claimed = true;
            this.claimedObject = target;
        }

        /// <summary>
        /// Returns the claimed object's position
        /// </summary>
        /// <returns></returns>
        public Vector3 GetClaimedObjectPosition() {
            return this.claimedObject.transform.position;
        }

        /// <summary>
        /// Carries the unclaimed object
        /// </summary>
        public void CarryUnclaimedObject() {
            Assertion.NotNull(this.claimedObject);
            Transform claimedObjectTransform = this.claimedObject.transform;
            claimedObjectTransform.parent = this.transform;
            claimedObjectTransform.localPosition = new Vector3(0.05f, 0, 0);
        }

        /// <summary>
        /// Returns the processor's position
        /// </summary>
        /// <returns></returns>
        public Vector3 GetProcessorPosition() {
            return this.processorTransform.position;
        }

        /// <summary>
        /// Processes the claimed shiny object
        /// </summary>
        public void ProcessShinyObject() {
            this.pool.Remove(this.claimedObject);
            Destroy(this.claimedObject.gameObject);
            this.claimedObject = null;
        }

        /// <summary>
        /// Returns a new roaming position
        /// </summary>
        /// <returns></returns>
        public Vector3 GetNewRoamPosition() {
            return UnityEngine.Random.insideUnitCircle * 0.5f;
        }

        /// <summary>
        /// Returns whether or not their are unclaimed shiny object
        /// </summary>
        /// <returns></returns>
        public bool HasUnclaimedShinyObject() {
            if (this.pool == null) {
                // Was not set yet (for instantiated collectors)
                return false;
            }
            return this.pool.GetUnclaimed() != null;
        }

    }
}
