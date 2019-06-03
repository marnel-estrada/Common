using System.Collections;

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneOpener : EditorWindow {

	[MenuItem("Window/SceneOpener #%r")]
	public static void Init() {
		SceneOpener window = EditorWindow.GetWindow<SceneOpener>();
	}
	
	private string sceneToOpen = "";
	
	private const string SCENE_CONTROL_NAME = "SceneToOpen";
	
	void OnGUI() {
		GUILayout.BeginVertical();
		GUILayout.Label("Scene Opener", EditorStyles.boldLabel);
		
		GUILayout.Space(10);
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Scene to open: ", GUILayout.Width(100));
		GUI.SetNextControlName(SCENE_CONTROL_NAME);
		this.sceneToOpen = EditorGUILayout.TextField(this.sceneToOpen, GUILayout.Width(150));
		GUILayout.EndHorizontal();
		
		GUILayout.Space(10);
		
		ListScenes();
		
		GUILayout.EndVertical();
		
		if(string.IsNullOrEmpty(GUI.GetNameOfFocusedControl())) {
			EditorGUI.FocusTextInControl(SCENE_CONTROL_NAME);
		}
		
		CheckKeyEvents();
	}

    private void ListScenes() {
		foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
			if(string.IsNullOrEmpty(this.sceneToOpen)) {
				// no need to filter if scene to open is empty
				GUILayout.Label(scene.path);
			} else if(scene.path.ToLower().Contains(this.sceneToOpen.ToLower())) {
				GUILayout.Label(scene.path);
			}
		}
	}
	
	private void CheckKeyEvents() {
		Event e = Event.current;
        if (e != null) {
            switch (e.type) {
                case EventType.KeyUp:
                    if (e.keyCode == KeyCode.Return) {
                        LoadScene();
                    }
                    break;
            }
        }
	}
	
	private void LoadScene() {
		if(string.IsNullOrEmpty(this.sceneToOpen)) {
			// don't load if scene to load is not specified
			return;
		}
		
		// load the first scene that meets the criteria
		foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
			if(scene.path.ToLower().Contains(this.sceneToOpen.ToLower())) {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(scene.path);
				Close();
				return;
			}
		}
	}

}
