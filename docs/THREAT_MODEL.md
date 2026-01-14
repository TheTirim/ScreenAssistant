# Threat Model (MVP)

- Protect chat/memory content at rest with AES-GCM per record.
- Protect master key using DPAPI CurrentUser to keep data scoped per Windows profile.
- Avoid logging plaintext data.
- Limit local service to loopback and never execute actions without user clicks.
- Known gaps: runtime memory exposure, compromised Windows user account, or malware with user-level access.
