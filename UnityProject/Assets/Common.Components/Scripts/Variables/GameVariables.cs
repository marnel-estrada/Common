using System.IO;

using UnityEngine;

using Common.Xml;
using System.Globalization;

namespace Common {
    public class GameVariables : MonoBehaviour {
        [SerializeField]
        private bool useStreamingSource = true;

        [Tooltip("Relative to StreamingAssets")]
        [SerializeField]
        private string gameVarXmlPath;

        [SerializeField]
        private TextAsset nonStreamingSource;

        [SerializeField]
        private GameVariableOverrideResolver overrideResolver; // May be null

        private GameVariableSet defaultVariables;

        private void Awake() {
            Assertion.AssertNotEmpty(this.gameVarXmlPath);

            if (!this.useStreamingSource) {
                Assertion.AssertNotNull(this.nonStreamingSource);
            }

            // populate varMap
            if (!HasVariables()) {
                // we check here because variables might have already been parsed when Get() was invoked
                PopulateVariables();
            }
        }

        private void PopulateVariables() {
            if (this.defaultVariables == null) {
                this.defaultVariables = new GameVariableSet();
            }

            Parse();
        }

        private const string ENTRY = "Entry";

        // attributes
        private const string KEY = "key";
        private const string VALUE = "value";

        private void Parse() {
            string xmlText = null;

            if (this.useStreamingSource) {
                string xmlPath = Application.streamingAssetsPath + "/" + gameVarXmlPath;
                xmlText = File.ReadAllText(xmlPath);
            } else {
                // Not using the streaming source (the secret one)
                xmlText = this.nonStreamingSource.text;
            }

            SimpleXmlReader reader = new SimpleXmlReader();
            SimpleXmlNode root = reader.Read(xmlText).FindFirstNodeInChildren("GameVariables");

            string overrideToUse = root.GetAttribute("useOverride");

            // Resolved override has higher precedence
            string resolvedOverride = ResolveOverride();
            if (!string.IsNullOrEmpty(resolvedOverride)) {
                overrideToUse = resolvedOverride;
            }

            SimpleXmlNode overrideNode = null;

            for (int i = 0; i < root.Children.Count; ++i) {
                SimpleXmlNode child = root.Children[i];
                if (ENTRY.EqualsFast(child.TagName)) {
                    // parse key-value pair
                    string key = child.GetAttribute(KEY);
                    string value = child.GetAttribute(VALUE);
                    this.defaultVariables.Add(key, value);
                }

                if ("Override".EqualsFast(child.TagName) && child.GetAttribute("id").EqualsFast(overrideToUse)) {
                    overrideNode = child;
                }
            }

            if (!string.IsNullOrEmpty(overrideToUse) && overrideNode == null) {
                // This means that an override was specified but no node for it was found
                Assertion.Assert(false, "Can't find node for override " + overrideToUse);
            }

            // After parsing the default entries, we load the override if it was specified
            if (overrideNode != null) {
                ParseOverride(overrideNode);
            }
        }

        private string ResolveOverride() {
            if(this.overrideResolver != null) {
                return this.overrideResolver.ResolveOverride();
            }

            return null;
        }

        private void ParseOverride(SimpleXmlNode node) {
            for (int i = 0; i < node.Children.Count; ++i) {
                SimpleXmlNode child = node.Children[i];
                if (ENTRY.Equals(child.TagName)) {
                    // parse key-value pair
                    string key = child.GetAttribute(KEY);
                    string value = child.GetAttribute(VALUE);

                    Assertion.Assert(this.defaultVariables.Contains(key), key, this.gameObject);
                    this.defaultVariables.Set(key, value);
                }
            }
        }

        private bool HasVariables() {
            return this.defaultVariables != null && this.defaultVariables.Count > 0;
        }

        /**
		 * Resolves a string game variable 
		 */
        public string Get(string key) {
            if (!HasVariables()) {
                PopulateVariables();
            }

            Assertion.Assert(this.defaultVariables.Contains(key), key);
            return this.defaultVariables.Get(key);
        }

        /**
		 * Resolves an integer game variable
		 */
        public int GetInt(string key) {
            return int.Parse(Get(key), NumberFormatInfo.InvariantInfo);
        }

        /**
		 * Resolves a float game variable
		 */
        public float GetFloat(string key) {
            return float.Parse(Get(key), NumberFormatInfo.InvariantInfo);
        }

        private const string TRUE = "true";
        private const string FALSE = "false";

        /**
		 * Resolves a bool game variable
		 */
        public bool GetBool(string key) {
            string rawBool = Get(key);

            if (TRUE.Equals(rawBool)) {
                return true;
            } else if (FALSE.Equals(rawBool)) {
                return false;
            }

            Assertion.Assert(false, "Can't resolve boolean value: " + rawBool);
            return false;
        }
    }
}
