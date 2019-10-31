namespace Common {
    /// <summary>
    /// A utility class for handling text with parameters that avoids garbage
    /// when executing the formatting
    /// </summary>
    public class FormattedText {
        private readonly string baseText;
        private readonly object[] parameters;

        public FormattedText(string baseText, int paramCount) {
            this.baseText = baseText;
            this.parameters = new object[paramCount];
        }

        public void SetParameter(int index, object parameter) {
            this.parameters[index] = parameter;
        }

        public override string ToString() {
            return this.baseText.FormatWith(this.parameters);
        }
    }
}