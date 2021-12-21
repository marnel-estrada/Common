using Common;
using Unity.Collections;

namespace CommonEcs {
    public static class CommonEcsUtils {
        /// <summary>
        /// Common way to convert a string id into an integer id.
        /// </summary>
        /// <param name="stringId"></param>
        /// <returns></returns>
        public static int ToIntId(string stringId) {
            Assertion.NotEmpty(stringId);
            return new FixedString64(stringId.Trim()).AsIntId();
        }

        public static int AsIntId(this string self) {
            return ToIntId(self);
        }

        public static int AsIntId(this FixedString64 self) {
            return self.GetHashCode();
        }

        public static int AsIntId(this FixedString32 self) {
            return self.GetHashCode();
        }
    }
}