using NUnit.Framework;

using Unity.Entities;
using Unity.Entities.Tests;

namespace CommonEcs {
    /// <summary>
    /// Provides utility methods for CommonEcs tests
    /// </summary>
    [TestFixture]
    [Category("CommonEcs")]
    public abstract class CommonEcsTest : ECSTestsFixture {
        protected EntityManager EntityManager {
            get {
                return this.m_Manager;
            }
        }
    }
}