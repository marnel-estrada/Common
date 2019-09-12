using Common;

using UnityEngine;

namespace GameEvent {
    [Group("GameEvent.Sample")]
    public class SamplePrestigeEffect : Effect {
        public override void Apply() {
            // Just a sample
            Debug.Log("SamplePrestigeEffect");
        }
    }
}