using System;

namespace Common {
    /// <summary>
    /// An attribute that gives a specific order to a property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class Order : Attribute {
        public int Value { get; }

        public Order(int order) {
            this.Value = order;
        }
    }
}