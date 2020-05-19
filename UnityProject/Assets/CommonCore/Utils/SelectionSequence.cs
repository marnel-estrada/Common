using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Common {
    /// <summary>
    /// A utility class that handles next and previous functionality
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SelectionSequence<T> {

        private readonly List<T> items = new List<T>();

        private int currentIndex = 0;

        // Delegate for the listener of the selection sequence
        public delegate void Observer(int index);

        private SimpleList<Observer> observers; // Lazy initialize because not all selection has observers. Most don't.

        /// <summary>
        /// Default constructor
        /// </summary>
        public SelectionSequence() {
        }

        /// <summary>
        /// Constructor with specified array of items
        /// </summary>
        /// <param name="array"></param>
        public SelectionSequence(T[] array) {
            this.items.AddRange(array);
            Reset();
        }

        /// <summary>
        /// Resets the sequence
        /// </summary>
        public void Reset() {
            this.currentIndex = 0;
            NotifyObservers();
        }

        /// <summary>
        /// Moves to the next item
        /// </summary>
        public void MoveToNext() {
            this.currentIndex = (currentIndex + 1) % this.items.Count;
            NotifyObservers();
        }

        /// <summary>
        /// Moves to the previous item
        /// </summary>
        public void MoveToPrevious() {
            int decremented = this.currentIndex - 1;
            this.currentIndex = decremented < 0 ? this.items.Count - 1 : decremented;
            NotifyObservers();
        }

        /// <summary>
        /// Returns the current selected item
        /// </summary>
        public T Current {
            get {
                return this.items[this.currentIndex];
            }
        }

        /// <summary>
        /// Selects the current index
        /// </summary>
        /// <param name="index"></param>
        public void Select(int index) {
            this.currentIndex = index;
            NotifyObservers();
        }

        /// <summary>
        /// Selects the specified item
        /// </summary>
        /// <param name="item"></param>
        public void Select(T item) {
            for(int i = 0; i < this.items.Count; ++i) {
                if(this.items[i].Equals(item)) {
                    Select(i);
                    return;
                }
            }
            
            Assertion.IsTrue(false, "Can't find the item to select: " + item.ToString());
        }

        public int Count {
            get {
                return this.items.Count;
            }
        }

        public int CurrentIndex {
            get {
                return currentIndex;
            }
        }

        /// <summary>
        /// Returns the item at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T GetAt(int index) {
            return this.items[index];
        }

        /// <summary>
        /// Adds an observer
        /// </summary>
        /// <param name="observer"></param>
        public void AddObserver(Observer observer) {
            // Note that we lazy initialize because mose SelectionSequence don't have observers
            if(this.observers == null) {
                this.observers = new SimpleList<Observer>();
            }

            this.observers.Add(observer);
            observer(this.currentIndex); // Invoke so that it processes the current selected item
        }

        private void NotifyObservers() {
            if(this.observers == null) {
                // No observers
                return;
            }

            for(int i = 0; i < this.observers.Count; ++i) {
                this.observers[i](this.currentIndex); // Invoke the delegate
            }
        }

        public T SelectRandom() {
            int randomIndex = Random.Range(0, this.Count);
            
            Select(randomIndex);
            
            return GetAt(randomIndex);
        }
    }
}
