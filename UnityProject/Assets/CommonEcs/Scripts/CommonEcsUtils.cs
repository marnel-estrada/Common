using Unity.Collections;

namespace CommonEcs {
    public static class CommonEcsUtils {
        /// <summary>
        /// Common way to convert a string id into an integer id.
        /// </summary>
        /// <param name="stringId"></param>
        /// <returns></returns>
        public static int ToIntId(string stringId) {
            return new FixedString64(stringId).GetHashCode();
        }
    }
}