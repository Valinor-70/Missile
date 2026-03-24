// CryptoHelper.cpp — OpenSSL implementation of crypto helpers.
// Demonstrates AES-256-CBC + HMAC-SHA256 and PBKDF2-SHA256.
// EDUCATIONAL PURPOSE ONLY.

#include "CryptoHelper.h"

#include <openssl/rand.h>
#include <openssl/evp.h>
#include <openssl/hmac.h>
#include <openssl/sha.h>
#include <openssl/bio.h>
#include <openssl/buffer.h>

#include <stdexcept>
#include <cstring>
#include <sstream>
#include <iomanip>
#include <random>

// ─── Base64 ─────────────────────────────────────────────────────────────────

std::string CryptoHelper::base64Encode(const std::vector<unsigned char> &data)
{
    BIO *b64 = BIO_new(BIO_f_base64());
    BIO *mem = BIO_new(BIO_s_mem());
    b64 = BIO_push(b64, mem);

    BIO_set_flags(b64, BIO_FLAGS_BASE64_NO_NL);
    BIO_write(b64, data.data(), static_cast<int>(data.size()));
    BIO_flush(b64);

    BUF_MEM *bptr = nullptr;
    BIO_get_mem_ptr(b64, &bptr);
    std::string result(bptr->data, bptr->length);
    BIO_free_all(b64);
    return result;
}

std::vector<unsigned char> CryptoHelper::base64Decode(const std::string &b64)
{
    BIO *b64Bio = BIO_new(BIO_f_base64());
    BIO *mem    = BIO_new_mem_buf(b64.data(), static_cast<int>(b64.size()));
    b64Bio      = BIO_push(b64Bio, mem);

    BIO_set_flags(b64Bio, BIO_FLAGS_BASE64_NO_NL);

    std::vector<unsigned char> buf(b64.size());
    int len = BIO_read(b64Bio, buf.data(), static_cast<int>(buf.size()));
    BIO_free_all(b64Bio);

    if (len < 0)
        throw std::runtime_error("Base64 decode failed");

    buf.resize(static_cast<size_t>(len));
    return buf;
}

// ─── PBKDF2 ─────────────────────────────────────────────────────────────────

std::vector<unsigned char> CryptoHelper::deriveKey(const std::string &passphrase,
                                                    const std::vector<unsigned char> &salt)
{
    std::vector<unsigned char> key(kKeySize);
    if (PKCS5_PBKDF2_HMAC(passphrase.c_str(), static_cast<int>(passphrase.size()),
                           salt.data(), static_cast<int>(salt.size()),
                           kPbkdf2Iters, EVP_sha256(),
                           kKeySize, key.data()) != 1)
    {
        throw std::runtime_error("PBKDF2 key derivation failed");
    }
    return key;
}

// ─── Password hashing ───────────────────────────────────────────────────────

std::string CryptoHelper::hashPassword(const std::string &password,
                                       std::string &saltOut)
{
    // Generate random salt
    std::vector<unsigned char> salt(kSaltSize);
    if (RAND_bytes(salt.data(), kSaltSize) != 1)
        throw std::runtime_error("RAND_bytes failed");

    saltOut = base64Encode(salt);

    auto key = deriveKey(password, salt);
    return base64Encode(key);
}

bool CryptoHelper::verifyPassword(const std::string &password,
                                   const std::string &storedHashB64,
                                   const std::string &storedSaltB64)
{
    auto salt     = base64Decode(storedSaltB64);
    auto key      = deriveKey(password, salt);
    std::string computed = base64Encode(key);
    // Constant-time compare
    if (computed.size() != storedHashB64.size()) return false;
    unsigned char diff = 0;
    for (size_t i = 0; i < computed.size(); ++i)
        diff |= static_cast<unsigned char>(computed[i]) ^ static_cast<unsigned char>(storedHashB64[i]);
    return diff == 0;
}

// ─── AES-256-CBC + HMAC-SHA256 encrypt ──────────────────────────────────────

std::string CryptoHelper::encryptAES(const std::string &plaintext,
                                      const std::string &passphrase)
{
    // Generate random salt + IV
    std::vector<unsigned char> salt(kSaltSize);
    std::vector<unsigned char> iv(kIvSize);
    if (RAND_bytes(salt.data(), kSaltSize) != 1 ||
        RAND_bytes(iv.data(),   kIvSize)   != 1)
        throw std::runtime_error("RAND_bytes failed");

    auto key = deriveKey(passphrase, salt);

    // AES-256-CBC encrypt
    EVP_CIPHER_CTX *ctx = EVP_CIPHER_CTX_new();
    if (!ctx) throw std::runtime_error("EVP_CIPHER_CTX_new failed");

    if (EVP_EncryptInit_ex(ctx, EVP_aes_256_cbc(), nullptr,
                           key.data(), iv.data()) != 1)
    {
        EVP_CIPHER_CTX_free(ctx);
        throw std::runtime_error("EVP_EncryptInit_ex failed");
    }

    const int inLen = static_cast<int>(plaintext.size());
    std::vector<unsigned char> ciphertext(inLen + 16);
    int outLen1 = 0, outLen2 = 0;

    if (EVP_EncryptUpdate(ctx,
                          ciphertext.data(), &outLen1,
                          reinterpret_cast<const unsigned char*>(plaintext.data()),
                          inLen) != 1)
    {
        EVP_CIPHER_CTX_free(ctx);
        throw std::runtime_error("EVP_EncryptUpdate failed");
    }

    if (EVP_EncryptFinal_ex(ctx, ciphertext.data() + outLen1, &outLen2) != 1)
    {
        EVP_CIPHER_CTX_free(ctx);
        throw std::runtime_error("EVP_EncryptFinal_ex failed");
    }
    EVP_CIPHER_CTX_free(ctx);
    ciphertext.resize(static_cast<size_t>(outLen1 + outLen2));

    // HMAC-SHA256 over salt|iv|ciphertext
    std::vector<unsigned char> authData;
    authData.insert(authData.end(), salt.begin(), salt.end());
    authData.insert(authData.end(), iv.begin(), iv.end());
    authData.insert(authData.end(), ciphertext.begin(), ciphertext.end());

    unsigned char hmacBuf[kHmacSize];
    unsigned int hmacLen = 0;
    HMAC(EVP_sha256(), key.data(), kKeySize,
         authData.data(), authData.size(),
         hmacBuf, &hmacLen);

    // Assemble: salt|iv|ciphertext|hmac
    std::vector<unsigned char> blob;
    blob.insert(blob.end(), salt.begin(), salt.end());
    blob.insert(blob.end(), iv.begin(), iv.end());
    blob.insert(blob.end(), ciphertext.begin(), ciphertext.end());
    blob.insert(blob.end(), hmacBuf, hmacBuf + hmacLen);

    return base64Encode(blob);
}

// ─── AES-256-CBC + HMAC-SHA256 decrypt ──────────────────────────────────────

std::string CryptoHelper::decryptAES(const std::string &ciphertextB64,
                                      const std::string &passphrase)
{
    auto blob = base64Decode(ciphertextB64);

    const size_t minSize = kSaltSize + kIvSize + kHmacSize + 1;
    if (blob.size() < minSize)
        throw std::runtime_error("Ciphertext too short");

    // Parse blob
    std::vector<unsigned char> salt(blob.begin(), blob.begin() + kSaltSize);
    std::vector<unsigned char> iv(blob.begin() + kSaltSize,
                                   blob.begin() + kSaltSize + kIvSize);
    std::vector<unsigned char> hmacStored(blob.end() - kHmacSize, blob.end());
    std::vector<unsigned char> ciphertext(blob.begin() + kSaltSize + kIvSize,
                                           blob.end() - kHmacSize);

    auto key = deriveKey(passphrase, salt);

    // Verify HMAC
    std::vector<unsigned char> authData;
    authData.insert(authData.end(), salt.begin(), salt.end());
    authData.insert(authData.end(), iv.begin(), iv.end());
    authData.insert(authData.end(), ciphertext.begin(), ciphertext.end());

    unsigned char hmacComputed[kHmacSize];
    unsigned int hmacLen = 0;
    HMAC(EVP_sha256(), key.data(), kKeySize,
         authData.data(), authData.size(),
         hmacComputed, &hmacLen);

    // Constant-time compare
    unsigned char diff = 0;
    for (int i = 0; i < kHmacSize; ++i)
        diff |= hmacComputed[i] ^ hmacStored[static_cast<size_t>(i)];
    if (diff != 0)
        throw std::runtime_error("Authentication failed – data may have been tampered with");

    // Decrypt
    EVP_CIPHER_CTX *ctx = EVP_CIPHER_CTX_new();
    if (!ctx) throw std::runtime_error("EVP_CIPHER_CTX_new failed");

    if (EVP_DecryptInit_ex(ctx, EVP_aes_256_cbc(), nullptr,
                           key.data(), iv.data()) != 1)
    {
        EVP_CIPHER_CTX_free(ctx);
        throw std::runtime_error("EVP_DecryptInit_ex failed");
    }

    std::vector<unsigned char> plaintext(ciphertext.size() + 16);
    int outLen1 = 0, outLen2 = 0;

    if (EVP_DecryptUpdate(ctx, plaintext.data(), &outLen1,
                          ciphertext.data(),
                          static_cast<int>(ciphertext.size())) != 1)
    {
        EVP_CIPHER_CTX_free(ctx);
        throw std::runtime_error("EVP_DecryptUpdate failed");
    }

    if (EVP_DecryptFinal_ex(ctx, plaintext.data() + outLen1, &outLen2) != 1)
    {
        EVP_CIPHER_CTX_free(ctx);
        throw std::runtime_error("Decryption failed – wrong key or corrupted data");
    }
    EVP_CIPHER_CTX_free(ctx);
    plaintext.resize(static_cast<size_t>(outLen1 + outLen2));

    return std::string(plaintext.begin(), plaintext.end());
}

// ─── Launch code ────────────────────────────────────────────────────────────

std::string CryptoHelper::generateLaunchCode()
{
    unsigned char buf[12];
    RAND_bytes(buf, sizeof(buf));

    auto toNum = [&](int offset, int mod) -> int {
        unsigned int v = 0;
        std::memcpy(&v, buf + offset, 4);
        return static_cast<int>(v % static_cast<unsigned>(mod));
    };

    int a = toNum(0, 1000);
    int b = toNum(4, 100);
    int c = toNum(8, 1000);

    std::ostringstream oss;
    oss << std::setfill('0') << std::setw(3) << a
        << '-' << std::setw(2) << b
        << '-' << std::setw(3) << c;
    return oss.str();
}
