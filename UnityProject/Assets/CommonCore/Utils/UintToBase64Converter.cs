using System;
using System.Collections.Generic;

namespace Common {
    /// <summary>
    /// The same concept as IntToBase64Converter but for uint.
    /// </summary>
    public class UintToBase64Converter {
        private readonly List<byte> bytes;
        
        public UintToBase64Converter() : this(32) {
        }

        public UintToBase64Converter(int initialCapacity) {
            this.bytes = new List<byte>(initialCapacity * 4);
        }

        public void Clear() {
            this.bytes.Clear();
        }

        public void Add(uint value) {
            Span<byte> valueBytes = stackalloc byte[4];

            if (!BitConverter.TryWriteBytes(valueBytes, value)) {
                return;
            }

            for (int i = 0; i < valueBytes.Length; i++) {
                this.bytes.Add(valueBytes[i]);
            }
        }
        
        public string Base64 => Convert.ToBase64String(this.bytes.ToArray());

        private const int UINT_SIZE = sizeof(int);

        public static void LoadValues(string base64String, List<uint> resultList) {
            resultList.Clear();
            
            byte[] bytes = Convert.FromBase64String(base64String);
            int integerCount = bytes.Length / UINT_SIZE;

            int byteArrayIndex = 0; // Index to bytes
            for (int i = 0; i < integerCount; ++i) {
                uint value = BitConverter.ToUInt32(bytes, byteArrayIndex);
                resultList.Add(value);
                
                byteArrayIndex += UINT_SIZE; // Jump to next integer
            }
        }
    }
}