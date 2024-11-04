using System;

namespace Common {
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyRenderer : Attribute {
        public PropertyRenderer(string rendererType) {
            this.RendererType = rendererType;
        }

        public string RendererType { get; }
    }
}