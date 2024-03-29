﻿using System.IO;

using UnityEngine;

using Common.Xml;
using System.Globalization;

namespace Common {
    public class GameVariables : MonoBehaviour {
        [SerializeField]
        private bool useStreamingSource = true;

        [Tooltip("Relative to StreamingAssets")]
        [SerializeField]
        private string? gameVarXmlPath;

        [SerializeField]
        private TextAsset? nonStreamingSource;

        [SerializeField]
        private GameVariableOverrideResolver? overrideResolver; // May be null

        private readonly GameVariableSet defaultVariables = new GameVariableSet();

        private void Awake() {
            Assertion.NotEmpty(this.gameVarXmlPath);

            if (!this.useStreamingSource) {
                Assertion.NotNull(this.nonStreamingSource);
            }

            // populate varMap
            if (!HasVariables()) {
                // we check here because variables might have already been parsed when Get() was invoked
                PopulateVariables();
            }
        }

        private void PopulateVariables() {
            Parse();
        }

        private const string ENTRY = "Entry";

        // attributes
        private const string KEY = "key";
        private const string VALUE = "value";

        private void Parse() {
            string xmlText;

            if (this.useStreamingSource) {
                string xmlPath = Application.streamingAssetsPath + "/" + this.gameVarXmlPath;
                xmlText = File.ReadAllText(xmlPath);
            } else {
                // Not using the streaming source (the secret one)
                if (this.nonStreamingSource == null) {
                    throw new CantBeNullException(nameof(this.nonStreamingSource));
                }
                
                xmlText = this.nonStreamingSource.text;
            }

            SimpleXmlNode root = SimpleXmlReader.Read(xmlText).FindFirstNodeInChildren("GameVariables");

            // Use custom override if it exists
            // Otherwise, use the override from XML
            Option<string> resolvedOverride = ResolveOverride();
            resolvedOverride.Match(new ParseOverrideMatcher(this, root));
        }

        private readonly struct ParseOverrideMatcher : IOptionMatcher<string> {
            private readonly GameVariables gameVariables;
            private readonly SimpleXmlNode xmlRoot;

            public ParseOverrideMatcher(GameVariables gameVariables, SimpleXmlNode xmlRoot) {
                this.gameVariables = gameVariables;
                this.xmlRoot = xmlRoot;
            }

            public void OnSome(string overrideId) {
                this.gameVariables.ParseOverride(this.xmlRoot, overrideId);
            }

            public void OnNone() {
                string xmlOverride = this.xmlRoot.GetAttribute("useOverride");
                this.gameVariables.ParseOverride(this.xmlRoot, xmlOverride);
            }
        } 

        private void ParseOverride(SimpleXmlNode root, string overrideToUse) {
            SimpleXmlNode? overrideNode = null;

            for (int i = 0; i < root.Children.Count; ++i) {
                SimpleXmlNode child = root.Children[i];
                if (ENTRY.EqualsFast(child.tagName)) {
                    // parse key-value pair
                    string key = child.GetAttribute(KEY);
                    string value = child.GetAttribute(VALUE);
                    this.defaultVariables.Add(key, value);
                }

                if ("Override".EqualsFast(child.tagName) && child.GetAttribute("id").EqualsFast(overrideToUse)) {
                    overrideNode = child;
                }
            }

            if (!string.IsNullOrEmpty(overrideToUse) && overrideNode == null) {
                // This means that an override was specified but no node for it was found
                Assertion.IsTrue(false, "Can't find node for override " + overrideToUse);
            }

            // After parsing the default entries, we load the override if it was specified
            if (overrideNode != null) {
                ParseOverride(overrideNode);
            }
        }

        private Option<string> ResolveOverride() {
            if(this.overrideResolver != null) {
                return Option<string>.Some(this.overrideResolver.ResolveOverride());
            }

            return Option<string>.NONE;
        }

        private void ParseOverride(SimpleXmlNode node) {
            for (int i = 0; i < node.Children.Count; ++i) {
                SimpleXmlNode child = node.Children[i];
                if (ENTRY.Equals(child.tagName)) {
                    // parse key-value pair
                    string key = child.GetAttribute(KEY);
                    string value = child.GetAttribute(VALUE);

                    Assertion.IsTrue(this.defaultVariables.Contains(key), key, this.gameObject);
                    this.defaultVariables.Set(key, value);
                }
            }
        }

        private bool HasVariables() {
            return this.defaultVariables.Count > 0;
        }

        /**
		 * Resolves a string game variable 
		 */
        public string Get(string key) {
            if (!HasVariables()) {
                PopulateVariables();
            }

            Assertion.IsTrue(this.defaultVariables.Contains(key), key);
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
            } 
            
            if (FALSE.Equals(rawBool)) {
                return false;
            }

            Assertion.IsTrue(false, "Can't resolve boolean value: " + rawBool);
            return false;
        }
    }
}
