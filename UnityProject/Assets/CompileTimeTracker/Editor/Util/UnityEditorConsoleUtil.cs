using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DTCompileTimeTracker {
  public struct UnityConsoleCountsByType {
    public int errorCount;
    public int warningCount;
    public int logCount;
  }

  public static class UnityEditorConsoleUtil {
    private static readonly MethodInfo _clearMethod;
    private static readonly MethodInfo _getCountMethod;
    private static readonly MethodInfo _getCountsByTypeMethod;

    static UnityEditorConsoleUtil() {
      Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
      Type logEntriesType;
#if UNITY_2017_1_OR_NEWER
      logEntriesType = assembly.GetType("UnityEditor.LogEntries");
#else 
      logEntriesType  = assembly.GetType("UnityEditorInternal.LogEntries");
#endif

      _clearMethod = logEntriesType.GetMethod("Clear");
      _getCountMethod = logEntriesType.GetMethod("GetCount");
      _getCountsByTypeMethod = logEntriesType.GetMethod("GetCountsByType");
    }

    public static void Clear() {
      if (_clearMethod == null) {
        Debug.LogError("Failed to find LogEntries.Clear method info!");
        return;
      }

      _clearMethod.Invoke(null, null);
    }

    public static int GetCount() {
      if (_getCountMethod == null) {
        Debug.LogError("Failed to find LogEntries.GetCount method info!");
        return 0;
      }

      return (int)_getCountMethod.Invoke(null, null);
    }

    public static UnityConsoleCountsByType GetCountsByType() {
      UnityConsoleCountsByType countsByType = new UnityConsoleCountsByType();

      if (_getCountsByTypeMethod == null) {
        Debug.LogError("Failed to find LogEntries.GetCountsByType method info!");
        return countsByType;
      }

      object[] arguments = new object[] { 0, 0, 0 };
      _getCountsByTypeMethod.Invoke(null, arguments);

      countsByType.errorCount = (int)arguments[0];
      countsByType.warningCount = (int)arguments[1];
      countsByType.logCount = (int)arguments[2];

      return countsByType;
    }
  }
}
