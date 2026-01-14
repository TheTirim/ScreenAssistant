namespace TabZeroAssistant.Core.Crypto;

public interface IKeyStore
{
    byte[] GetOrCreateMasterKey();
}
