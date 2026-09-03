using System;
using UnityEngine;

namespace Common {
    /// <summary>
    /// Makes StreamingAssets data available to the StreamingAssetsCache on WebGL. Boot-critical
    /// files are baked into TextAssets (via the editor button) and stored synchronously in Awake,
    /// so frame-1 ECS reads find them without waiting for any async load. Non-critical files may
    /// still be fetched via UnityWebRequest through PreloadRoutine. Put this on the same GameObject
    /// as SceneLoadingSystem.
    /// </summary>
    [DefaultExecutionOrder(-10000)]   // populate the cache before any other Awake reads it
    public class StreamingAssetsPreloader : MonoBehaviour {
        [Serializable]
        public struct BakedTextAsset {
            [Tooltip("Relative StreamingAssets path, e.g. Game/Data/GameVariables.xml")]
            public string path;

            [Tooltip("Baked copy under Assets/Game/Data/Baked (filled by the Bake button).")]
            public TextAsset asset;
        }

        [Header("Bundled as TextAssets (synchronous, boot-safe)")]
        [SerializeField]
        private BakedTextAsset[] bakedAssets = Array.Empty<BakedTextAsset>();

        public BakedTextAsset[] BakedAssets => this.bakedAssets;

        private void Awake() {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Populate synchronously from bundled TextAssets so boot-critical reads (frame-1 ECS
            // systems) find the data with no async wait. Desktop/editor read live StreamingAssets
            // via the cache's File fallback, so this is WebGL-only.
            for (int i = 0; i < this.bakedAssets.Length; ++i) {
                BakedTextAsset entry = this.bakedAssets[i];
                if (entry.asset != null && !string.IsNullOrEmpty(entry.path)) {
                    StreamingAssetsCache.Store(entry.path, entry.asset.bytes);
                }
            }
#endif
        }
    }
}
