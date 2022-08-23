// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.Linq;
using System.Text;

using Niantic.ARDK.VirtualStudio.AR;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.Networking;
using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VirtualStudio.AR.Mock;
using Niantic.ARDK.VirtualStudio.Networking;
using Niantic.ARDK.VirtualStudio.Networking.Mock;

using UnityEditor;

using UnityEngine;

namespace Niantic.ARDK.VirtualStudio.Editor
{
  public sealed class VirtualStudioConfigurationEditor : EditorWindow
  {
    // "None" tab correlates to RuntimeEnvironment.Native (as in nothing running in Virtual Studio),
    // so _vsModeTabSelection == (int)currRuntimeEnvironment - 1
    private static readonly string[] _modeSelectionGridStrings = { "None", "Remote", "Mock" };
    private int _vsModeTabSelection;

    [SerializeField]
    private _RemoteConfigurationEditor _remoteConfigEditor;

    [SerializeField]
    private _MockPlayConfigurationEditor _mockPlayConfigEditor;

    private static GUIStyle _headerStyle;

    internal static GUIStyle _HeaderStyle
    {
      get
      {
        if (_headerStyle == null)
        {
          _headerStyle = new GUIStyle(EditorStyles.boldLabel);
          _headerStyle.fontSize = 18;
          _headerStyle.fixedHeight = 36;
        }

        return _headerStyle;
      }
    }

    private static GUIStyle _subHeadingStyle;

    internal static GUIStyle _SubHeadingStyle
    {
      get
      {
        if (_subHeadingStyle == null)
        {
          _subHeadingStyle = new GUIStyle(EditorStyles.boldLabel);
          _subHeadingStyle.fontSize = 14;
          _subHeadingStyle.fixedHeight = 28;
        }

        return _subHeadingStyle;
      }
    }

    private static GUIStyle _lineBreakStyle;
    internal static GUIStyle _LineBreakStyle
    {
      get
      {
        if (_lineBreakStyle == null)
        {
          _lineBreakStyle = new GUIStyle(EditorStyles.label);
          _lineBreakStyle.wordWrap = false;
        }

        return _lineBreakStyle;
      }
    }

    private Vector2 scrollPos = Vector2.zero;

    [MenuItem("Lightship/ARDK/Virtual Studio")]
    public static void Init()
    {
      var window = GetWindow<VirtualStudioConfigurationEditor>(false, "Virtual Studio");
      window.Show();

      window._mockPlayConfigEditor = new _MockPlayConfigurationEditor();
      window._remoteConfigEditor = new _RemoteConfigurationEditor();

      window.ApplyModeChange();
    }

    private void ApplyModeChange()
    {
      var currentRuntime = _VirtualStudioLauncher.SelectedMode;

      // Valid RuntimeEnvironment values start at 1
      _vsModeTabSelection = (int)currentRuntime - 1;

      _remoteConfigEditor.OnSelectionChange(currentRuntime == RuntimeEnvironment.Remote);
      _mockPlayConfigEditor.OnSelectionChange(currentRuntime == RuntimeEnvironment.Mock);
    }

    private void OnGUI()
    {
      using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos))
      {
        scrollPos = scrollView.scrollPosition;

        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        DrawEnabledGUI();
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(50);

        switch (_VirtualStudioLauncher.SelectedMode)
        {
          case RuntimeEnvironment.Remote:
            EditorGUILayout.LabelField("Remote Mode - USB", _HeaderStyle);
            GUILayout.Space(10);
            _remoteConfigEditor.DrawGUI();
            break;

          case RuntimeEnvironment.Mock:
            EditorGUILayout.LabelField("Mock Mode", _HeaderStyle);
            GUILayout.Space(10);
            _mockPlayConfigEditor.DrawGUI();
            break;
        }
      }
    }

    private void DrawEnabledGUI()
    {
      var newModeSelection =
        GUI.SelectionGrid
        (
          new Rect(10, 20, 300, 20),
          _vsModeTabSelection,
          _modeSelectionGridStrings,
          3
        );

      if (newModeSelection != _vsModeTabSelection)
      {
        _vsModeTabSelection = newModeSelection;
        _VirtualStudioLauncher.SelectedMode = (RuntimeEnvironment)(_vsModeTabSelection + 1);
        ApplyModeChange();
      }
    }
  }
}
