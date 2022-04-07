using Unity.Collections;

namespace CommonEcs {
    public static class StringDotsExtensions {
        /// <summary>
        /// We provided this because FixedString conversions can't handle a null string. 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static ValueTypeOption<FixedString64Bytes> AsFixedString64Option(this string? s) {
            return string.IsNullOrWhiteSpace(s) ? ValueTypeOption<FixedString64Bytes>.None :
                ValueTypeOption<FixedString64Bytes>.Some(s);
        }
    }
}