namespace Common.Editor {
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(StreamingAssetsPreloader))]
    public class StreamingAssetsPreloaderEditor : UnityEditor.Editor {
        private const string BakedDir = "Assets/Game/Data/Baked";

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Bake StreamingAssets → Game/Data/Baked")) {
                Bake();
            }
        }

        private void Bake() {
            StreamingAssetsPreloader preloader = (StreamingAssetsPreloader) this.target;
            StreamingAssetsPreloader.BakedTextAsset[] entries = preloader.BakedAssets;

            if (!Directory.Exists(BakedDir)) {
                Directory.CreateDirectory(BakedDir);
            }

            Undo.RecordObject(preloader, "Bake StreamingAssets");

            int baked = 0;
            for (int i = 0; i < entries.Length; ++i) {
                string relPath = entries[i].path;
                if (string.IsNullOrEmpty(relPath)) {
                    continue;
                }

                string source = Path.Combine(Application.streamingAssetsPath, relPath);
                if (!File.Exists(source)) {
                    Debug.LogError($"StreamingAssets file not found: {source}");
                    continue;
                }

                string dest = $"{BakedDir}/{Path.GetFileName(relPath)}";
                File.Copy(source, dest, true);
                AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceUpdate);

                entries[i].asset = AssetDatabase.LoadAssetAtPath<TextAsset>(dest);
                ++baked;
            }

            EditorUtility.SetDirty(preloader);
            AssetDatabase.SaveAssets();
            Debug.Log($"Baked {baked} StreamingAssets file(s) into {BakedDir}.");
        }
    }
}
