using Common;

namespace GoapBrain {
    /// <summary>
    /// There are times that this is needed to collect preconditions and transform it to another condition
    /// Since we can't have an action with no atom actions, we can use this action as filler
    /// </summary>
    [Group("GoapBrain.General")]
    public class NothingAction : GoapAtomAction {
    }
}
