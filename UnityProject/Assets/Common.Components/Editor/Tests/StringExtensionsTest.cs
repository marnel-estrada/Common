using NUnit.Framework;

using Common;

namespace Common.Test {
    [TestFixture]
    [Category("Common")]
    public class StringExtensionsTest {
        [Test]
        public void TestEqualsFast() {
            const string a = "nbfdjkgdfjkgjkdfgjkdfg347546534bfesuf3745235sdjfgbsdhfg327432rsdfbhjs";
            const string b = "nbfdjkgdfjkgjkdfgjkdfg347546534bfesuf3745235sdjfgbsdhfg327432rsdfbhjs";
            Assert.IsTrue(a.EqualsFast(b));
        }

        [Test]
        public void TestNotEqualsFast() {
            const string a = "nbfdjkgdfjkgjkdfgjkdfg347546534bfesuf3745235sdjfgbsdhfg327432rsdfbhjs";
            const string b = "nbfdjkgdfjkgjkdfgjkdfg347546534bfesuf3745235sdjfgbsdhfg327432rsdfbhjsxxxx";
            Assert.IsFalse(a.EqualsFast(b));
        }
        
        [Test]
        public void TestStartsWithFast() {
            const string a = "nbfdjkgdfjkgjkdfgjkdfg347546534bfesuf3745235sdjfgbsdhfg327432rsdfbhjs";
            Assert.IsTrue(a.StartsWith("nbfdjkgdfjkgjkdfgjkdfg347546534bfesuf3745235sdjfgbsdhfg327432rsdfbhj"));
        }
        
        [Test]
        public void TestEndsWithFast() {
            const string a = "nbfdjkgdfjkgjkdfgjkdfg347546534bfesuf3745235sdjfgbsdhfg327432rsdfbhjs";
            Assert.IsTrue(a.EndsWith("bfdjkgdfjkgjkdfgjkdfg347546534bfesuf3745235sdjfgbsdhfg327432rsdfbhjs"));
        }
    }
}