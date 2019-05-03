using System;

using NUnit.Framework;
using Common;

using UnityEngine;

namespace CommonEcs.Test {
    [TestFixture]
    [Category("Common.IntSmallArray10")]
    public class IntStackArray10Test {
        [Test]
        public void TestIndexAccess() {
            IntStackArray10 array = new IntStackArray10();
            
            // Set values
            for (int i = 0; i < IntStackArray10.Length; ++i) {
                array[i] = i * 2;
            }
            
            // Verify
            for (int i = 0; i < IntStackArray10.Length; ++i) {
                Debug.Log($"{i} = {array[i]}");
                Assert.IsTrue(array[i] == i * 2);
            }
        }

        [Test]
        public void TestClear() {
            IntStackArray10 array = new IntStackArray10();
            
            // Set values
            for (int i = 0; i < IntStackArray10.Length; ++i) {
                array[i] = i * 2;
            }
            
            array.Clear();
            
            // Verify
            for (int i = 0; i < IntStackArray10.Length; ++i) {
                Assert.IsTrue(array[i] == 0);
            }
        }

        [Test]
        public void TestInvalidAccess() {
            Assert.Throws<InvalidOperationException>(delegate {
                IntStackArray10 array = new IntStackArray10();
                array[10] = 1;
            });
            
            Assert.Throws<InvalidOperationException>(delegate {
                IntStackArray10 array = new IntStackArray10();
                array[-1] = 1;
            });
        }
    }
}