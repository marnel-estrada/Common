using System;
using System.Collections.Generic;

namespace Common {
    /// <summary>
    /// Utility class to convert a collection of integers into base 64 and vice versa
    /// </summary>
    public class IntToBase64Converter {
        private readonly List<byte> bytes = new List<byte>(20);

        public void Clear() {
            this.bytes.Clear();
        }

        public void Add(int value) {
            this.bytes.AddRange(BitConverter.GetBytes(value));
        }

        public string Base64 {
            get {
                return Convert.ToBase64String(this.bytes.ToArray());
            }
        }
        
        private const int INTEGER_SIZE = sizeof(int);

        public static void LoadValues(string base64String, List<int> container) {
            container.Clear();

            byte[] bytes = Convert.FromBase64String(base64String);
            int integerCount = bytes.Length / INTEGER_SIZE;

            int byteArrayIndex = 0; // Index to bytes
            for (int i = 0; i < integerCount; ++i) {
                int value = BitConverter.ToInt32(bytes, byteArrayIndex);
                container.Add(value);
                
                byteArrayIndex += INTEGER_SIZE; // Jump to next integer
            }
        }
    }
}