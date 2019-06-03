using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Common;
using Common.Fsm;

namespace Common.Fsm.Action {
    public class SetImageColorByDuration : FsmAction {

        private Image image;
        private Color startColor;
        private Color endColor;
        private float duration;
        private string finishEvent;

        private CountdownTimer timer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="owner"></param>
        public SetImageColorByDuration(FsmState owner, string timeReferenceName) : base(owner) {
            // dummy time specified here
            // actual duration will be set at OnEnter()
            this.timer = new CountdownTimer(1, timeReferenceName);
        }

        /// <summary>
        /// Initializer
        /// Sets the variables needed by the action
        /// </summary>
        /// <param name="image"></param>
        /// <param name="startColor"></param>
        /// <param name="endColor"></param>
        /// <param name="duration"></param>
        /// <param name="finishEvent"></param>
        public void Init(Image image, Color startColor, Color endColor, float duration, string finishEvent) {
            this.image = image;
            this.startColor = startColor;
            this.endColor = endColor;
            this.duration = duration;
            this.finishEvent = finishEvent;
        }

        public override void OnEnter() {
            if(Comparison.TolerantEquals(this.duration, 0)) {
                // zero duration was specified finish immediately
                Finish();
                return;
            }

            // set the initial color to the starting color
            this.timer.Reset(this.duration);
            this.image.color = this.startColor;
        }

        public override void OnUpdate() {
            this.timer.Update();

            if(this.timer.HasElapsed()) {
                // duration has elapsed
                Finish();
                return;
            }

            // interpolate color
            this.image.color = Color.Lerp(this.startColor, this.endColor, this.timer.GetRatio());
        }

        private void Finish() {
            // snap to end color
            image.color = this.endColor;

            // send finishEvent if specified
            if (!string.IsNullOrEmpty(this.finishEvent)) {
                GetOwner().SendEvent(this.finishEvent);
            }
        }

    }
}
