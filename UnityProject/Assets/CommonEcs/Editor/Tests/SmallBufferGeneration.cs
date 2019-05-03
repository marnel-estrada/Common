using Common;

using NUnit.Framework;

using UnityEngine;

namespace CommonEcs.Test {
    [TestFixture]
    [Category("Common.SmallBufferGeneration")]
    public class SmallBufferGeneration {
        [Test]
        public void Generate() {
            string result = SmallBufferGenerator.Generate("GoapBrainEcs", 
                "ConditionBuckets16", 16, true,
                SmallBufferGenerator.ErrorHandlingStrategy.Exceptions,
                SmallBufferGenerator.ErrorHandlingStrategy.Exceptions, "ConditionList16",
                SmallBufferGenerator.ElementType.UnmanagedWithCsharp7SupportAndUnityBurstSupport);
            Debug.Log(result);
        }
    }
}