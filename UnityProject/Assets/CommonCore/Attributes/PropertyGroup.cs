using System;

namespace Common {
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyGroup : Attribute {
        private readonly string name;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public PropertyGroup(string name) {
            this.name = name;
        }

        public string Name {
            get {
                return this.name;
            }
        }
    }
}
