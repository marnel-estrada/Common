using UnityEngine;

namespace Common {
    /**
	 * Wraps a GameVariable through queries
	 */
    public class GameVariablesQuery : MonoBehaviour {

        [SerializeField]
        private GameVariables gameVariables;
        
        public static readonly Query<string, string> GET_STRING_GAME_VARIABLE = new Query<string, string>();
        public static readonly Query<string, int> GET_INT_GAME_VARIABLE = new Query<string, int>();
        public static readonly Query<string, float> GET_FLOAT_GAME_VARIABLE = new Query<string, float>();
        public static readonly Query<string, bool> GET_BOOL_GAME_VARIABLE = new Query<string, bool>();
        
        private static readonly PublicStaticFieldsInvoker CLEAR_PROVIDERS = new PublicStaticFieldsInvoker(typeof(GameVariablesQuery), "ClearProvider");
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnDomainReload() {
            CLEAR_PROVIDERS.Execute();
        }

        // Parameters
        public const string KEY = "Key";

        private void Awake() {
            Assertion.NotNull(this.gameVariables);
            
            GET_STRING_GAME_VARIABLE.RegisterProvider(delegate(string key) {
                return this.gameVariables.Get(key);
            });
            
            GET_INT_GAME_VARIABLE.RegisterProvider(delegate(string key) {
                return this.gameVariables.GetInt(key);
            });
            
            GET_FLOAT_GAME_VARIABLE.RegisterProvider(delegate(string key) {
                return this.gameVariables.GetFloat(key);
            });
            
            GET_BOOL_GAME_VARIABLE.RegisterProvider(delegate(string key) {
                return this.gameVariables.GetBool(key);
            });
        }

        /// <summary>
        /// Queries a string game variable
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get(string key) {
            return GET_STRING_GAME_VARIABLE.Execute(key);
        }

        /**
		 * Queries a float game variable
		 */
        public static float GetFloat(string key) {
            return GET_FLOAT_GAME_VARIABLE.Execute(key);
        }

        /**
		 * Queries an int game variable
		 */
        public static int GetInt(string key) {
            return GET_INT_GAME_VARIABLE.Execute(key);
        }

        /// <summary>
        /// Queries a bool game variable
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetBool(string key) {
            return GET_BOOL_GAME_VARIABLE.Execute(key);
        }

    }
}
