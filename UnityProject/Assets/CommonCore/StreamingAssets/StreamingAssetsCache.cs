namespace Common {
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// Synchronous accessor for StreamingAssets data. On WebGL the files must be
    /// preloaded (see StreamingAssetsPreloader) because StreamingAssets is served
    /// over HTTP and cannot be read synchronously. On other platforms it falls back
    /// to direct File reads, so behavior there is unchanged.
    /// </summary>
    public static class StreamingAssetsCache {
        private static readonly Dictionary<string, byte[]> CACHE = new(8);

        public static void Store(string relativePath, byte[] bytes) {
            string key = Normalize(relativePath);
            CACHE[key] = bytes;
            Debug.Log($"StreamingAssetsCache: Stored data for {key}");
        }

        public static bool Has(string relativePath) {
            return CACHE.ContainsKey(Normalize(relativePath));
        }

        public static byte[] ReadAllBytes(string relativePath) {
            string key = Normalize(relativePath);
            if (CACHE.TryGetValue(key, out byte[] bytes)) {
                return bytes;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.LogError($"StreamingAssets not preloaded (add it to the StreamingAssetsPreloader list): {key}");
            throw new FileNotFoundException(
                $"StreamingAssets not preloaded (add it to the StreamingAssetsPreloader list): {key}");
#else
            return File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, key));
#endif
        }

        public static string ReadAllText(string relativePath) {
            // Use a StreamReader so a byte-order mark is detected and stripped, matching
            // File.ReadAllText / new StreamReader(path). A raw Encoding.UTF8.GetString would
            // leave a leading U+FEFF that breaks XML parsing ("Data at the root level is invalid").
            using MemoryStream stream = new(ReadAllBytes(relativePath));
            using StreamReader reader = new(stream, detectEncodingFromByteOrderMarks: true);
            return reader.ReadToEnd();
        }

        private static string Normalize(string relativePath) {
            return relativePath.Replace('\\', '/').TrimStart('/');
        }
    }
}
