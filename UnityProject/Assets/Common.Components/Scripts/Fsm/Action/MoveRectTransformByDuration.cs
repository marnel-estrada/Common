using UnityEngine;

namespace Common.Fsm.Action {
    /// <summary>
    /// A generic FSM actions that moves a RectTransform in a certain duration
    /// </summary>
    public class MoveRectTransformByDuration : FsmAction {

        private RectTransform transform;
        private Vector2 start;
        private Vector2 destination;
        private float duration;
        private string finishEvent;

        private readonly CountdownTimer timer;

        /// <summary>
        /// Constructor with specified time reference name
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="timeReferenceName"></param>
        public MoveRectTransformByDuration(FsmState owner, string timeReferenceName) : base(owner) {
            // 1 is dummy duration here
            // the one to be set in Init() will be set at OnEnter()
            this.timer = new CountdownTimer(1, timeReferenceName);
        }

        /// <summary>
        /// Initializer
        /// Action parameters are set here
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="start"></param>
        /// <param name="destination"></param>
        /// <param name="duration"></param>
        /// <param name="finishEvent"></param>
        public void Init(RectTransform transform, Vector2 start, Vector2 destination, float duration, string finishEvent) {
            this.transform = transform;
            Assertion.NotNull(this.transform);

            this.start = start;
            this.destination = destination;
            this.duration = duration;
            this.finishEvent = finishEvent;
        }

        public override void OnEnter() {
            if (this.duration.TolerantEquals(0)) {
                Finish();
                return;
            }

            if (this.start.TolerantEquals(this.destination)) {
                // positionFrom and positionTo are already the same
                Finish();
                return;
            }

            SetPosition(this.start);
            this.timer.Reset(this.duration);
        }

        public override void OnUpdate() {
            this.timer.Update();

            if (this.timer.HasElapsed()) {
                Finish();
                return;
            }

            // interpolate position
            SetPosition(Vector2.Lerp(this.start, this.destination, this.timer.GetRatio()));
        }

        private void Finish() {
            // snap to destination
            SetPosition(this.destination);

            if (!string.IsNullOrEmpty(this.finishEvent)) {
                GetOwner().SendEvent(this.finishEvent);
            }
        }

        private void SetPosition(Vector2 position) {
            this.transform.anchoredPosition = position;
        }

    }
}
