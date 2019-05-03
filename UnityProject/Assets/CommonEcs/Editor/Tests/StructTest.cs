using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

using Unity.Entities.Tests;

namespace CommonEcs.Test {
    public class StructTest : ECSTestsFixture {
        private struct Position {
            public int x, y;

            public Position(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }
        
        [Test]
        public void TestIn() {
            Position a = new Position(0, 0);
            UpdatePosition(a);
        }

        private void UpdatePosition(in Position position) {
            // This is a compile error which is nice
            // position.x = 1;
        }
    }
}