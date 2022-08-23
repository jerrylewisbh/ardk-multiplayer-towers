using Niantic.ARDK.Configuration.Internal;
using Niantic.ARDK.Networking;

namespace Niantic.ARDK.Configuration
{
  // Wrapper as a common reference for all classes wanting to implement _IArdkConfig and _IArdkMetadataConfig
  internal abstract class _ArdkGlobalConfigBase :
    _IArdkConfig,
    _IArdkMetadataConfig
  {
    public abstract bool SetUserIdOnLogin(string userId);

    public abstract bool SetDbowUrl(string url);

    public abstract string GetDbowUrl();

    public abstract string GetContextAwarenessUrl();

    public abstract bool SetContextAwarenessUrl(string url);

    public abstract bool SetApiKey(string key);

    public abstract string GetAuthenticationUrl();

    public abstract bool SetAuthenticationUrl(string url);

    public abstract NetworkingErrorCode VerifyApiKeyWithFeature(string feature, bool isAsync = true);

    public abstract void SetApplicationId(string bundleId);

    public abstract void SetArdkInstanceId(string instanceId);

    public abstract string GetApplicationId();

    public abstract string GetPlatform();

    public abstract string GetManufacturer();

    public abstract string GetDeviceModel();

    public abstract string GetArdkVersion();

    public abstract string GetUserId();

    public abstract string GetClientId();

    public abstract string GetArdkAppInstanceId();

    public abstract string GetApiKey();
  }
}
