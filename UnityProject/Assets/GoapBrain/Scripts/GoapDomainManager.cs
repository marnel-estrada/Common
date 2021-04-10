using Common;

using UnityEngine;

namespace GoapBrain {
    /// <summary>
    /// The hidden manager for GoapDomain instances
    /// Implemented as a singleton
    /// </summary>
    class GoapDomainManager : MonoBehaviour {
        private readonly SimpleList<GoapDomain> entries = new SimpleList<GoapDomain>();

        /// <summary>
        /// Adds a domain
        /// </summary>
        /// <param name="domain"></param>
        public void Add(GoapDomain domain) {
            this.entries.Add(domain);
        }

        /// <summary>
        /// Removes the specified domain
        /// </summary>
        /// <param name="domain"></param>
        public void Remove(GoapDomain domain) {
            this.entries.Remove(domain);
        }

        // Limit updating of GoapDomain to this count
        private const int UPDATE_COUNT_PER_FRAME = 400;
        private int currentIndex;

        /// <summary>
        /// Note that we did it this way because Update() is slow as mentioned here https://blogs.unity3d.com/2015/12/23/1k-update-calls/
        /// </summary>
        private void Update() {
            int entriesCount = this.entries.Count;

            if (entriesCount < UPDATE_COUNT_PER_FRAME) {
                // Just update every entry
                for (int i = 0; i < this.entries.Count; ++i) {
                    GoapDomain domain = this.entries[i];
                    if (!domain.gameObject.activeInHierarchy) {
                        // Not active
                        continue;
                    }
                
                    domain.ExecuteUpdate();
                }

                return;
            }
            
            for (int i = 0; i < UPDATE_COUNT_PER_FRAME; ++i) {
                if (this.currentIndex >= entriesCount) {
                    // We do this check because some entries might have been already removed
                    this.currentIndex = 0;
                }
                
                GoapDomain domain = this.entries[this.currentIndex];
                this.currentIndex = (this.currentIndex + 1) % entriesCount;
                
                if (!domain.gameObject.activeInHierarchy) {
                    // Not active
                    continue;
                }

                domain.ExecuteUpdate();
            }
        }

        private static GoapDomainManager INSTANCE;

        public static GoapDomainManager Instance {
            get {
                if (INSTANCE == null) {
                    GameObject go = new GameObject("GoapDomainManager");
                    go.AddComponent<DontDestroyOnLoadComponent>();
                    INSTANCE = go.AddComponent<GoapDomainManager>();
                }

                return INSTANCE;
            }
        }
    }
}
