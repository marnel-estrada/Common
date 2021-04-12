using CommonEcs.Goap;

using UnityEngine;

namespace GoapBrain {
    /// <summary>
    /// Create the GoapDomainDatabase blob asset from the specified list of GoapDomainData.
    /// </summary>
    public class GoapDomainDatabaseHandler : MonoBehaviour {
        [SerializeField]
        private GoapDomainData domains;

        private GoapDomain Parse(GoapDomainData data) {
            GoapDomain domain = new GoapDomain();

            for (int i = 0; i < data.ActionCount; ++i) {
                GoapActionData? actionData = data.GetActionAt(i);
                if (actionData != null) {
                    
                }
            }

            return domain;
        }
    }
}