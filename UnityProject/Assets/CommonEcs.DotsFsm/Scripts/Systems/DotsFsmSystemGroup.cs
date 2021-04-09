using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.DotsFsm {
    public class DotsFsmSystemGroup : ComponentSystemGroup {
        private NativeReference<bool> rerunGroup;

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
            int updateCounter = 0;
            do {
                this.rerunGroup.Value = false;
                base.OnUpdate();
                ++updateCounter;
                this.EntityManager.CompleteAllJobs();
            } while (this.rerunGroup.Value && updateCounter < 3);
        }

        public ref NativeReference<bool> RerunGroup {
            get {
                return ref this.rerunGroup;
            }
        }
    }
}