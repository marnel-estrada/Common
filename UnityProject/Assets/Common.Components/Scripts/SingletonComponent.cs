using UnityEngine;

namespace Common {
    public struct SingletonComponent<T> where T : Component {
        private T instance;

        public T Instance {
            get {
                if (this.instance == null) {
                    GameObject go = new GameObject(typeof(T).Name);
                    go.AddComponent<DontDestroyOnLoadComponent>();
                    this.instance = go.AddComponent<T>();
                }
                
                return this.instance;
            }
        }
    }
}