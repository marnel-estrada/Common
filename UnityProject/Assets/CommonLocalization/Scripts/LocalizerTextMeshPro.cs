using UnityEngine;

using Common;

using TMPro;

namespace Game {
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizerTextMeshPro : Localizer {
        private TextMeshProUGUI label;
        
        protected override void Awake() {
            this.label = this.GetRequiredComponent<TextMeshProUGUI>();
            base.Awake();
        }

        public override void UpdateText(Option<string> newText) {
            this.label.text = newText.ValueOr(string.Empty);
        }
    }
}
