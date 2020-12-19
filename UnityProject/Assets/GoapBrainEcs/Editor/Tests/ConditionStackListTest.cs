using NUnit.Framework;

namespace GoapBrainEcs.Tests {
    [TestFixture]
    [Category("GoapBrainEcs")]
    public class ConditionStackListTest {
        [Test]
        public void TestAdd() {
            ConditionList5 list = new ConditionList5();
            list.Add(new Condition(1, true));
            list.Add(new Condition(2, false));
            
            Assert.True(list.Count == 2);

            Condition condition0 = list[0];
            Assert.True(condition0.id == 1 && condition0.value);

            Condition condition1 = list[1];
            Assert.True(condition1.id == 2 && !condition1.value);
        }

        [Test]
        public void TestRemove() {
            ConditionList5 list = new ConditionList5();
            list.Add(new Condition(1, true));
            list.Add(new Condition(2, false));
            list.Add(new Condition(3, true));
            
            Assert.True(list.Count == 3);
            
            list.RemoveAt(1);
            Assert.True(list.Count == 2);
            
            // Verify that the items are moved when an item is removed
            Assert.True(list[1].id == 3);
            Assert.True(list[0].id == 1);
        }
    }
}
