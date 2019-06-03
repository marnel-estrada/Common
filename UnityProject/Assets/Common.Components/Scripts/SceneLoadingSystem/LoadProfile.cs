using Common.Utils;

namespace Common {
    internal class LoadProfile {
        private IdentifiableSet<string> set;
        
        // These are scenes that are not in a set
        private SimpleList<string> scenes;

        /// <summary>
        /// Constructor with specified ID
        /// </summary>
        /// <param name="id"></param>
        public LoadProfile(string id) {
            this.set = new IdentifiableSet<string>(id);
        }

        public string Id {
            get {
                return this.set.Id;
            }
        }

        /// <summary>
        /// Adds a scene set
        /// </summary>
        /// <param name="sceneSetId"></param>
        public void AddSceneSet(string sceneSetId) {
            this.set.Add(sceneSetId);
        }

        public int SetCount {
            get {
                return this.set.Count;
            }
        }

        /// <summary>
        /// Returns the scene set ID at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetSceneSetAt(int index) {
            return this.set.GetAt(index);
        }

        public void AddScene(string scene) {
            // We lazy initialize because not every profile has separate scenes
            if (this.scenes == null) {
                this.scenes = new SimpleList<string>();
            }
            
            this.scenes.Add(scene);
        }

        public int SceneCount {
            get {
                if (this.scenes == null) {
                    return 0;
                }

                return this.scenes.Count;
            }
        }

        public string GetSceneAt(int index) {
            return this.scenes[index];
        }
    }
}
