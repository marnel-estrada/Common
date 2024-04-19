using System.Collections.Generic;
using System.Reflection;

namespace Common {
    public class GenericObjectPropertyInfoComparer : IComparer<PropertyInfo> {
        public int Compare(PropertyInfo x, PropertyInfo y) {
            Order? xOrder = TypeUtils.GetCustomAttribute<Order>(x);
            Order? yOrder = TypeUtils.GetCustomAttribute<Order>(y);
            
            // Null checks
            if (xOrder == null && yOrder == null) {
                // No order specified to both. Order by property name.
                return string.CompareOrdinal(x.Name, y.Name);
            }
            
            if (xOrder != null && yOrder == null) {
                // Considered as x < y
                return -1;
            }

            if (yOrder == null && xOrder == null) {
                // Considered as x > y
                return 1;
            }
            
            // Value check
            int orderDifference = xOrder!.Value - yOrder!.Value;
            if (orderDifference == 0) {
                // Equal order. Order by property name.
                return string.CompareOrdinal(x.Name, y.Name);
            }

            return orderDifference;
        }
    }
}