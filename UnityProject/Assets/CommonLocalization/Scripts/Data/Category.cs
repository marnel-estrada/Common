using System.Collections.Generic;
using System.Xml.Serialization;
using System;

namespace Common {
    [Serializable]
    public class Category {
        [XmlElement("term")]
        public List<Term> termList;

        [XmlAttribute("name")]
        public string name;

        [XmlIgnore()]
        public int lineCount = 0;

        [XmlIgnore()]
        public int todoCount = 0;

        private readonly Dictionary<string, Term> termMap = new Dictionary<string, Term>(20);

        public Category() {
            this.termList = new List<Term>();
        }

        public Category(string newName) {
            if (newName.EqualsFast("Default")) {
                this.name = string.Empty;
            } else {
                this.name = newName;
            }

            this.termList = new List<Term>();
        }

        public void DeleteTerm(Term termToDelete) {
            this.termList.Remove(termToDelete);
        }

        public Option<Term> GetTerm(string id) {
            Option<Term> term = this.termMap.Find(id);
            if (term.IsSome) {
                return term;
            }

            // Resolve term from list
            for (int i = 0; i < this.termList.Count; i++) {
                Term termFromList = this.termList[i];
                if (termFromList.id.EqualsFast(id)) {
                    // Store for easier retrieval later
                    this.termMap[id] = termFromList;

                    return Option<Term>.Some(termFromList);
                }
            }

            // Can't be found
            return Option<Term>.NONE;
        }

        private readonly List<Term> unionTerms = new List<Term>();

        public void Compare(Category other) {
            if (this.name != other.name) {
                return;
            }

            this.unionTerms.Clear();
            this.unionTerms.AddRange(other.termList);

            for (int i = 0; i < this.termList.Count; i++) {
                Term currentTerm = this.termList[i];
                Term otherTerm = this.unionTerms.Find(term => term.id == currentTerm.id);

                if (otherTerm != null) {
                    if (string.IsNullOrEmpty(currentTerm.translation)) {
                        // continue iterating through the list if loaded translation term is empty
                        // this will use the masterlist's translation
                        continue;
                    } 
                    
                    otherTerm.translation = currentTerm.translation;
                } else {
                    this.unionTerms.Add(currentTerm);
                }
            }

            this.termList.Clear();
            this.termList.AddRange(this.unionTerms);
        }

        public int TermCount {
            get {
                return this.termList.Count;
            }
        }
    }
}