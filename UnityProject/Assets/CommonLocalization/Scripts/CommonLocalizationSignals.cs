using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common.Signal;

namespace Common {
    /// <summary>
    /// Keep signals specifically to CommonLocalization here
    /// </summary>
    public static class CommonLocalizationSignals {

        // This signal is dispatched whenever the language is changed
        public static readonly Signal.Signal TERMS_CHANGED = new Signal.Signal("TermsChanged");

        public static readonly Signal.Signal PARSE_LANGUAGE_XML = new Signal.Signal("ParseLanguageXml");

        public static readonly Signal.Signal PARSE_DEFAULT_LANGUAGE = new Signal.Signal("ParseDefaultLanguage");

    }
}
