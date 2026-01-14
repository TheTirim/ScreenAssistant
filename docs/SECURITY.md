# Security Notes

- **Key protection**: The 32-byte master key is generated on first run and protected with DPAPI (`DataProtectionScope.CurrentUser`). The protected key is stored at `%APPDATA%\TabZeroAssistant\masterkey.protected`.
- **Record encryption**: Each message and memory is encrypted with AES-256-GCM. A fresh 12-byte nonce is generated per record and stored alongside the ciphertext (ciphertext + tag). AAD binds the record type and id.
- **No plaintext logs**: The app and service avoid logging message contents or decrypted memory data.
- **Local-only**: The Python service binds to `127.0.0.1:8123` only.

Limitations:
- Malware or a local admin can still access data while the app is running and decrypting in memory.
- If the Windows user profile is compromised, the DPAPI-protected key can be unprotected by that user.
