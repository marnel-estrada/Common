namespace Common {
    public abstract class DataPoolFilterStrategy<T> where T : Identifiable, new() {
        private readonly string label;
        private readonly int labelWidth;

        private string filterText = "";

        /// <summary>
        /// Constructor with specified label
        /// </summary>
        /// <param name="label"></param>
        public DataPoolFilterStrategy(string label, int labelWidth) {
            this.label = label;
            this.labelWidth = labelWidth;
        }

        public string FilterText {
            get {
                return this.filterText;
            }
            
            set {
                // Use empty when null is specified
                // filterText can't be null
                this.filterText = value ?? "";
            }
        }

        public string Label {
            get {
                return this.label;
            }
        }

        public int LabelWidth {
            get {
                return this.labelWidth;
            }
        }

        /// <summary>
        /// Returns whether or not the specified is filtered
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract bool IsFilterMet(T data);
    }
}