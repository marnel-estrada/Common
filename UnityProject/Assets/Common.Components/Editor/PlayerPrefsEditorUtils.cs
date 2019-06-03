using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace Common {
    class PlayerPrefsEditorUtils {

        [MenuItem("Common/Clear PlayerPrefs")]
        public static void OpenWindow() {
            PlayerPrefs.DeleteAll();
            Debug.Log("PlayerPrefs cleared!");
        }

    }
}
