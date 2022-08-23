using System;
using System.Collections.Generic;

using Niantic.ARDK.Utilities.Logging;

using UnityEngine;
using UnityEditor;

namespace Niantic.ARDK.VirtualStudio.Editor
{
  [InitializeOnLoad]
  internal static class _VirtualStudioLauncher
  {
    private const string VS_MODE_KEY = "ARDK_VirtualStudio_Mode";

    private static readonly Dictionary<RuntimeEnvironment, _IVirtualStudioModeLauncher> _modeLaunchers;

    // Default is invalid value
    private static RuntimeEnvironment _selectedMode = RuntimeEnvironment.Default;

    public static RuntimeEnvironment SelectedMode
    {
      get
      {
        if (_selectedMode == RuntimeEnvironment.Default)
        {
          _selectedMode =
            (RuntimeEnvironment)PlayerPrefs.GetInt
            (
              VS_MODE_KEY,
              (int)RuntimeEnvironment.Mock
            );
        }

        return _selectedMode;
      }

      set
      {
        if (value == RuntimeEnvironment.Default)
          throw new InvalidOperationException("Cannot set launch in `Default` mode.");

        _selectedMode = value;
        PlayerPrefs.SetInt(VS_MODE_KEY, (int)_selectedMode);
      }
    }

    public static _IVirtualStudioModeLauncher GetOrCreateModeLauncher(RuntimeEnvironment env)
    {
      if (!_modeLaunchers.ContainsKey(env))
      {
        switch (env)
        {
          case RuntimeEnvironment.Mock:
            _modeLaunchers[env] = new _MockModeLauncher();
            break;

          case RuntimeEnvironment.Remote:
            _modeLaunchers[env] = new _RemoteModeLauncher();
            break;

          case RuntimeEnvironment.LiveDevice:
            _modeLaunchers[env] = null;
            break;
          
          // default to Mock mode
          case RuntimeEnvironment.Default:
            ARLog._Warn("Running _VirtualStudioLauncher with environment Default, which should not happen");
            _modeLaunchers[env] = null;
            break;
          
          default:
            ARLog._Warn("Not a valid runtime environment for _VirtualStudioLauncher");
            _modeLaunchers[env] = null;
            break;
        }
      }

      return _modeLaunchers[env];
    }

    static _VirtualStudioLauncher()
    {
      _modeLaunchers = new Dictionary<RuntimeEnvironment, _IVirtualStudioModeLauncher>();
      EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
      var launcher = GetOrCreateModeLauncher(SelectedMode);
      if (launcher == null)
        return;

      switch (state)
      {
        case PlayModeStateChange.EnteredPlayMode:
          launcher.EnterPlayMode();
          break;

        case PlayModeStateChange.ExitingPlayMode:
          launcher.ExitPlayMode();
          break;
      }
    }
  }
}

