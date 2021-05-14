using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.DotsFsm {
    [UpdateInGroup(typeof(ScalableTimeSystemGroup))]
    public class DotsFsmSystemGroup : ComponentSystemGroup {
        private NativeReference<bool> rerunGroup;
        private int rerunCounter;

        protected override void OnCreate() {
            base.OnCreate();

            this.rerunGroup = new NativeReference<bool>(Allocator.Persistent);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            this.rerunGroup.Dispose();
        }

        protected override void OnUpdate() {
            // Allow rerun only twice
            this.rerunCounter = 0;
            do {
                this.rerunGroup.Value = false;
                base.OnUpdate();
                ++this.rerunCounter;

                if (this.rerunGroup.Value) {
                    // Force complete only if rerun was requested
                    this.EntityManager.CompleteAllJobs();
                }
            } while (this.rerunGroup.Value && this.rerunCounter < 3);
        }

        public ref NativeReference<bool> RerunGroup {
            get {
                return ref this.rerunGroup;
            }
        }

        public int RerunCounter {
            get {
                return this.rerunCounter;
            }
        }
    }
}