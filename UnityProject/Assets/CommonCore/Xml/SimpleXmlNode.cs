using System.Collections.Generic;
using System.Globalization;

namespace Common.Xml {
    public class SimpleXmlNode {
        public delegate void NodeProcessor(SimpleXmlNode node);

        private const string YES = "yes";
        private const string TRUE = "true";
        public Dictionary<string, string> Attributes;
        public List<SimpleXmlNode> Children;
        public SimpleXmlNode ParentNode;

        public string TagName;

        public SimpleXmlNode() {
            this.TagName = "NONE";
            this.ParentNode = null;
            this.Children = new List<SimpleXmlNode>();
            this.Attributes = new Dictionary<string, string>();
        }

        public string InnerText { get; set; }

        /**
         * Finds the first node along the node tree with the same name as the specified one.
         */
        public SimpleXmlNode FindFirstNodeInChildren(string tagName) {
            return FindFirstNodeInChildren(this, tagName);
        }

        private SimpleXmlNode FindFirstNodeInChildren(SimpleXmlNode node, string tagName) {
            for (int i = 0; i < node.Children.Count; ++i) {
                if (node.Children[i].TagName.EqualsFast(tagName)) {
                    return node.Children[i];
                }
            }

            // not found
            // try to look for it in children
            for (int i = 0; i < node.Children.Count; ++i) {
                SimpleXmlNode found = FindFirstNodeInChildren(node.Children[i], tagName);
                if (found != null) {
                    return found;
                }
            }

            // not found
            return null;
        }

        /**
         * Returns whether or not the node contains the specified attribute.
         */
        public bool HasAttribute(string attributeKey) {
            return this.Attributes.ContainsKey(attributeKey);
        }

        /**
         * Returns the attribute value with the specified key.
         */
        public string GetAttribute(string attributeKey) {
            return this.Attributes[attributeKey].Trim();
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
            return this.Attributes.ContainsKey(attributeKey);
        }

        /**
         * Traverses the children nodes of this node.
         */
        public void TraverseChildren(NodeProcessor visitor) {
            int count = this.Children.Count;
            for (int i = 0; i < count; ++i) {
                visitor(this.Children[i]);
            }
        }
    }
}