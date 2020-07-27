using UnityEngine;

using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;

namespace Common {
    [Serializable]
    [XmlRoot("language")]
    public class TranslationContainer {
        //<language id = "English" label="Base English" core="en-US">

        [XmlAttribute("id")]
        public string id;

        [XmlAttribute("label")]
        public string label;

        [XmlAttribute("core")]
        public string core;

        [XmlElement("category")]
        public List<Category> catList;

        public TranslationContainer() {
            this.catList = new List<Category>();
        }

        public TranslationContainer(string newId, string newLabel, string newCore) {
            this.id = newId;
            this.label = newLabel;
            this.core = newCore;

            this.catList = new List<Category>();
        }

        private List<Term> termList = new List<Term>();

        private Dictionary<string, Term> termDictionary = new Dictionary<string, Term>();

        //This might not be needed, but it's good for an exact matching kind of search for category.
        private Dictionary<string, Category> catDictionary = new Dictionary<string, Category>();

        //Caches, Indices, and whatever.

        public static TranslationContainer Load(string urlPath) {
            XmlSerializer serializer = new XmlSerializer(typeof(TranslationContainer));

            StreamReader reader = null;
            TranslationContainer t = null;

            using (reader = new StreamReader(urlPath)) {
                t = serializer.Deserialize(reader) as TranslationContainer;

                reader.Close();
            }

            if (t != null) {
                //At this point, catList should already be populated!
                t.SetupLists();

                return t;
            }

            return null; //OOPS.
        }

        public Option<Term> GetTerm(string id) {
            if (this.termDictionary.Count <= 0) {
                SetupLists();
            }

            return this.termDictionary.Find(id);
        }

        public void SetupLists() {
            this.termDictionary.Clear();
            this.catDictionary.Clear();
            this.termList.Clear();

            foreach (Category cat in this.catList) {
                this.termList.AddRange(cat.termList);
                this.catDictionary.Add(cat.name, cat);
                foreach (Term term in cat.termList) {
                    term.CategoryName = cat.name;

                    string id = term.id.Trim();

                    if (!string.IsNullOrEmpty(term.CategoryName)) {
                        id = term.CategoryName + "/" + term.id.Trim();
                    }

                    if (this.termDictionary.ContainsKey(id)) {
                        // Already has the term. Just log an error
#if UNITY_EDITOR
                        Debug.LogError("Term already exists: " + id + " - " + term.translation);
#endif
                    }

                    this.termDictionary[id] = term; // This will replace the old term if it exists
                }
            }
        }

        public Category GetCategoryById(string name) {
            string toFind = name == "Default" ? string.Empty : name;
            for (int i = 0; i < this.catList.Count; i++) {
                if (this.catList[i].name == toFind) {
                    return this.catList[i];
                }
            }

            return null;
        }

        private readonly List<Category> unionCategory = new List<Category>();

        public void CompareTranslation(TranslationContainer newList) {
            this.unionCategory.Clear();
            this.unionCategory.AddRange(newList.catList);

            for (int i = 0; i < this.catList.Count; i++) {
                Category currentCategory = this.catList[i];
                Category otherCategory = this.unionCategory.Find(cat => cat.name == currentCategory.name);

                if (otherCategory != null) {
                    otherCategory.Compare(currentCategory);
                } else {
                    this.unionCategory.Add(currentCategory);
                }
            }

            this.catList.Clear();
            this.catList.AddRange(this.unionCategory);
            SetupLists();
        }
    }
}