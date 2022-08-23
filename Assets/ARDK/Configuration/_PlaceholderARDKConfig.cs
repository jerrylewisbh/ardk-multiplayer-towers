// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;

using Niantic.ARDK.Configuration.Internal;
using Niantic.ARDK.Networking;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace Niantic.ARDK.Configuration
{
  // <summary>
  // Temporary ardk config class while proper support for other Operating Systems and architecture is being added.
  // </summary>
  internal sealed class _PlaceholderArdkConfig :
    _ArdkGlobalConfigBase
  {
    private static readonly string _clientId;
    
    private string _authenticationUrl;
    private string _userId;
    private string _apiKey;
    private string _dbowUrl;

    static _PlaceholderArdkConfig()
    {
      _clientId = _ArdkMetadataConfigExtension._CreateFormattedGuid();
    }

    public _PlaceholderArdkConfig()
    {
      ARLog._Debug($"Using config: {nameof(_PlaceholderArdkConfig)}");
    }

    public override bool SetUserIdOnLogin(string userId)
    {
      _userId = userId;
      return true;
    }

    public override bool SetDbowUrl(string url)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentException($"{nameof(url)} is null or whitespace.");
      
      _dbowUrl = url;
      return true;
    }

    public override string GetDbowUrl()
    {
      return _dbowUrl ?? string.Empty;
    }

    private string _contextAwarenessUrl;
    public override string GetContextAwarenessUrl()
    {
      return _contextAwarenessUrl ?? string.Empty;
    }

    public override bool SetContextAwarenessUrl(string url)
    {
      if (url == null)
        throw new ArgumentException($"{nameof(url)} is null.");
      
      _contextAwarenessUrl = url;
      return true;
    }

    public override bool SetApiKey(string key)
    {
      if (string.IsNullOrWhiteSpace(key))
        throw new ArgumentException($"{nameof(key)} is null or whitespace.");
      
      _apiKey = key;
      return true;
    }

    public override string GetAuthenticationUrl()
    {
      return _authenticationUrl ?? string.Empty;
    }

    public override bool SetAuthenticationUrl(string url)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentException($"{nameof(url)} is null or whitespace.");
      
      _authenticationUrl = url;
      return true;
    }

    public override NetworkingErrorCode VerifyApiKeyWithFeature(string feature, bool isAsync)
    {
      return NetworkingErrorCode.Ok;
    }
    
    private string _appId;
    public override void SetApplicationId(string bundleId)
    {
      _appId = bundleId;
    }
    public override string GetApplicationId()
    {
      return _appId;
    }

    private string _ardkInstanceId;
    public override void SetArdkInstanceId(string instanceId)
    {
      _ardkInstanceId = instanceId;
    }
    
    public override string GetArdkAppInstanceId()
    {
      return _ardkInstanceId;
    }

    public override string GetPlatform()
    {
      return Application.unityVersion;
    }

    public override string GetManufacturer()
    {
      return null;
    }

    public override string GetDeviceModel()
    {
      return SystemInfo.operatingSystem;
    }

    public override string GetArdkVersion()
    {
      // This doesn't work without the native plugin :(
      return null;
    }

    public override string GetUserId()
    {
      return _userId;
    }

    public override string GetClientId()
    {
      return _clientId;
    }

    public override string GetApiKey()
    {
      return _apiKey;
    }
  }
}
