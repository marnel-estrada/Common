using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using Common.Xml;

namespace Common {
    public class SceneLoadingSystem : MonoBehaviour {
        [SerializeField]
        private TextAsset configXml;

        [SerializeField]
        private string loadProfileToExecute;

        [SerializeField]
        private bool showLoadingTime;

        private Dictionary<string, LoadProfile> profileMap;
        private Dictionary<string, SceneSet> sceneSetMap;

        public bool ShowLoadingTime {
            get {
                return this.showLoadingTime;
            }
            set {
                this.showLoadingTime = value;
            }
        }

        private void Awake() {
            Assertion.NotNull(configXml);

            this.profileMap = new Dictionary<string, LoadProfile>();
            this.sceneSetMap = new Dictionary<string, SceneSet>();
            Parse();

            if(!string.IsNullOrEmpty(this.loadProfileToExecute)) {
                Load(this.loadProfileToExecute);
            }
        }

        // elements
        private const string LOAD_PROFILE = "LoadProfile";
        private const string SCENE_SET = "SceneSet";

        private void Parse() {
            SimpleXmlReader reader = new SimpleXmlReader();
            SimpleXmlNode root = reader.Read(this.configXml.text).FindFirstNodeInChildren("SceneLoadingSystem");

            for(int i = 0; i < root.Children.Count; ++i) {
                SimpleXmlNode child = root.Children[i];

                switch(child.TagName) {
                    case LOAD_PROFILE:
                        ParseLoadProfile(child);
                        break;

                    case SCENE_SET:
                        ParseSceneSet(child);
                        break;
                }
            }
        }
         
        private const string SCENE = "Scene";
        
        // attributes
        private const string ID = "id";
        private const string NAME = "name";

        private void ParseLoadProfile(SimpleXmlNode node) {
            string id = node.GetAttribute(ID);
            LoadProfile profile = new LoadProfile(id);

            // parse scene sets in the profile
            for(int i = 0; i < node.Children.Count; ++i) {
                SimpleXmlNode child = node.Children[i];

                if(SCENE_SET.EqualsFast(child.TagName)) {
                    // add the id
                    profile.AddSceneSet(child.GetAttribute(ID));
                }

                if (SCENE.EqualsFast(child.TagName)) {
                    profile.AddScene(child.GetAttribute(NAME));
                }
            }

            this.profileMap[id] = profile;
        }


        private void ParseSceneSet(SimpleXmlNode node) {
            string id = node.GetAttribute(ID);
            SceneSet sceneSet = new SceneSet(id);

            // parse scenes in the set
            for(int i = 0; i < node.Children.Count; ++i) {
                SimpleXmlNode child = node.Children[i];
                if(SCENE.Equals(child.TagName)) {
                    sceneSet.Add(child.GetAttribute(NAME));
                }
            }

            this.sceneSetMap[id] = sceneSet;
        }

        private DateTime startTime;

        /// <summary>
        /// Loads the specified load profile
        /// </summary>
        /// <param name="loadProfileId"></param>
        /// <param name="actionAfterLoading"></param>
        public void Load(string loadProfileId, Action actionAfterLoading = null) {
            Assertion.IsTrue(this.profileMap.TryGetValue(loadProfileId, out LoadProfile loadProfile));

            this.startTime = DateTime.Now;
            
            // Load sets
            for(int i = 0; i < loadProfile.SetCount; ++i) {
                LoadSceneSet(loadProfile.GetSceneSetAt(i));
            }
            
            // Load scenes
            for (int i = 0; i < loadProfile.SceneCount; ++i) {
                LoadScene(loadProfile.GetSceneAt(i));
            }
            
            // Invoke if it exists
            actionAfterLoading?.Invoke();
        }

        private void LoadSceneSet(string sceneSetId) {
            Assertion.IsTrue(this.sceneSetMap.TryGetValue(sceneSetId, out SceneSet sceneSet));

            for(int i = 0; i < sceneSet.Count; ++i) {
                string sceneName = sceneSet.GetAt(i);
                LoadScene(sceneName);
            }
        }

        private void LoadScene(string sceneName) {
            LoadSceneAdditively(sceneName);

            if (this.showLoadingTime) {
                Debug.LogFormat($"{sceneName} - Time since load start: {(DateTime.Now - this.startTime).TotalSeconds} seconds");
            }
        }

        /// <summary>
        /// Common algorithm for loading a scene additively
        /// </summary>
        /// <param name="sceneName"></param>
        public static void LoadSceneAdditively(string sceneName) {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }
}
