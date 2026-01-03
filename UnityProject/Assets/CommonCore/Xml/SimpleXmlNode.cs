using System;
using System.Collections.Generic;
using System.Globalization;

namespace Common.Xml {
    public class SimpleXmlNode {
        private const string YES = "y";
        private const string TRUE = "true";
        private readonly Dictionary<string, string> attributes = new();
        private readonly List<SimpleXmlNode> children = new();
        public SimpleXmlNode? parentNode;

        public string tagName;

        public SimpleXmlNode() {
            this.tagName = "NONE";
        }

        public string? InnerText { get; set; }

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

        public Option<SimpleXmlNode> FindFirstNodeInChildrenAsOption(string tagName) {
            return FindFirstNodeInChildrenAsOption(this, tagName);
        }
        
        private static Option<SimpleXmlNode> FindFirstNodeInChildrenAsOption(SimpleXmlNode node, string tagName) {
            for (int i = 0; i < node.children.Count; ++i) {
                if (node.children[i].tagName.EqualsFast(tagName)) {
                    return Option<SimpleXmlNode>.Some(node.children[i]);
                }
            }

            // not found
            // try to look for it in children
            // It's found or error
            for (int i = 0; i < node.children.Count; ++i) {
                Option<SimpleXmlNode> found = FindFirstNodeInChildrenAsOption(node.children[i], tagName);
                if (found.IsSome) {
                    return found;
                }
            }

            // not found
            return Option<SimpleXmlNode>.NONE;
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
            if (!this.attributes.TryGetValue(attributeKey, out string value)) {
                throw new Exception($"Attribute '{attributeKey}' can't be found.");
            }
            
            return value?.Trim() ?? string.Empty;
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

        public uint GetAttributeAsUint(string attributeKey) {
            return !TryGetAttribute(attributeKey, out string value) 
                ? (uint)0 
                : uint.Parse(value, NumberFormatInfo.InvariantInfo);
        }

        public bool TryGetAttributeAsUint(string attributeKey, out uint value) {
            value = 0;
            if (!TryGetAttribute(attributeKey, out string stringValue)) {
                return false;
            }

            value = uint.Parse(stringValue, NumberFormatInfo.InvariantInfo);
            return true;
        }

        public long GetAttributeAsLong(string attributeKey) {
            if (!ContainsAttribute(attributeKey)) {
                return 0;
            }

            return long.Parse(GetAttribute(attributeKey), NumberFormatInfo.InvariantInfo);
        }
        
        public byte GetAttributeAsByte(string attributeKey) {
            if (!ContainsAttribute(attributeKey)) {
                return 0;
            }

            return byte.Parse(GetAttribute(attributeKey), NumberFormatInfo.InvariantInfo);
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

        public bool TryGetAttribute(string attributeKey, out string value) {
            return this.attributes.TryGetValue(attributeKey, out value);
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