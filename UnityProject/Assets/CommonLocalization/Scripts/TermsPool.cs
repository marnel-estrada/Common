using System;
using System.IO;
using UnityEngine;
using Common.Signal;

namespace Common {
    /// <summary>
    /// Parses terms XML and keeps a dictionary of terms in memory
    /// </summary>
    public class TermsPool : MonoBehaviour {
        [SerializeField]
        private string xmlPath = "BaseGame/Data/EnglishMaster.xml"; // Relative to StreamingAssets

        private TranslationContainer? currentTranslation;

        private Action<string>? parseMatcher;

        private void Awake() {
            Assertion.NotEmpty(this.xmlPath);

            this.parseMatcher = Parse;

            // Parse master language on awake
            ParseDefaultLanguage();

            CommonLocalizationSignals.PARSE_LANGUAGE_XML.AddListener(Parse);
            CommonLocalizationSignals.PARSE_DEFAULT_LANGUAGE.AddListener(ParseDefaultLanguage);
        }

        private void OnDestroy() {
            CommonLocalizationSignals.PARSE_LANGUAGE_XML.RemoveListener(Parse);
            CommonLocalizationSignals.PARSE_DEFAULT_LANGUAGE.RemoveListener(ParseDefaultLanguage);
        }

        private void ParseDefaultLanguage(ISignalParameters parameters) {
            ParseDefaultLanguage();
        }

        private void ParseDefaultLanguage() {
            Assertion.NotEmpty(this.xmlPath);
            string masterFullPath = Path.Combine(Application.streamingAssetsPath, this.xmlPath);
            Parse(masterFullPath);
        }

        private void Parse(ISignalParameters parameters) {
            Option<string> parameterPath = parameters.GetParameter<string>(CommonLocalizationParams.XML_PATH);

            if (this.parseMatcher == null) {
                return;
            }

            parameterPath.Match(this.parseMatcher);
        }

        /// <summary>
        /// Parses a language XML
        /// xmlPath here is already the full path
        /// </summary>
        /// <param name="xmlPath"></param>
        private void Parse(string xmlPath) {
            string path = xmlPath;

            Debug.Log("Parsed " + path);

            if (this.currentTranslation == null) {
                this.currentTranslation = TranslationContainer.Load(path);
            } else {
                this.currentTranslation.CompareTranslation(TranslationContainer.Load(path));
            }

            Assertion.NotNull(this.currentTranslation);

            // Dispatch this signal after parsing to change the text    
            CommonLocalizationSignals.TERMS_CHANGED.Dispatch();
        }

        /// <summary>
        /// Returns the translation of the specified term id
        /// </summary>
        /// <param name="termId"></param>
        /// <returns></returns>
        public Option<string> GetTranslation(string termId) {
            Assertion.NotNull(this.currentTranslation);
            if (this.currentTranslation == null) {
                return Option<string>.NONE;
            }

            Option<Term> currentTerm = this.currentTranslation.GetTerm(termId.Trim());
            return currentTerm.MatchExplicit<GetTranslationMatcher, Option<string>>(new GetTranslationMatcher());
        }

        private readonly struct GetTranslationMatcher : IFuncOptionMatcher<Term, Option<string>> {
            public Option<string> OnSome(Term term) {
                return Option<string>.Some(term.translation);
            }

            public Option<string> OnNone() {
                return Option<string>.NONE;
            }
        }

        /// <summary>
        /// Returns the category with the specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Category? GetCategory(string id) {
            return this.currentTranslation?.GetCategoryById(id);
        }

        private static TermsPool? INSTANCE;

        public static TermsPool Instance {
            get {
                if (INSTANCE != null) {
                    return INSTANCE;
                }

                // Look for existing first
                TermsPool[] foundPools = FindObjectsOfType<TermsPool>();
                Assertion.IsTrue(foundPools.Length <= 1); // Should not be more than 2

                if (foundPools.Length > 0) {
                    INSTANCE = foundPools[0]; // Get only the first one
                } else {
                    // There's no instance ever
                    // Make a new one
                    GameObject go = new GameObject("TermsPool");
                    go.AddComponent<DontDestroyOnLoadComponent>();
                    INSTANCE = go.AddComponent<TermsPool>();
                }

                return INSTANCE;
            }
        }

        /// <summary>
        /// Returns the translation of the term ID
        /// </summary>
        /// <param name="termId"></param>
        /// <returns></returns>
        public static Option<string> GetTermTranslation(string termId) {
            return Instance.GetTranslation(termId);
        }
    }
}