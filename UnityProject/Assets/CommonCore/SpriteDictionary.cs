using System;
using System.Collections.Generic;

using UnityEngine;

namespace Common {
    /// <summary>
    /// Contains a dictionary of sprites that objects can refer to
    /// </summary>
    [CreateAssetMenu(menuName = "Common/SpriteDictionary")]
    public class SpriteDictionary : ScriptableObject {
        [Serializable]
        public struct Entry {
            public string id;
            public Sprite sprite;
        }

        public Entry[]? entries;
        
        private Dictionary<string, Sprite>? map;

        public Sprite Get(string id) {
            Populate();

            if (this.map == null) {
                throw new CantBeNullException(nameof(this.map));
            }
            
            Assertion.IsTrue(this.map.TryGetValue(id, out Sprite sprite), id);
            return sprite;
        }

        private void Populate() {
            if (this.map != null) {
                // Already populated
                return;
            }

            if (this.entries == null) {
                throw new CantBeNullException(nameof(this.entries));
            }
            
            this.map = new Dictionary<string, Sprite>();
            for (int i = 0; i < this.entries.Length; ++i) {
                this.map.Add(this.entries[i].id, this.entries[i].sprite);
            }
        }
    }
}