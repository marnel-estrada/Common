using UnityEngine;
using Common.Fsm;
using Common.Fsm.Action;

namespace Common {
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(SwarmItem))]
    public class SoundEffectPlayer : MonoBehaviour {
        public enum PlayerType {
            Default,
            Ambient
        }

        [SerializeField]
        private string timeReferenceName;

        [SerializeField]
        private PlayerType playerType;

        private AudioSource source;
        private SwarmItem swarmItem;

        private Common.Fsm.Fsm fsm;

        private Transform selfTransform;

        void Awake() {
            this.source = GetComponent<AudioSource>();
            Assertion.AssertNotNull(this.source);

            this.swarmItem = GetComponent<SwarmItem>();
            Assertion.AssertNotNull(this.swarmItem);

            this.selfTransform = this.transform;

            PrepareFsm();
        }

        // initial state
        private const string PLAYING = "Playing";

        // events
        private const string FINISHED = "Finished";

        private void PrepareFsm() {
            this.fsm = new Common.Fsm.Fsm("SfxPlayer." + this.gameObject.name);

            // states
            FsmState playing = this.fsm.AddState(PLAYING);
            FsmState killed = this.fsm.AddState("Killed");

            // actions
            TimedWaitAction wait = new TimedWaitAction(playing, this.timeReferenceName, FINISHED);

            playing.AddAction(new FsmDelegateAction(playing, delegate {
                this.source.Play();

                // initialize wait timer before it is started
                wait.Init(this.source.clip.length + 0.1f); // a little offset to make sure that sound finishes playing
            }));

            playing.AddAction(wait);

            if (this.playerType != PlayerType.Ambient) {
                killed.AddAction(new FsmDelegateAction(killed, delegate {
                    KillPlayer();
                }));
            }

            // transitions
            playing.AddTransition(FINISHED, killed);

            // fsm would be started at Play()
        }

        public virtual void KillPlayer() {
            this.playerType = PlayerType.Default;
            this.source.clip = null;
            // Stop this fsm
            this.fsm.Stop();
            this.swarmItem.Kill();
        }

        /// <summary>
        /// Plays the specified clip at the origin
        /// </summary>
        /// <param name="clip"></param>
        public void Play(AudioClip clip) {
            Play(clip, VectorUtils.ZERO);
        }

        /**
         * Initializes the player with a clip and position. Plays the sound effect. Auto kills self.
         */
        public void Play(AudioClip clip, Vector3 position) {
            this.source.clip = clip;
            this.selfTransform.position = position;

            this.fsm.Start(PLAYING);
        }

        private void Update() {
            this.fsm.Update();
        }

        public float Volume {
            get {
                return this.source.volume;
            }

            set {
                this.source.volume = value;
            }
        }

        public AudioSource Source {
            get {
                return this.source;
            }

            set {
                this.source = value;
            }
        }

        public Vector3 PlayerPosition {
            get {
                return this.selfTransform.position;
            }
            set {
                this.selfTransform.position = value;
            }
        }
    }
}