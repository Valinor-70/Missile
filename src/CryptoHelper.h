// CryptoHelper.h
// Educational cryptography helper using OpenSSL.
// Demonstrates AES-256-CBC + HMAC-SHA256 encrypt/decrypt and PBKDF2 password hashing.
// EDUCATIONAL PURPOSE ONLY — no real-world targeting.

#pragma once
#include <string>
#include <vector>

class CryptoHelper
{
public:
    // Hash a password with PBKDF2-SHA256 and a random salt.
    // Returns Base64(hash). Outputs Base64(salt).
    static std::string hashPassword(const std::string &password, std::string &saltOut);

    // Verify a password against a stored hash and salt (both Base64-encoded).
    static bool verifyPassword(const std::string &password,
                               const std::string &storedHashB64,
                               const std::string &storedSaltB64);

    // Encrypt plaintext with AES-256-CBC + HMAC-SHA256.
    // Key is derived from passphrase using PBKDF2.
    // Returns a single Base64 blob: salt|iv|ciphertext|hmac
    static std::string encryptAES(const std::string &plaintext,
                                  const std::string &passphrase);

    // Decrypt a blob produced by encryptAES().
    // Throws std::runtime_error on failure.
    static std::string decryptAES(const std::string &ciphertextB64,
                                  const std::string &passphrase);

    // Generate a random launch code in XXX-XX-XXX format.
    static std::string generateLaunchCode();

    // Base64 encode/decode helpers
    static std::string base64Encode(const std::vector<unsigned char> &data);
    static std::vector<unsigned char> base64Decode(const std::string &b64);

private:
    // Derive a 32-byte key from passphrase + salt using PBKDF2-SHA256 (100 000 iterations).
    static std::vector<unsigned char> deriveKey(const std::string &passphrase,
                                                const std::vector<unsigned char> &salt);

    static constexpr int kSaltSize      = 32;   // bytes
    static constexpr int kKeySize       = 32;   // bytes (AES-256)
    static constexpr int kIvSize        = 16;   // bytes
    static constexpr int kHmacSize      = 32;   // bytes (SHA-256)
    static constexpr int kPbkdf2Iters   = 100000;
};
