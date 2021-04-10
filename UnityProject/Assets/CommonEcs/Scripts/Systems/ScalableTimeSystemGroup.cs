using Common;

using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A system group which can be controlled how many times it runs per frame 
    /// </summary>
    public class ScalableTimeSystemGroup : ComponentSystemGroup {
        private int updateCount = 1;

        public int UpdateCount {
            get {
                return this.updateCount;
            }
            
            set {
                this.updateCount = value;
                Assertion.IsTrue(this.updateCount > 0);
            }
        }

        protected override void OnUpdate() {
            for (int i = 0; i < this.updateCount; ++i) {
                base.OnUpdate();
            }
        }
    }
}