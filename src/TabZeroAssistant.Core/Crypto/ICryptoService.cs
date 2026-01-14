namespace TabZeroAssistant.Core.Crypto;

public interface ICryptoService
{
    (byte[] Nonce, byte[] Ciphertext) Encrypt(byte[] plaintext, byte[]? aad);
    byte[] Decrypt(byte[] nonce, byte[] ciphertext, byte[]? aad);
}
