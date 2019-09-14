using System;

using UnityEngine;

namespace Common {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ReadOnlyFieldAttribute : PropertyAttribute {
    }
}