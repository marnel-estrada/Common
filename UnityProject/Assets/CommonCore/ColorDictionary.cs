using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common {
    [CreateAssetMenu(menuName = "Common/ColorDictionary")]
    public class ColorDictionary : ScriptableObject {
        [Serializable]
        public struct Entry {
            public string id;
            public Color color;
        }

        public Entry[]? entries;
        
        private Dictionary<string, Color>? map;

        public Color Get(string id) {
            Populate();

            if (this.Map.TryGetValue(id, out Color color)) {
                return color;
            }

            throw new Exception($"Can't find color for '{id}'");
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