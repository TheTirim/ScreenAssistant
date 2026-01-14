using System.Security.Cryptography;
using TabZeroAssistant.Core.Services;

namespace TabZeroAssistant.Core.Crypto;

public sealed class DpapiKeyStore : IKeyStore
{
    public byte[] GetOrCreateMasterKey()
    {
        Directory.CreateDirectory(AppPaths.BaseDirectory);
        if (File.Exists(AppPaths.MasterKeyPath))
        {
            var protectedKey = File.ReadAllBytes(AppPaths.MasterKeyPath);
            return ProtectedData.Unprotect(protectedKey, null, DataProtectionScope.CurrentUser);
        }

        var key = RandomNumberGenerator.GetBytes(32);
        var protectedBytes = ProtectedData.Protect(key, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(AppPaths.MasterKeyPath, protectedBytes);
        return key;
    }
}
