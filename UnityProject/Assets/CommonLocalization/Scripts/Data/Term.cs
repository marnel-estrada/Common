using System.Xml.Serialization;
using System;

namespace Common {
    [Serializable]
    public class Term {
        [XmlAttribute("id")]
        public string id;

        [XmlElement("translation")]
        public string translation;

        [XmlIgnore()]
        private string categoryName;

        public Term() { }
        
        public Term(string newId, string newTranslation, string newCategory) {
            this.id = newId;
            this.translation = newTranslation;

            this.CategoryName = newCategory;
        }

        public string CategoryName {
            get {
                return this.categoryName;
            }

            set {
                this.categoryName = value;
            }
        }
    }
}