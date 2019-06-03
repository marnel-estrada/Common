using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Common;
using Common.Fsm;
using Common.Fsm.Action;

namespace Game {
    class NotificationOpenerBehaviour : MonoBehaviour {

        [SerializeField]
        private RectTransform displayRoot;

        // Number of pixels that shows a part of the display
        [SerializeField]
        private int exposedWidth = 60;

        private Fsm fsm;
        private Action showDetailsAction; // routines to show the details of the notification
        private Action removeAction; // routines on how to remove the notification

        /// <summary>
        /// Initializer
        /// </summary>
        public void Init(Action showDetailsAction, Action removeAction) {
            this.showDetailsAction = showDetailsAction;
            this.removeAction = removeAction;
        }

        void Awake() {
            PrepareFsm();
        }

        // initial state
        private const string PEEK = "Peek";

        // events
        private const string SHOW_DETAILS = "ShowDetails";
        private const string FINISHED = "Finished";

        private void PrepareFsm() {
            this.fsm = new Fsm("ScandalNotificationOpenerView");

            // states
            FsmState peek = fsm.AddState("Peek");
            FsmState hide = fsm.AddState("Hide");
            FsmState remove = fsm.AddState("Remove");

            // actions
            peek.AddAction(new FsmDelegateAction(peek, delegate (FsmState owner) {
                // Note that the position of the display rect starts at this.exposedWith
                // We animate to zero to peek the button
                LeanTween.moveX(this.displayRoot, 0, 0.5f).setEase(LeanTweenType.easeInQuad);
            }));

            hide.AddAction(new FsmDelegateAction(hide, delegate (FsmState owner) {
                this.showDetailsAction(); // invoke the action

                LeanTween.moveX(this.displayRoot, this.exposedWidth, 0.5f).setEase(LeanTweenType.easeInQuad).setOnComplete(delegate () {
                    owner.SendEvent(FINISHED);
                });
            }));

            remove.AddAction(new FsmDelegateAction(remove, delegate (FsmState owner) {
                this.removeAction(); // invoke the action
            }));

            // transitions
            peek.AddTransition(SHOW_DETAILS, hide);

            hide.AddTransition(FINISHED, remove);

            // We don't auto start
            // It will be started in Begin()
        }

        void Update() {
            this.fsm.Update();
        }

        /// <summary>
        /// Begins the behaviour
        /// </summary>
        public void Begin() {
            this.fsm.Start(PEEK);
        }

        /// <summary>
        /// Opens the details of the notification
        /// </summary>
        public void OpenDetails() {
            this.fsm.SendEvent(SHOW_DETAILS);
        }

    }
}
