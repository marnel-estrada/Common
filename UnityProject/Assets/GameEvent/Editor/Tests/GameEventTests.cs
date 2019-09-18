using System.IO;

using Common;

using NUnit.Framework;

using UnityEditor;

using UnityEngine;

namespace GameEvent {
    [TestFixture]
    [Category("GameEvent")]
    public class GameEventTests {
        [Test]
        public void EventSelectionTest() {
            string path = "Assets/GameEventComposer/Data/TestEventsPool.asset";
            EventsPool pool = AssetDatabase.LoadAssetAtPath(path, typeof(EventsPool)) as EventsPool;
            Assert.NotNull(pool);
            
            // Specify null to use the default selection strategy
            EventSelectionManager selectionManager = new EventSelectionManager(pool, null);
            selectionManager.Reset();

            for (int i = 0; i < 3; ++i) {
                int eventId = selectionManager.ResolveNextEvent();
                Maybe<EventData> found = pool.Find(eventId);
                Assert.True(found.HasValue);
                
                Debug.Log($"Event: {found.Value.NameId}");
            }
        }

        [Test]
        public void EventInstanceTest() {
            string path = "Assets/GameEventComposer/Data/TestEventsPool.asset";
            EventsPool pool = AssetDatabase.LoadAssetAtPath(path, typeof(EventsPool)) as EventsPool;
            Assert.NotNull(pool);
            
            EventInstanceManager instanceManager = new EventInstanceManager(pool);
            
            // Try to execute an event option
            EventInstance instance = instanceManager.Get(2);
            instance.Options[0].ApplyEffects();
        }
    }
}