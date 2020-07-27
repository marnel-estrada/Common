using Common.Signal;
using UnityEngine;

namespace Common {
    /// <summary>
    /// Handles localizer components
    /// This is implemented as a singleton
    /// </summary>
    public class LocalizerManager : MonoBehaviour {
        private TermsPool termsPool;

        private readonly SimpleList<Localizer> localizers = new SimpleList<Localizer>(500);

        private void Awake() {
            CommonLocalizationSignals.TERMS_CHANGED.AddListener(UpdateLocalizers);
        }

        private void OnDestroy() {
            CommonLocalizationSignals.TERMS_CHANGED.RemoveListener(UpdateLocalizers);
        }

        private TermsPool TermsPool {
            get {
                if (this.termsPool == null) {
                    TermsPool[] pools = FindObjectsOfType<TermsPool>();

                    if (pools.Length > 0) {
                        Assertion.IsTrue(pools.Length == 1); // There should be only one
                        this.termsPool = pools[0];
                    }
                }

                return this.termsPool;
            }
        }

        private void UpdateLocalizers(ISignalParameters parameters) {
            TermsPool pool = this.TermsPool;
            if(pool == null) {
                // There's no pool yet
                return;
            }

            // Update all localizers
            int count = this.localizers.Count;
            for (int i = 0; i < count; ++i) {
                this.localizers[i].UpdateText(pool.GetTranslation(this.localizers[i].TermId));
            }
        }

        /// <summary>
        /// Adds a localizer
        /// </summary>
        /// <param name="localizer"></param>
        public void Add(Localizer localizer) {
            // Update the text on add so that it will be updated with the latest loaded terms
            string termId = localizer.TermId;
            Option<string> translation = this.TermsPool.GetTranslation(termId);
            localizer.UpdateText(translation);
            this.localizers.Add(localizer);
        }

        /// <summary>
        /// Removes a localizer
        /// </summary>
        /// <param name="localizer"></param>
        public void Remove(Localizer localizer) {
            this.localizers.Remove(localizer);
        }

        private static LocalizerManager INSTANCE;

        public static LocalizerManager Instance {
            get {
                if(INSTANCE == null) {
                    // Create a new one
                    GameObject go = new GameObject("LocalizerManager");
                    go.AddComponent<DontDestroyOnLoadComponent>();

                    INSTANCE = go.AddComponent<LocalizerManager>();
                }

                return INSTANCE;
            }
        }
    }
}
