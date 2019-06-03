using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Common {
    /// <summary>
    /// We may specify a GameVariableOverrideResolver in a GameVariable to resolve the correct override
    /// This is usually used for multiple platforms like a different override for Steam and NonSteam
    /// </summary>
    public abstract class GameVariableOverrideResolver : ScriptableObject {

        /// <summary>
        /// Resolves the appropriate override
        /// </summary>
        /// <returns></returns>
        public abstract string ResolveOverride();

    }
}
