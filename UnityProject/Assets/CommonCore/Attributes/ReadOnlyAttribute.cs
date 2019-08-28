using System;

using UnityEngine;

namespace Common {
    [AttributeUsage(AttributeTargets.Property)]
    public class ReadOnlyFieldAttribute : PropertyAttribute {
    }
}