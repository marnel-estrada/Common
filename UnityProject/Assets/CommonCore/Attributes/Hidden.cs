using System;

namespace Common {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class Hidden : Attribute {
    }
}