namespace Common {
    using System;

    /// <summary>
    /// Contains utility methods about the environment the game is running on
    /// </summary>
    public static class EnvironmentUtils {
        
        public static bool Is64Bit {
            get {
                return IntPtr.Size == 8;
            }
        }
        
        public static bool Is32Bit {
            get {
                return IntPtr.Size == 4;
            }
        }
        
    }
}
