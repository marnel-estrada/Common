using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common {
    public class IntIdTest : MonoBehaviour {

        private void Awake() {
            TestEquality();
            TestAssignment();
            TestAsKey();
        }

        private void TestEquality() {
            IntId a = new IntId(123);
            IntId b = new IntId(123);
            Assertion.IsTrue(a == b);
            Assertion.IsTrue(a.Equals(b));

            IntId c = new IntId(321);
            Assertion.IsTrue(a != c);
            Assertion.IsTrue(b != c);
            Assertion.IsTrue(!a.Equals(c));
            Assertion.IsTrue(!b.Equals(c));

            Debug.Log("TestEquality passed");
        }

        private void TestAssignment() {
            IntId a = new IntId(1);
            IntId b = a;
            Assertion.IsTrue(b == a);

            IntId c = new IntId(3);
            Assertion.IsTrue(b != c);

            b = c;
            Assertion.IsTrue(b == c);

            Debug.Log("TestAssignment passed");
        }

        private void TestAsKey() {
            Dictionary<IntId, string> map = new Dictionary<IntId, string>() {
                { new IntId(1), "Dog" },
                { new IntId(2), "Cat" },
                { new IntId(3), "Rabbit" },
            };

            Assertion.IsTrue(map.ContainsKey(new IntId(2)));
            Assertion.IsTrue(!map.ContainsKey(new IntId(4)));
            Assertion.IsTrue(map[new IntId(1)].Equals("Dog"));

            map[new IntId(2)] = "Human";
            Assertion.IsTrue(map[new IntId(2)].Equals("Human"));

            map.Remove(new IntId(2));
            Assertion.IsTrue(!map.ContainsKey(new IntId(2)));

            Debug.Log("TestAsKey passed!");
        }

    }
}
