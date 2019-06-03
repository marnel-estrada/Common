namespace Common {
    using UnityEditor;

    using UnityEngine;

    [CustomEditor(typeof(LockTransform))]
    public class LockTransformEditor : Editor {
        private LockTransform lockTransform;

        private void OnEnable() {
            this.lockTransform = this.target as LockTransform;
            Assertion.AssertNotNull(this.lockTransform);
        }

        private void OnSceneGUI() {
            ResetTransform();
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            ResetTransform();
        }

        private void ResetTransform() {
            this.lockTransform.transform.localPosition = VectorUtils.ZERO;
            this.lockTransform.transform.localRotation = Quaternion.identity;
            this.lockTransform.transform.localScale = VectorUtils.ONE;
        }
    }
}
