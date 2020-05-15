using Common.Time;

using UnityEngine;

namespace Common.Fsm.Action {
    public class RotateByEulerRotationSpeed : FsmAction {
        private Transform transform;
        private Vector3 eulerRotationVelocity;
        private readonly TimeReference timeReference;
        
        public RotateByEulerRotationSpeed(FsmState owner, string timeReferenceName) : base(owner) {
            this.timeReference = TimeReferencePool.GetInstance().Get(timeReferenceName);
        }

        public void Init(Transform transform, Vector3 eulerRotationVelocity) {
            this.transform = transform;
            this.eulerRotationVelocity = eulerRotationVelocity;
        }

        public override void OnUpdate() {
            this.transform.Rotate(this.eulerRotationVelocity * this.timeReference.DeltaTime);
        }
    }
}