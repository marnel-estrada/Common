using UnityEngine;

namespace Common.Fsm.Action {
    public class MoveToTargetTransform : FsmAction {
        private Transform agent;
        private Transform target;
        private float duration;
        private string finishEvent;
        private readonly CountdownTimer timer;
        private Space space;

        private Vector3 start;

        public MoveToTargetTransform(FsmState owner, string timeReferenceName) : base(owner) {
            this.timer = new CountdownTimer(1, timeReferenceName);
        }

        public void Init(Transform agent, Transform target, float duration, string finishEvent,
            Space space = Space.World) {
            this.agent = agent;
            this.target = target;
            this.duration = duration;
            this.finishEvent = finishEvent;
            this.space = space;
        }

        public override void OnEnter() {
            if(this.duration.TolerantEquals(0)) {
                Finish();
                return;
            }
			
            if(this.agent.position.TolerantEquals(this.target.position)) {
                // positionFrom and positionTo are already the same
                Finish();
                return;
            }

            this.start = this.agent.position;
            this.timer.Reset(this.duration);
        }
        
        public override void OnUpdate() {
            this.timer.Update();
			
            if(this.timer.HasElapsed()) {
                Finish();
                return;
            }
			
            // interpolate position
            SetPosition(Vector3.Lerp(this.start, this.target.position, this.timer.GetRatio()));
        }

        private void SetPosition(Vector3 position) {
            switch(this.space) {
                case Space.World:
                    this.agent.position = position;
                    break;

                case Space.Self:
                    this.agent.localPosition = position;
                    break;
            }
        }
        
        private void Finish() {
            // snap to target
            SetPosition(this.target.position);
			
            if(!string.IsNullOrEmpty(this.finishEvent)) {
                GetOwner().SendEvent(this.finishEvent);
            }
        }
    }
}