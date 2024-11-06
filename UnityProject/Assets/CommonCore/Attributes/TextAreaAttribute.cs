using System;

namespace Common {
    /// <summary>
    /// An attribute that suggests to an editor to render the property in a text area
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TextAreaAttribute : Attribute {
    }
}