////////////////////////////////////////////////////////////////////////////////
// Warning: This file was automatically generated by SmallBufferGenerator.
//          If you edit this by hand, the next run of SmallBufferGenerator
//          will overwrite your edits.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace CommonEcs {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LinearHashMapBucket<K, V> 
        where K : unmanaged, IEquatable<K> 
        where V : unmanaged {
        public ref struct Enumerator {
            private readonly LinearHashMapEntry<K, V>* m_Elements;
        
            private int m_Index;
        
            public Enumerator(LinearHashMapEntry<K, V>* elements) {
                this.m_Elements = elements;
                this.m_Index = -1;
            }
        
            public bool MoveNext() {
                this.m_Index++;
        
                return this.m_Index < 128;
            }
        
            public ref LinearHashMapEntry<K, V> Current {
                get {
                    return ref this.m_Elements[this.m_Index];
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

        private readonly LinearHashMapEntry<K, V> m_Element16;

        private readonly LinearHashMapEntry<K, V> m_Element17;

        private readonly LinearHashMapEntry<K, V> m_Element18;

        private readonly LinearHashMapEntry<K, V> m_Element19;

        private readonly LinearHashMapEntry<K, V> m_Element20;

        private readonly LinearHashMapEntry<K, V> m_Element21;

        private readonly LinearHashMapEntry<K, V> m_Element22;

        private readonly LinearHashMapEntry<K, V> m_Element23;

        private readonly LinearHashMapEntry<K, V> m_Element24;

        private readonly LinearHashMapEntry<K, V> m_Element25;

        private readonly LinearHashMapEntry<K, V> m_Element26;

        private readonly LinearHashMapEntry<K, V> m_Element27;

        private readonly LinearHashMapEntry<K, V> m_Element28;

        private readonly LinearHashMapEntry<K, V> m_Element29;

        private readonly LinearHashMapEntry<K, V> m_Element30;

        private readonly LinearHashMapEntry<K, V> m_Element31;

        private readonly LinearHashMapEntry<K, V> m_Element32;

        private readonly LinearHashMapEntry<K, V> m_Element33;

        private readonly LinearHashMapEntry<K, V> m_Element34;

        private readonly LinearHashMapEntry<K, V> m_Element35;

        private readonly LinearHashMapEntry<K, V> m_Element36;

        private readonly LinearHashMapEntry<K, V> m_Element37;

        private readonly LinearHashMapEntry<K, V> m_Element38;

        private readonly LinearHashMapEntry<K, V> m_Element39;

        private readonly LinearHashMapEntry<K, V> m_Element40;

        private readonly LinearHashMapEntry<K, V> m_Element41;

        private readonly LinearHashMapEntry<K, V> m_Element42;

        private readonly LinearHashMapEntry<K, V> m_Element43;

        private readonly LinearHashMapEntry<K, V> m_Element44;

        private readonly LinearHashMapEntry<K, V> m_Element45;

        private readonly LinearHashMapEntry<K, V> m_Element46;

        private readonly LinearHashMapEntry<K, V> m_Element47;

        private readonly LinearHashMapEntry<K, V> m_Element48;

        private readonly LinearHashMapEntry<K, V> m_Element49;

        private readonly LinearHashMapEntry<K, V> m_Element50;

        private readonly LinearHashMapEntry<K, V> m_Element51;

        private readonly LinearHashMapEntry<K, V> m_Element52;

        private readonly LinearHashMapEntry<K, V> m_Element53;

        private readonly LinearHashMapEntry<K, V> m_Element54;

        private readonly LinearHashMapEntry<K, V> m_Element55;

        private readonly LinearHashMapEntry<K, V> m_Element56;

        private readonly LinearHashMapEntry<K, V> m_Element57;

        private readonly LinearHashMapEntry<K, V> m_Element58;

        private readonly LinearHashMapEntry<K, V> m_Element59;

        private readonly LinearHashMapEntry<K, V> m_Element60;

        private readonly LinearHashMapEntry<K, V> m_Element61;

        private readonly LinearHashMapEntry<K, V> m_Element62;

        private readonly LinearHashMapEntry<K, V> m_Element63;

        private readonly LinearHashMapEntry<K, V> m_Element64;

        private readonly LinearHashMapEntry<K, V> m_Element65;

        private readonly LinearHashMapEntry<K, V> m_Element66;

        private readonly LinearHashMapEntry<K, V> m_Element67;

        private readonly LinearHashMapEntry<K, V> m_Element68;

        private readonly LinearHashMapEntry<K, V> m_Element69;

        private readonly LinearHashMapEntry<K, V> m_Element70;

        private readonly LinearHashMapEntry<K, V> m_Element71;

        private readonly LinearHashMapEntry<K, V> m_Element72;

        private readonly LinearHashMapEntry<K, V> m_Element73;

        private readonly LinearHashMapEntry<K, V> m_Element74;

        private readonly LinearHashMapEntry<K, V> m_Element75;

        private readonly LinearHashMapEntry<K, V> m_Element76;

        private readonly LinearHashMapEntry<K, V> m_Element77;

        private readonly LinearHashMapEntry<K, V> m_Element78;

        private readonly LinearHashMapEntry<K, V> m_Element79;

        private readonly LinearHashMapEntry<K, V> m_Element80;

        private readonly LinearHashMapEntry<K, V> m_Element81;

        private readonly LinearHashMapEntry<K, V> m_Element82;

        private readonly LinearHashMapEntry<K, V> m_Element83;

        private readonly LinearHashMapEntry<K, V> m_Element84;

        private readonly LinearHashMapEntry<K, V> m_Element85;

        private readonly LinearHashMapEntry<K, V> m_Element86;

        private readonly LinearHashMapEntry<K, V> m_Element87;

        private readonly LinearHashMapEntry<K, V> m_Element88;

        private readonly LinearHashMapEntry<K, V> m_Element89;

        private readonly LinearHashMapEntry<K, V> m_Element90;

        private readonly LinearHashMapEntry<K, V> m_Element91;

        private readonly LinearHashMapEntry<K, V> m_Element92;

        private readonly LinearHashMapEntry<K, V> m_Element93;

        private readonly LinearHashMapEntry<K, V> m_Element94;

        private readonly LinearHashMapEntry<K, V> m_Element95;

        private readonly LinearHashMapEntry<K, V> m_Element96;

        private readonly LinearHashMapEntry<K, V> m_Element97;

        private readonly LinearHashMapEntry<K, V> m_Element98;

        private readonly LinearHashMapEntry<K, V> m_Element99;

        private readonly LinearHashMapEntry<K, V> m_Element100;

        private readonly LinearHashMapEntry<K, V> m_Element101;

        private readonly LinearHashMapEntry<K, V> m_Element102;

        private readonly LinearHashMapEntry<K, V> m_Element103;

        private readonly LinearHashMapEntry<K, V> m_Element104;

        private readonly LinearHashMapEntry<K, V> m_Element105;

        private readonly LinearHashMapEntry<K, V> m_Element106;

        private readonly LinearHashMapEntry<K, V> m_Element107;

        private readonly LinearHashMapEntry<K, V> m_Element108;

        private readonly LinearHashMapEntry<K, V> m_Element109;

        private readonly LinearHashMapEntry<K, V> m_Element110;

        private readonly LinearHashMapEntry<K, V> m_Element111;

        private readonly LinearHashMapEntry<K, V> m_Element112;

        private readonly LinearHashMapEntry<K, V> m_Element113;

        private readonly LinearHashMapEntry<K, V> m_Element114;

        private readonly LinearHashMapEntry<K, V> m_Element115;

        private readonly LinearHashMapEntry<K, V> m_Element116;

        private readonly LinearHashMapEntry<K, V> m_Element117;

        private readonly LinearHashMapEntry<K, V> m_Element118;

        private readonly LinearHashMapEntry<K, V> m_Element119;

        private readonly LinearHashMapEntry<K, V> m_Element120;

        private readonly LinearHashMapEntry<K, V> m_Element121;

        private readonly LinearHashMapEntry<K, V> m_Element122;

        private readonly LinearHashMapEntry<K, V> m_Element123;

        private readonly LinearHashMapEntry<K, V> m_Element124;

        private readonly LinearHashMapEntry<K, V> m_Element125;

        private readonly LinearHashMapEntry<K, V> m_Element126;

        private readonly LinearHashMapEntry<K, V> m_Element127;

        public ref LinearHashMapEntry<K, V> this[int index] {
            get {
                RequireIndexInBounds(index);

                return ref GetElement(index);
            }
        }

        private ref LinearHashMapEntry<K, V> GetElement(int index) {
            fixed (LinearHashMapEntry<K, V>* elements = &this.m_Element0) {
                return ref elements[index];
            }
        }

        private void SetElement(int index, LinearHashMapEntry<K, V> value) {
            fixed (LinearHashMapEntry<K, V>* elements = &this.m_Element0) {
                elements[index] = value;
            }
        }

        public const int LENGTH = 128;

        public Enumerator GetEnumerator() {
            // Safe because Enumerator is a 'ref struct'
            fixed (LinearHashMapEntry<K, V>* elements = &this.m_Element0) {
                return new Enumerator(elements);
            }
        }

        public void RequireIndexInBounds(int index) {
            if (index < 0 || index >= 128) {
                // ReSharper disable once UseStringInterpolation (due to Burst)
                throw new InvalidOperationException( string.Format("Index out of bounds: {0}", index));
            }
        }
    }
}