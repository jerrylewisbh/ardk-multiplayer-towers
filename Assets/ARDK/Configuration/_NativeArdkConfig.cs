// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Runtime.InteropServices;
using System.Text;

using Niantic.ARDK.Internals;
using Niantic.ARDK.Networking;
using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.Utilities.VersionUtilities;

using UnityEngine;

namespace Niantic.ARDK.Configuration
{
  internal sealed class _NativeArdkConfig:
    _ArdkGlobalConfigBase
  {
    private string _dbowUrl;

    public _NativeArdkConfig()
    {
        ARLog._Debug($"Using config: {nameof(_NativeArdkConfig)}");
    }

    public override bool SetUserIdOnLogin(string userId)
    {
      if (!_NAR_ARDKGlobalConfigHelper_SetUserId(userId))
      {
        ARLog._Warn("Failed to set the user Id");
        return false;
      }

      return true;
    }

    public override bool SetDbowUrl(string url)
    {
      if (!_NAR_ARDKGlobalConfigHelper_SetDBoWUrl(url))
      {
        ARLog._Warn("Failed to set the DBoW URL. It may have already been set.");
        return false;
      }

      // The C++ side actually changes the provided url to include some version information.
      // So, here we just want to clear the cache. On a future get we will get the C++ provided
      // value.
      _dbowUrl = null;
      return true;
    }

    public override string GetDbowUrl()
    {
      var result = _dbowUrl;
      if (result != null)
        return result;

      var stringBuilder = new StringBuilder(512);
      _NAR_ARDKGlobalConfigHelper_GetDBoWUrl(stringBuilder, (ulong)stringBuilder.Capacity);

      result = stringBuilder.ToString();
      _dbowUrl = result;
      return result;
    }

    private string _contextAwarenessUrl;
    public override bool SetContextAwarenessUrl(string url)
    {
      if (!_NAR_ARDKGlobalConfigHelper_SetContextAwarenessUrl(url))
      {
        ARLog._Warn("Failed to set the Context Awareness URL.");
        return false;
      }

      _contextAwarenessUrl = url;
      return true;
    }
    public override string GetContextAwarenessUrl()
    {
      /// For security reasons, we will not exposed the default URL
      return _contextAwarenessUrl;
    }

    public override string GetAuthenticationUrl()
    {
      var stringBuilder = new StringBuilder(512);
      _NAR_ARDKGlobalConfigHelper_GetAuthURL(stringBuilder, (ulong)stringBuilder.Capacity);

      var result = stringBuilder.ToString();
      return result;
    }

    public override bool SetAuthenticationUrl(string url)
    {
      if (!_NAR_ARDKGlobalConfigHelper_SetAuthURL(url))
      {
        ARLog._Warn("Failed to set the Authentication URL.");
        return false;
      }

      return true;
    }

    public override NetworkingErrorCode VerifyApiKeyWithFeature(string feature, bool isAsync)
    {
      var error = 
        (NetworkingErrorCode) _NAR_ARDKGlobalConfigHelper_ValidateApiKeyWithFeature(feature, isAsync);

      return error;
    }

    public override bool SetApiKey(string key)
    {
      if (!_NAR_ARDKGlobalConfigHelper_SetApiKey(key))
      {
        ARLog._Warn("Failed to set the API Key.");
        return false;
      }

      return true;
    }
    
    public override void SetApplicationId(string bundleId)
    {
      _NAR_ARDKGlobalConfigHelperInternal_SetDataField((uint)_ConfigDataField.ApplicationId, bundleId);
    }

    public override void SetArdkInstanceId(string instanceId)
    {
      _NAR_ARDKGlobalConfigHelperInternal_SetDataField((uint)_ConfigDataField.ArdkAppInstanceId, instanceId);
    }

    public override string GetApplicationId()
    {
      var stringBuilder = new StringBuilder(512);
      _NAR_ARDKGlobalConfigHelper_GetDataField((uint)_ConfigDataField.ApplicationId, stringBuilder, (ulong)stringBuilder.Capacity);

      var result = stringBuilder.ToString();
      return result;
    }

    public override string GetPlatform()
    {
#if UNITY_EDITOR
      return Application.unityVersion;
#else
      var stringBuilder = new StringBuilder(512);
      _NAR_ARDKGlobalConfigHelper_GetDataField((uint)_ConfigDataField.Platform, stringBuilder, (ulong)stringBuilder.Capacity);

      var result = stringBuilder.ToString();
      return result;
#endif
    }

    public override string GetManufacturer()
    {
      var stringBuilder = new StringBuilder(512);
      _NAR_ARDKGlobalConfigHelper_GetDataField((uint)_ConfigDataField.Manufacturer, stringBuilder, (ulong)stringBuilder.Capacity);

      var result = stringBuilder.ToString();
      return result;
    }

    public override string GetDeviceModel()
    {
#if UNITY_EDITOR
      return SystemInfo.operatingSystem;
#else
      var stringBuilder = new StringBuilder(512);
      _NAR_ARDKGlobalConfigHelper_GetDataField((uint)_ConfigDataField.DeviceModel, stringBuilder, (ulong)stringBuilder.Capacity);

      var result = stringBuilder.ToString();
      return result;
#endif
    }

    public override string GetArdkVersion()
    {
      return ARDKGlobalVersion.GetARDKVersion();
    }

    public override string GetUserId()
    {
      var stringBuilder = new StringBuilder(512);
      _NAR_ARDKGlobalConfigHelper_GetDataField((uint)_ConfigDataField.UserId, stringBuilder, (ulong)stringBuilder.Capacity);

      var result = stringBuilder.ToString();
      return result;
    }

    public override string GetClientId()
    {
      var stringBuilder = new StringBuilder(512);
      _NAR_ARDKGlobalConfigHelper_GetDataField((uint)_ConfigDataField.ClientId, stringBuilder, (ulong)stringBuilder.Capacity);

      var result = stringBuilder.ToString();
      return result;
    }

    public override string GetArdkAppInstanceId()
    {
      var stringBuilder = new StringBuilder(512);
      _NAR_ARDKGlobalConfigHelper_GetDataField((uint)_ConfigDataField.ArdkAppInstanceId, stringBuilder, (ulong)stringBuilder.Capacity);

      var result = stringBuilder.ToString();
      return result;
    }

    public override string GetApiKey()
    {
      var stringBuilder = new StringBuilder(512);
      _NAR_ARDKGlobalConfigHelper_GetApiKey(stringBuilder, (ulong)stringBuilder.Capacity);

      var result = stringBuilder.ToString();
      return result;
    }

    // get the last good jwt token
    internal string GetJwtToken()
    {
      var stringBuilder = new StringBuilder(512);

      _NAR_ARDKGlobalConfigHelper_GetJwtToken(stringBuilder, (ulong)stringBuilder.Capacity);

      var result = stringBuilder.ToString();
      return result;
    }
    
    // Keep this synchronized with ardk_global_config_helper.hpp
    private enum _ConfigDataField : uint
    {
      ApplicationId = 1,
      Platform,
      Manufacturer,
      DeviceModel,
      UserId,
      ClientId,
      DeveloperId,
      ArdkVersion,
      ArdkAppInstanceId
    } 
    
    // Switch to using a protobuf to pass data back and forth when that is solidified.
    // This is a bit fragile for now
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ARDKGlobalConfigHelperInternal_SetDataField(uint field, string data);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ARDKGlobalConfigHelper_GetApiKey(StringBuilder outKey, ulong maxKeySize);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ARDKGlobalConfigHelper_GetDataField
    (
      uint field,
      StringBuilder outData,
      ulong maxDataSize
    );

    // Set DBoW URL
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ARDKGlobalConfigHelper_SetDBoWUrl(string url);

    // Get DBoW URL
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ARDKGlobalConfigHelper_GetDBoWUrl
    (
      StringBuilder outUrl,
      ulong maxUrlSize
    );

    // Set ContextAwareness URL
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ARDKGlobalConfigHelper_SetContextAwarenessUrl(string url);

    // Set Api Key
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ARDKGlobalConfigHelper_SetApiKey(string key);
    
    // Set Auth URL
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ARDKGlobalConfigHelper_SetAuthURL(string key);
    
    // Attempt to validate the specified feature, with a previously set Api Key. 
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern Int32 _NAR_ARDKGlobalConfigHelper_ValidateApiKeyWithFeature(string feature, bool isAsync);

    // Get Auth URL
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ARDKGlobalConfigHelper_GetAuthURL
    (
      StringBuilder outKey,
      ulong maxKeySize
    );
    
    // Get last known jwt token
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ARDKGlobalConfigHelper_GetJwtToken
    (
      StringBuilder outToken,
      ulong maxTokenSize
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ARDKGlobalConfigHelper_SetUserId(string userId);
  }
}
