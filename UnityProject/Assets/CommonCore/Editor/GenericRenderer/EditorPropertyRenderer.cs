using System.Reflection;

namespace Common {
    public abstract class EditorPropertyRenderer {
        public abstract void Render(PropertyInfo property, object instance);
    }
}