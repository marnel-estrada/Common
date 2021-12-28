////////////////////////////////////////////////////////////////////////////////
// Warning: This file was automatically generated by SmallBufferGenerator.
//          If you edit this by hand, the next run of SmallBufferGenerator
//          will overwrite your edits.
////////////////////////////////////////////////////////////////////////////////

using System;

namespace CommonEcs {
    [System.Runtime.InteropServices.StructLayout(
        System.Runtime.InteropServices.LayoutKind.Sequential)]
    public unsafe struct LinearHashMapBucket16<K, V>
        where K : unmanaged, IEquatable<K>
        where V : unmanaged {
        public ref struct Enumerator {
            private readonly LinearHashMapEntry<K, V>* m_Elements;

            private int m_Index;

            public Enumerator(LinearHashMapEntry<K, V>* elements) {
                m_Elements = elements;
                m_Index = -1;
            }

            public bool MoveNext() {
                m_Index++;
                return m_Index < 16;
            }

            public ref LinearHashMapEntry<K, V> Current {
                get {
                    RequireIndexInBounds();
                    return ref m_Elements[m_Index];
                }
            }

            [Unity.Burst.BurstDiscard]
            public void RequireIndexInBounds() {
                if (m_Index < 0 || m_Index >= 16) {
                    // ReSharper disable once UseStringInterpolation (due to Burst)
                    throw new System.Exception(
                        string.Format("Index out of bounds: {0}", m_Index));
                }
            }
        }

        private readonly LinearHashMapEntry<K, V> m_Element0;

        private readonly LinearHashMapEntry<K, V> m_Element1;

        private readonly LinearHashMapEntry<K, V> m_Element2;

        private readonly LinearHashMapEntry<K, V> m_Element3;

        private readonly LinearHashMapEntry<K, V> m_Element4;

        private readonly LinearHashMapEntry<K, V> m_Element5;

        private readonly LinearHashMapEntry<K, V> m_Element6;

        private readonly LinearHashMapEntry<K, V> m_Element7;

        private readonly LinearHashMapEntry<K, V> m_Element8;

        private readonly LinearHashMapEntry<K, V> m_Element9;

        private readonly LinearHashMapEntry<K, V> m_Element10;

        private readonly LinearHashMapEntry<K, V> m_Element11;

        private readonly LinearHashMapEntry<K, V> m_Element12;

        private readonly LinearHashMapEntry<K, V> m_Element13;

        private readonly LinearHashMapEntry<K, V> m_Element14;

        private readonly LinearHashMapEntry<K, V> m_Element15;

        public ref LinearHashMapEntry<K, V> this[int index] {
            get {
                RequireIndexInBounds(index);
                return ref GetElement(index);
            }
        }

        private ref LinearHashMapEntry<K, V> GetElement(int index) {
            fixed (LinearHashMapEntry<K, V>* elements = &m_Element0) {
                return ref elements[index];
            }
        }

        private void SetElement(int index, LinearHashMapEntry<K, V> value) {
            fixed (LinearHashMapEntry<K, V>* elements = &m_Element0) {
                elements[index] = value;
            }
        }

        public const int Length = 16;

        public Enumerator GetEnumerator() {
            // Safe because Enumerator is a 'ref struct'
            fixed (LinearHashMapEntry<K, V>* elements = &m_Element0) {
                return new Enumerator(elements);
            }
        }

        private static void RequireIndexInBounds(int index) {
            if (index < 0 || index >= 16) {
                // ReSharper disable once UseStringInterpolation (due to Burst)
                throw new System.Exception(
                    string.Format("Index out of bounds: {0}", index));
            }
        }
    }
}