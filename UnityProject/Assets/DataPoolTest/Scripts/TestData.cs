using System;

using Common;

using UnityEngine;

namespace Test {
    [Serializable]
    public class TestData : Identifiable, IntIdentifiable {
        [SerializeField]
        private int intId;
        
        [SerializeField]
        private string textId;

        [SerializeField]
        private string textProperty;

        public string Id {
            get {
                return this.textId;
            }

            set {
                this.textId = value;
            }
        }

        public int IntId {
            get {
                return this.intId;
            }
            set {
                this.intId = value;
            }
        }

        public string TextProperty {
            get {
                return this.textProperty;
            }
            set {
                this.textProperty = value;
            }
        }
    }
}