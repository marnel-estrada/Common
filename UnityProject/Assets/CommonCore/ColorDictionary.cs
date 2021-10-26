using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Common {
    [CreateAssetMenu(menuName = "Common/ColorDictionary")]
    public class ColorDictionary : ScriptableObject {
        [Serializable]
        public struct Entry {
            public string id;
            public Color color;
        }

        [Searchable]
        public Entry[]? entries;
        
        private Dictionary<string, Color>? map;

        public Color Get(string id) {
            Populate();
            Assertion.IsTrue(this.Map.TryGetValue(id, out Color color), id);
            return color;
        }

        private Dictionary<string, Color> Map {
            get {
                if (this.map == null) {
                    throw new CantBeNullException(nameof(this.map));
                }

                return this.map;
            }
        }

        private void Populate() {
            if (this.map != null) {
                // Already populated
                return;
            }

            if (this.entries == null) {
                throw new CantBeNullException(nameof(this.entries));
            }
            
            this.map = new Dictionary<string, Color>();
            for (int i = 0; i < this.entries.Length; ++i) {
                this.map.Add(this.entries[i].id, this.entries[i].color);
            }
        }
    }
}