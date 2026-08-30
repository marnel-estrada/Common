namespace Common {
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Networking;

    /// <summary>
    /// Fetches a configured set of StreamingAssets files via UnityWebRequest into the
    /// StreamingAssetsCache. Meant to run before scenes that read those files load
    /// (driven by SceneLoadingSystem on WebGL). Put this on the same GameObject as
    /// SceneLoadingSystem.
    /// </summary>
    public class StreamingAssetsPreloader : MonoBehaviour {
        [SerializeField]
        private string[] streamingAssetsToPreload = Array.Empty<string>();

        public IEnumerator PreloadRoutine() {
            for (int i = 0; i < this.streamingAssetsToPreload.Length; ++i) {
                string rel = this.streamingAssetsToPreload[i].Replace('\\', '/').TrimStart('/');
                using UnityWebRequest request = UnityWebRequest.Get($"{Application.streamingAssetsPath}/{rel}");
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success) {
                    Debug.LogError($"Failed to preload StreamingAsset '{rel}': {request.error}");
                    continue;
                }

                StreamingAssetsCache.Store(rel, request.downloadHandler.data);
            }
        }
    }
}
