using System;

namespace Common {
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyRenderer : Attribute {
        private readonly string rendererType;

        public PropertyRenderer(string rendererType) {
            this.rendererType = rendererType;
        }

        public string RendererType {
            get {
                return this.rendererType;
            }
        }
    }
}