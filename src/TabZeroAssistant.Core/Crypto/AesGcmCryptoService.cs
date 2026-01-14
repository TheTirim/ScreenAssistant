using System.Security.Cryptography;

namespace TabZeroAssistant.Core.Crypto;

public sealed class AesGcmCryptoService : ICryptoService
{
    private readonly byte[] _key;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public AesGcmCryptoService(IKeyStore keyStore)
    {
        _key = keyStore.GetOrCreateMasterKey();
    }

    public (byte[] Nonce, byte[] Ciphertext) Encrypt(byte[] plaintext, byte[]? aad)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag, aad);

        // Store ciphertext + tag together for simpler persistence.
        var combined = new byte[ciphertext.Length + TagSize];
        Buffer.BlockCopy(ciphertext, 0, combined, 0, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, combined, ciphertext.Length, TagSize);
        return (nonce, combined);
    }

    public byte[] Decrypt(byte[] nonce, byte[] ciphertext, byte[]? aad)
    {
        if (ciphertext.Length < TagSize)
        {
            throw new CryptographicException("Ciphertext is too short.");
        }

        var payloadLength = ciphertext.Length - TagSize;
        var payload = new byte[payloadLength];
        var tag = new byte[TagSize];
        Buffer.BlockCopy(ciphertext, 0, payload, 0, payloadLength);
        Buffer.BlockCopy(ciphertext, payloadLength, tag, 0, TagSize);

        var plaintext = new byte[payloadLength];
        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, payload, tag, plaintext, aad);
        return plaintext;
    }
}
