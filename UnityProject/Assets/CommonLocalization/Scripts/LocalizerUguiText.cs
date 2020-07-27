using UnityEngine;
using UnityEngine.UI;

namespace Common {
    /// <summary>
    /// Localizes text in a Unity UI Text component
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class LocalizerUguiText : Localizer {
        private Text text;

        protected override void Awake() {
            this.text = this.GetRequiredComponent<Text>();
            base.Awake();
        }

        public override void UpdateText(Option<string> newText) {
            this.text.text = newText.ValueOr(string.Empty);
        }
    }
}
