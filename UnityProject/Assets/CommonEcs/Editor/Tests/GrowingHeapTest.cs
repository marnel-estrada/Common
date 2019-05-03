using NUnit.Framework;

using Unity.Collections;
using Unity.Entities.Tests;
using Unity.Mathematics;

namespace CommonEcs.Test {
    [TestFixture]
    [Category("CommonEcs.GrowingHeap")]
    public class GrowingHeapTest : ECSTestsFixture {
        [Test]
        public void TestPush() {
            NativeList<HeapNode> list = new NativeList<HeapNode>(Allocator.TempJob);
            GrowingHeap heap = new GrowingHeap(list);
            
            heap.Push(CreateNode(0, 0, 0));
            Assert.True(heap.ElementCount == 1);
            
            heap.Push(CreateNode(1, 1, 1));
            Assert.True(heap.ElementCount == 2);
            
            list.Dispose();
        }
        
        [Test]
        public void TestElementCount() {
            NativeList<HeapNode> list = new NativeList<HeapNode>(Allocator.TempJob);
            GrowingHeap heap = new GrowingHeap(list);
            
            heap.Push(CreateNode(0, 0, 0));
            Assert.True(heap.ElementCount == 1);
            
            heap.Push(CreateNode(1, 1, 1));
            Assert.True(heap.ElementCount == 2);
            
            heap.Push(CreateNode(3, 3, 3));
            heap.Push(CreateNode(4, 4, 4));
            heap.Push(CreateNode(2, 2, 2));
            heap.Push(CreateNode(5, 5, 5));
            Assert.True(heap.ElementCount == 6);

            heap.Pop();
            Assert.True(heap.ElementCount == 5);
            
            heap.Pop();
            Assert.True(heap.ElementCount == 4);
            
            heap.Pop();
            heap.Pop();
            Assert.True(heap.ElementCount == 2);
            
            list.Dispose();
        }
        
        [Test]
        public void TestPop() {
            NativeList<HeapNode> list = new NativeList<HeapNode>(Allocator.TempJob);
            GrowingHeap heap = new GrowingHeap(list);
            
            heap.Push(CreateNode(0, 0, 0));
            heap.Push(CreateNode(1, 1, 1));
            heap.Push(CreateNode(3, 3, 3));
            heap.Push(CreateNode(4, 4, 4));
            heap.Push(CreateNode(2, 2, 2));
            heap.Push(CreateNode(5, 5, 5));

            float previousCost = int.MinValue;
            while (heap.HasItems) {
                AStarNode top = heap.Top;
                Assert.True(previousCost < top.F);
                previousCost = top.F;
                heap.Pop();
            }
            
            list.Dispose();
        }
        
        [Test]
        public void TestIndexAccess() {
            NativeList<HeapNode> list = new NativeList<HeapNode>(Allocator.TempJob);
            GrowingHeap heap = new GrowingHeap(list);
            
            heap.Push(CreateNode(0, 0, 0));
            heap.Push(CreateNode(1, 1, 1));
            heap.Push(CreateNode(2, 2, 2));
            heap.Push(CreateNode(3, 3, 3));
            heap.Push(CreateNode(4, 4, 4));
            heap.Push(CreateNode(5, 5, 5));

            for (int i = 0; i < heap.ElementCount; ++i) {
                HeapNode node = heap[i];
                Assert.True(Comparison.TolerantEquals(i, node.cost));
            }
            
            list.Dispose();
        }
        
        [Test]
        public void TestHasItems() {
            NativeList<HeapNode> list = new NativeList<HeapNode>(Allocator.TempJob);
            GrowingHeap heap = new GrowingHeap(list);
            Assert.False(heap.HasItems);
            
            heap.Push(CreateNode(0, 0, 0));
            Assert.True(heap.HasItems);
            
            heap.Push(CreateNode(1, 1, 1));
            heap.Push(CreateNode(2, 2, 2));
            heap.Push(CreateNode(3, 3, 3));
            Assert.True(heap.HasItems);

            while (heap.HasItems) {
                heap.Pop();
            }
            Assert.False(heap.HasItems);
            
            heap.Push(CreateNode(4, 4, 4));
            Assert.True(heap.HasItems);
            
            heap.Clear();
            Assert.False(heap.HasItems);
            
            list.Dispose();
        }
        
        [Test]
        public void TestClear() {
            NativeList<HeapNode> list = new NativeList<HeapNode>(Allocator.TempJob);
            GrowingHeap heap = new GrowingHeap(list);

            const int count = 1000;
            for (int i = 0; i < count; ++i) {
                heap.Push(CreateNode(i, i, i));
            }
            Assert.True(heap.ElementCount == count);
            
            heap.Clear();
            Assert.True(heap.ElementCount == 0);
            Assert.False(heap.HasItems);
            
            list.Dispose();
        }
        
        // Test preventing infinite loop while pushing
        [Test]
        public void TestNextNotPointingToItself() {
            NativeList<HeapNode> list = new NativeList<HeapNode>(Allocator.TempJob);
            GrowingHeap heap = new GrowingHeap(list);
            
            // The values here are derived from a bug that happened during A* search
            heap.Push(CreateNode(-2, 0, 24));
            heap.Pop();
            heap.Push(CreateNode(-2, 1, 15));
            heap.Push(CreateNode(-2, -1, 35));
            heap.Push(CreateNode(-1, 0, 29));
            heap.Push(CreateNode(-3, 0, 21));
            
            Assert.True(CheckIntegrity(heap));
            heap.Pop();
            Assert.True(CheckIntegrity(heap));
            heap.Push(CreateNode(-2, 2, 12));
            Assert.True(CheckIntegrity(heap));
            heap.Push(CreateNode(-1, 1, 26));
            Assert.True(CheckIntegrity(heap));
            heap.Push(CreateNode(-3, 1, 12));
            Assert.True(CheckIntegrity(heap));
            heap.Pop();
            Assert.True(CheckIntegrity(heap));
            
            list.Dispose();
        }

        private bool CheckIntegrity(GrowingHeap heap) {
            for (int i = 0; i < heap.Length; ++i) {
                HeapNode node = heap[i];
                if (node.next == i) {
                    // This causes infinite loop
                    // It must be avoided
                    return false;
                }
            }
            
            return true;
        }

        private static AStarNode CreateNode(int x, int y, int cost) {
            return new AStarNode(0, new int2(x, y), -1, cost, 0);
        }
    }
}