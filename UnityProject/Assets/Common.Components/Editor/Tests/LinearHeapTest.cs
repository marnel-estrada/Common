using System;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace Common.Test {
    [TestFixture]
    [Category("Common")]
    public class LinearHeapTest {
        private class Item {
            public int value;

            public Item(int value) {
                this.value = value;
            }
        }

        private static int ItemComparator(Item a, Item b) {
            if (a.value < b.value) {
                return -1;
            }

            if (a.value > b.value) {
                return 1;
            }

            return 0;
        }
        
        [Test]
        public void TestAdd() {
            LinearHeap<Item> heap = new LinearHeap<Item>(10, ItemComparator);
            heap.Add(new Item(0));
            
            Assert.IsTrue(heap.Count == 1);
        }

        [Test]
        public void TestTop() {
            LinearHeap<Item> heap = new LinearHeap<Item>(10, ItemComparator);
            heap.Add(new Item(5));
            heap.Add(new Item(0));
            
            Assert.IsTrue(heap.Top.value == 5);
        }

        [Test]
        public void TestPop() {
            LinearHeap<Item> heap = new LinearHeap<Item>(10, ItemComparator);
            heap.Add(new Item(7));
            heap.Add(new Item(5));
            heap.Add(new Item(0));
            heap.Add(new Item(3));

            {
                Item popped = heap.Pop();
                Assert.IsTrue(popped.value == 7);
            }

            {
                Item popped = heap.Pop();
                Assert.IsTrue(popped.value == 5);
            }
            
            {
                Item popped = heap.Pop();
                Assert.IsTrue(popped.value == 3);
            }
            
            {
                Item popped = heap.Pop();
                Assert.IsTrue(popped.value == 0);
            }
            
            Assert.IsTrue(heap.Count == 0);
        }

        [Test]
        public void TestFix() {
            LinearHeap<Item> heap = new LinearHeap<Item>(10, ItemComparator);
            
            // Prepare items
            Item a = new Item(1);
            heap.Add(a);
            
            Item b = new Item(2);
            heap.Add(b);
            
            Item c = new Item(3);
            heap.Add(c);
            
            Item d = new Item(4);
            heap.Add(d);
            
            // Fiddle items
            c.value = 1;
            d.value = 2;
            b.value = 3;
            a.value = 4;
            
            heap.Fix();

            int max = int.MaxValue;
            while (heap.Count > 0) {
                Item popped = heap.Pop();
                Assert.IsTrue(max > popped.value);
                max = popped.value;
            }
            
            Assert.IsTrue(heap.Count == 0);
        }

        [Test]
        public void TestCount() {
            LinearHeap<Item> heap = new LinearHeap<Item>(10, ItemComparator);
            heap.Add(new Item(0));
            heap.Add(new Item(5));
            heap.Add(new Item(3));
            
            Assert.IsTrue(heap.Count == 3);
            heap.Pop();
            Assert.IsTrue(heap.Count == 2);
        }

        [Test]
        public void TestClear() {
            LinearHeap<Item> heap = new LinearHeap<Item>(10, ItemComparator);
            heap.Add(new Item(0));
            heap.Add(new Item(5));
            heap.Add(new Item(3));
            
            heap.Clear();
            
            Assert.IsTrue(heap.Count == 0);
        }

        [Test]
        public void TestIllegalTop() {
            LinearHeap<Item> heap = new LinearHeap<Item>(10, ItemComparator);
            LogAssert.Expect(LogType.Error, Assertion.DEFAULT_MESSAGE);
            try {
                Debug.Log(heap.Top.value);
            } catch (Exception e) {
            }
        }

        [Test]
        public void TestIllegalPop() {
            LinearHeap<Item> heap = new LinearHeap<Item>(10, ItemComparator);
            LogAssert.Expect(LogType.Error, Assertion.DEFAULT_MESSAGE);
            try {
                heap.Pop();
            } catch (Exception e) {
            }
        }
    }
}
