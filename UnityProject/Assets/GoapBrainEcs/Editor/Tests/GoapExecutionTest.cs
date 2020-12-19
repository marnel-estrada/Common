using Common;

using NUnit.Framework;

using Unity.Entities;
using Unity.Entities.Tests;

namespace GoapBrainEcs {
    [TestFixture]
    [Category("GoapBrainEcs")]
    public class GoapExecutionTest : ECSTestsFixture {
        public EntityManager EntityManager {
            get {
                return this.m_Manager;
            }
        }

        [Test]
        public void ExecuteOnFailTest() {
            ExecuteOnFail test = new ExecuteOnFail(this.World, this.EntityManager);
            const int FRAMES = 20;
            test.Execute(FRAMES);
        }

        [Test]
        public void MultipleOnFailExecutionTest() {
            const int FRAMES = 20;
            new MultipleOnFailExecution(this.World, this.EntityManager).Execute(FRAMES);
        }

        [Test]
        public void FailedAtomActionTest() {
            const int frames = MultipleAtomActions.MAX_VALUE * 2;
            new FailedAtomAction(this.World, this.EntityManager).Execute(frames);
        }

        [Test]
        public void MoveIntTranslationTest() {
            const int frames = 40;
            new MoveIntTranslationTest(this.World, this.EntityManager).Execute(frames);
        }

        [Test]
        public void MultipleActionsWithMultipleAtomActionsTest() {
            const int frames = 50;
            new MultipleActionsWithMultipleAtomActions(this.World, this.EntityManager).Execute(frames);
        }

        [Test]
        public void MultipleAtomActionsTest() {
            const int frames = MultipleAtomActions.MAX_VALUE * 3;
            new MultipleAtomActions(this.World, this.EntityManager).Execute(frames);
        }

        [Test]
        public void SingleAtomActionTest() {
            SingleAtomAction test = new SingleAtomAction(this.World, this.EntityManager);
            const int frames = 20;
            test.Execute(frames);
        }

        [Test]
        public void SingleMultipleFrameAtomActionTest() {
            const int frames = SingleMultipleFrameAtomAction.MAX_VALUE * 2;
            SingleMultipleFrameAtomAction test = new SingleMultipleFrameAtomAction(this.World, this.EntityManager);
            test.Execute(frames);
        }

        [Test]
        public void ReplanOnFailTest() {
            const int FRAMES = 20;
            new ReplanOnFail(this.World, this.EntityManager).Execute(FRAMES, true);
        }

        [Test]
        public void ReplanOnSuccessTest() {
            const int FRAMES = 20;
            new ReplanOnSuccess(this.World, this.EntityManager).Execute(FRAMES, true);
        }

        [Test]
        public void ActionShouldReplaceFailedConditionResolverTest() {
            const int FRAMES = 20;
            new ActionShouldReplaceFailedConditionResolver(this.World, this.EntityManager).Execute(FRAMES);
        }

        [Test]
        public void FirstAtomActionFailedTest() {
            const int FRAMES = 20;
            new FirstAtomActionFailed(this.World, this.EntityManager).Execute(FRAMES);
        }

        [Test]
        public void SecondAtomActionFailedTest() {
            const int FRAMES = 20;
            new SecondAtomActionFailed(this.World, this.EntityManager).Execute(FRAMES);
        }

        [Test]
        public void ThirdAtomActionFailedTest() {
            const int FRAMES = 20;
            new ThirdAtomActionFailed(this.World, this.EntityManager).Execute(FRAMES);
        }
    }
}