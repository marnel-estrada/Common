using UnityEditor;
using UnityEditor.SceneManagement;

namespace Common {
    /// <summary>
    /// Contains utilities to run certain parts of the game such as main game or editor.
    /// </summary>
    [InitializeOnLoad]
    public static class RunUtils {
        static RunUtils() {
            EditorApplication.playmodeStateChanged += LoadLastOpenedScene;
        }

        // pref IDs
        private const string LAST_OPENED_SCENE = "Game.LastOpenedScene";
        private const string PLAYED_USING_RUN_UTILS = "Game.PlayedUsingRunUtils";

        // bool states
        private static bool aboutToRun;

        [MenuItem("Game/Run Game #&z")]
        public static void Run() {
            SceneSetup[] setups = EditorSceneManager.GetSceneManagerSetup();
            if (setups.Length > 0) {
                EditorPrefs.SetString(LAST_OPENED_SCENE, setups[0].path);
            }

            EditorPrefs.SetBool(PLAYED_USING_RUN_UTILS, true);
            aboutToRun = true;

            // Refresh first to cause compilation and include new assets
            AssetDatabase.Refresh();

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene("Assets/Game/Scenes/Main.unity");
            EditorApplication.isPlaying = true;
        }

        private static void LoadLastOpenedScene() {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling) {
                // changed to playing or compiling
                // no need to do anything
                return;
            }
            
            if (!EditorPrefs.GetBool(PLAYED_USING_RUN_UTILS)) {
                // this means that normal play mode might have been used
                return;
            }

            // We added this check because this method is still invoked while EditorApplication.isPlaying is false
            // We only load the last opened scene when the aboutToRun flag is "consumed"
            if (aboutToRun) {
                aboutToRun = false;
                return;
            }

            // at this point, the scene has stopped playing 
            // so we load the last opened scene
            string lastScene = EditorPrefs.GetString(LAST_OPENED_SCENE);
            if (!string.IsNullOrEmpty(lastScene)) {
                EditorSceneManager.OpenScene(lastScene);
            }

            EditorPrefs.SetBool(PLAYED_USING_RUN_UTILS, false); // reset flag
        }
    }
}
