using System;
using System.Collections.Generic;
using System.Globalization;

#nullable disable

namespace Common.Xml {
    public class SimpleXmlNode {
        public delegate void NodeProcessor(SimpleXmlNode node);

        private const string YES = "yes";
        private const string TRUE = "true";
        private readonly Dictionary<string, string> attributes;
        private readonly List<SimpleXmlNode> children;
        public SimpleXmlNode parentNode;

        public string tagName;

        public SimpleXmlNode() {
            this.tagName = "NONE";
            this.parentNode = null;
            this.children = new List<SimpleXmlNode>();
            this.attributes = new Dictionary<string, string>();
        }

        public string InnerText { get; set; }

        /**
         * Finds the first node along the node tree with the same name as the specified one.
         */
        public SimpleXmlNode FindFirstNodeInChildren(string tagName) {
            return FindFirstNodeInChildren(this, tagName);
        }

        private static SimpleXmlNode FindFirstNodeInChildren(SimpleXmlNode node, string tagName) {
            for (int i = 0; i < node.children.Count; ++i) {
                if (node.children[i].tagName.EqualsFast(tagName)) {
                    return node.children[i];
                }
            }

            // not found
            // try to look for it in children
            // It's found or error
            for (int i = 0; i < node.children.Count; ++i) {
                SimpleXmlNode found = FindFirstNodeInChildren(node.children[i], tagName);
                return found;
            }

            // not found
            throw new Exception($"Can't find any node named \"{tagName}\".");
        }

        /**
         * Returns whether or not the node contains the specified attribute.
         */
        public bool HasAttribute(string attributeKey) {
            return this.attributes.ContainsKey(attributeKey);
        }

        /**
         * Returns the attribute value with the specified key.
         */
        public string GetAttribute(string attributeKey) {
            return this.attributes[attributeKey].Trim();
        }

        public Option<string> GetAttributeAsOption(string attributeKey) {
            return this.attributes.TryGetValue(attributeKey, out string value) ? Option<string>.AsOption(value.Trim()) 
                : Option<string>.NONE;
        }

        /**
         * Returns an attribute as an int.
         */
        public int GetAttributeAsInt(string attributeKey) {
            if (!ContainsAttribute(attributeKey)) {
                return 0;
            }

            return int.Parse(GetAttribute(attributeKey), NumberFormatInfo.InvariantInfo);
        }

        /**
         * Returns an attribute as a float.
         */
        public float GetAttributeAsFloat(string attributeKey) {
            if (!ContainsAttribute(attributeKey)) {
                return 0;
            }

            return float.Parse(GetAttribute(attributeKey), NumberFormatInfo.InvariantInfo);
        }

        /**
         * Retrieves an attribute as a boolean value.
         */
        public bool GetAttributeAsBool(string attributeKey) {
            if (!ContainsAttribute(attributeKey)) {
                return false;
            }

            string attributeValue = GetAttribute(attributeKey);

            if (!string.IsNullOrEmpty(attributeValue)) {
                // We do this because bool.ToString() uses "True" (capitalized first letter)
                attributeValue = attributeValue.ToLower();
            }

            return TRUE.EqualsFast(attributeValue) || YES.EqualsFast(attributeValue);
        }

        /**
         * Returns whether or not the node contains the specified attribute.
         */
        public bool ContainsAttribute(string attributeKey) {
            return this.attributes.ContainsKey(attributeKey);
        }

        public void AddChild(SimpleXmlNode child) {
            this.children.Add(child);
        }

        public void AddAttribute(string name, string value) {
            this.attributes[name] = value;
        }
        
        public IReadOnlyList<SimpleXmlNode> Children {
            get {
                return this.children;
            }
        }

        public IReadOnlyDictionary<string, string> Attributes {
            get {
                return this.attributes;
            }
        }
    }
}