using System;
using UnityEditor;
using UnityEngine;

namespace DTCompileTimeTracker {
  [InitializeOnLoad]
  public static class EditorApplicationCompilationUtil {
    public static event Action StartedCompiling = delegate {};
    public static event Action FinishedCompiling = delegate {};

    static EditorApplicationCompilationUtil() {
      EditorApplication.update += OnEditorUpdate;
    }


    private static bool StoredCompilingState {
      get { return EditorPrefs.GetBool("EditorApplicationCompilationUtil::StoredCompilingState"); }
      set { EditorPrefs.SetBool("EditorApplicationCompilationUtil::StoredCompilingState", value); }
    }

    private static void OnEditorUpdate() {
      if (EditorApplication.isCompiling && StoredCompilingState == false) {
        StoredCompilingState = true;
        StartedCompiling.Invoke();
      }

      if (!EditorApplication.isCompiling && StoredCompilingState == true) {
        StoredCompilingState = false;
        FinishedCompiling.Invoke();
      }
    }
  }
}