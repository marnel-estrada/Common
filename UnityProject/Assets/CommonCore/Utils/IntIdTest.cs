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
            Assertion.Assert(a == b);
            Assertion.Assert(a.Equals(b));

            IntId c = new IntId(321);
            Assertion.Assert(a != c);
            Assertion.Assert(b != c);
            Assertion.Assert(!a.Equals(c));
            Assertion.Assert(!b.Equals(c));

            Debug.Log("TestEquality passed");
        }

        private void TestAssignment() {
            IntId a = new IntId(1);
            IntId b = a;
            Assertion.Assert(b == a);

            IntId c = new IntId(3);
            Assertion.Assert(b != c);

            b = c;
            Assertion.Assert(b == c);

            Debug.Log("TestAssignment passed");
        }

        private void TestAsKey() {
            Dictionary<IntId, string> map = new Dictionary<IntId, string>() {
                { new IntId(1), "Dog" },
                { new IntId(2), "Cat" },
                { new IntId(3), "Rabbit" },
            };

            Assertion.Assert(map.ContainsKey(new IntId(2)));
            Assertion.Assert(!map.ContainsKey(new IntId(4)));
            Assertion.Assert(map[new IntId(1)].Equals("Dog"));

            map[new IntId(2)] = "Human";
            Assertion.Assert(map[new IntId(2)].Equals("Human"));

            map.Remove(new IntId(2));
            Assertion.Assert(!map.ContainsKey(new IntId(2)));

            Debug.Log("TestAsKey passed!");
        }

    }
}
