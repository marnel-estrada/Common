﻿using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Common {
    /// <summary>
    /// Contains text related utility methods
    /// </summary>
    public static class TextUtils {
        /// <summary>
        /// Returns comma separated number on the thousands
        /// </summary>
        /// <param name="intValue"></param>
        /// <returns></returns>
        public static string AsCommaSeparated(int intValue) {
            return $"{intValue:n0}";
        }

        /// <summary>
        /// Returns comma separated number on the thousands
        /// </summary>
        /// <param name="uintValue"></param>
        /// <returns></returns>
        public static string AsCommaSeparated(uint uintValue) {
            return $"{uintValue:n0}";
        }

        /// <summary>
        /// Returns comma separated number on the thousands with 2 decimal places
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AsCommaSeparated(float value) {
            return $"{value:n2}";
        }

        /// <summary>
        /// Generates a signed text to the specified integer value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AsSignedText(int value) {
            return value >= 0 ? ("+" + AsCommaSeparated(value)) : AsCommaSeparated(value); // note that negative values already have negative sign
        }
        
        public static string AsSignedText(float value) {
            return value >= 0 ? ("+" + AsCommaSeparated(value)) : AsCommaSeparated(value); // note that negative values already have negative sign
        }

        /// <summary>
        /// Proper trimming of a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Trim(string str) {
            return str?.Trim() ?? string.Empty;
        }

        private static readonly StringBuilder BUILDER = new StringBuilder();

        /// <summary>
        /// Returns another version of the text that changed the first letter to upper case
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string UpperCaseFirstLetter(string text) {
            BUILDER.Remove(0, BUILDER.Length);
            BUILDER.Append(text.Substring(0, 1).ToUpperInvariant());
            BUILDER.Append(text.Substring(1));

            return BUILDER.ToString();
        }

        /// <summary>
        /// Formats the specified float value to a percent text
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToPercentText(float value) {
            float raisedToHundreds = value * 100;
            return $"{raisedToHundreds:0}" + "%";
        }

        /// <summary>
        /// Formats the specified float to a signed percent text
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToSignedPercentText(float value) {
            if(value < 0) {
                // note here that negative values already have their negative sign
                return ToPercentText(value);
            }

            return "+" + ToPercentText(value);
        }

		/// <summary>
		/// Formats the specified int from 1,000 to 1K
		/// </summary>
		/// <returns></returns>
		/// <param name="value"></param>
		public static string AsKiloText(int value) {
			if(value >= 1000) {
				return (value / 1000).ToString("0.#") + "K";
			}
			return value.ToString("#,0");
		}

		public static string AsKiloSignedText(int value) {
			if(value < 0) {
				// note here that negative values already have their negative sign
				return AsKiloText(value);
			}

			return "+" + AsKiloText(value);
		}

        /// <summary>
        /// Composes a single string made up from the specified string separated by the specified separator
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string ComposeSeparatedString(string[] strings, char separator) {
            BUILDER.Remove(0, BUILDER.Length); // Clear

            for(int i = 0; i < strings.Length; ++i) {
                if(string.IsNullOrEmpty(strings[i])) {
                    continue;
                }

                BUILDER.Append(strings[i]);

                // Append separator only if not yet the last string
                if(i + 1 < strings.Length) {
                    BUILDER.Append(separator);
                }
            }

            return BUILDER.ToString();
        }

        /// <summary>
        /// Formats the date using the formatting of the machine being used
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string FormatDateToCurrentCulture(DateTime date) {
            return date.ToString("MM/dd/yyyy hh:mm tt", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Cleans the specified string of unsafe characters for file names
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string RemoveFileNameUnsafeCharacters(string s) {
            string unsafeCharacters = Regex.Escape("<>:\"\\/|?*");
            string pattern = "[" + unsafeCharacters + "]";

            // Replace with empty string
            return Regex.Replace(s, pattern, "");
        }

        /// <summary>
        /// Returns the text in a dollar format. The output is already comma-separated.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AsDollarText(int value) {
            return value < 0 ? $"-${AsCommaSeparated(-value)}" : $"${AsCommaSeparated(value)}";
        }

        /// <summary>
        /// Returns the comma-separated, dollar format which is signed before the dollar sign.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AsSignedDollarText(int value) {
            return value >= 0 ? ("+" + AsDollarText(value)) : AsDollarText(value); // note that negative values already have negative sign
        }
    }
}
