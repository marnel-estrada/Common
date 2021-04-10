using UnityEngine;
using Common;

namespace GoapBrain {
    [Group("GoapBrain.General")]
    public class MoveBySpeed : GoapAtomAction {
        public NamedString timeReferenceName { get; set; }
        public NamedFloat speed { get; set; }
        public NamedVector3 destination { get; set; }
        public NamedBool ignoreZ { get; set; }

        private Transform agentTransform;
        private Vector3 startPosition;
        private Vector3 destPosition;
        private CountdownTimer timer;

        /// <summary>
        /// Empty constructor
        /// </summary>
        public MoveBySpeed() {
        }

        /// <summary>
        /// Constructor with some settings
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="ignoreZ"></param>
        public MoveBySpeed(float speed, bool ignoreZ) {
            this.speed = new NamedFloat();
            this.speed.Name = "speed";
            this.speed.Value = speed;

            this.destination = new NamedVector3();
            this.destination.Name = "destination";

            this.ignoreZ = new NamedBool();
            this.ignoreZ.Name = "ignoreZ";
            this.ignoreZ.Value = ignoreZ;
        }

        public Vector3 Destination {
            get {
                return this.destination.Value;
            }

            set {
                this.destination.Value = value;
            }
        }

        public override GoapResult Start(GoapAgent agent) {
            if (this.timer == null) {
                // We instantiate here because timeReferenceName might not have been assigned in constructor
                this.timer = new CountdownTimer(1, this.timeReferenceName.Value);
            }

            // Preparations
            this.agentTransform = agent.transform;
            this.startPosition = this.agentTransform.position;

            // Compute time to arrive
            this.destPosition = this.destination.Value;
            if (this.ignoreZ.Value) {
                // Just copy z of the agent if z is to be ignored (used for 2D)
                this.destPosition.z = this.agentTransform.position.z;
            }

            if (VectorUtils.Equals(this.destPosition, this.agentTransform.position)) {
                // Agent is already at the destination
                // No need to run
                return GoapResult.SUCCESS;
            }

            float distance = (this.destPosition - this.agentTransform.position).magnitude;
            float duration = distance / this.speed.Value;
            this.timer.Reset(duration);

            return GoapResult.RUNNING;
        }

        public override GoapResult Update(GoapAgent agent) {
            this.timer.Update();
            this.agentTransform.position = Vector3.Lerp(this.startPosition, this.destPosition, this.timer.GetRatio());

            if (this.timer.HasElapsed()) {
                return GoapResult.SUCCESS;
            }

            return GoapResult.RUNNING;
        }
    }
}