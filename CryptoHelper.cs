using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace MissileSimulator
{
    /// <summary>
    /// Cryptographic helper class demonstrating secure password hashing and AES encryption.
    /// Educational purposes only - demonstrates proper use of PBKDF2, salts, and authenticated encryption.
    /// </summary>
    public static class CryptoHelper
    {
        private const int PBKDF2_ITERATIONS = 100000; // Industry standard iterations
        private const int SALT_SIZE = 16; // 128 bits
        private const int KEY_SIZE = 32; // 256 bits for AES-256
        private const int IV_SIZE = 16; // 128 bits for AES
        
        /// <summary>
        /// Hash a password using PBKDF2 with a random salt.
        /// This demonstrates secure password storage (never store plaintext passwords).
        /// </summary>
        public static (byte[] hash, byte[] salt) HashPassword(string password)
        {
            byte[] salt = new byte[SALT_SIZE];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, PBKDF2_ITERATIONS, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(KEY_SIZE);
                return (hash, salt);
            }
        }
        
        /// <summary>
        /// Verify a password against a stored hash and salt.
        /// </summary>
        public static bool VerifyPassword(string password, byte[] storedHash, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, PBKDF2_ITERATIONS, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(KEY_SIZE);
                
                // Constant-time comparison to prevent timing attacks
                bool equal = hash.Length == storedHash.Length;
                for (int i = 0; i < hash.Length && i < storedHash.Length; i++)
                {
                    equal &= hash[i] == storedHash[i];
                }
                return equal;
            }
        }
        
        /// <summary>
        /// Derive an encryption key from a passphrase using PBKDF2.
        /// This demonstrates key derivation from user input.
        /// </summary>
        public static byte[] DeriveKey(string passphrase, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(passphrase, salt, PBKDF2_ITERATIONS, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(KEY_SIZE);
            }
        }
        
        /// <summary>
        /// Encrypt data using AES-256-CBC with HMAC for authentication.
        /// This demonstrates authenticated encryption.
        /// Returns: IV + Ciphertext + HMAC (all concatenated)
        /// </summary>
        public static byte[] EncryptAES(string plaintext, byte[] key)
        {
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV();
                
                byte[] iv = aes.IV;
                
                using (var encryptor = aes.CreateEncryptor(key, iv))
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(plaintextBytes, 0, plaintextBytes.Length);
                    cs.FlushFinalBlock();
                    byte[] ciphertext = ms.ToArray();
                    
                    // Create HMAC for authentication
                    using (var hmac = new HMACSHA256(key))
                    {
                        byte[] dataToAuthenticate = new byte[iv.Length + ciphertext.Length];
                        Buffer.BlockCopy(iv, 0, dataToAuthenticate, 0, iv.Length);
                        Buffer.BlockCopy(ciphertext, 0, dataToAuthenticate, iv.Length, ciphertext.Length);
                        byte[] mac = hmac.ComputeHash(dataToAuthenticate);
                        
                        // Return IV + Ciphertext + MAC
                        byte[] result = new byte[iv.Length + ciphertext.Length + mac.Length];
                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(ciphertext, 0, result, iv.Length, ciphertext.Length);
                        Buffer.BlockCopy(mac, 0, result, iv.Length + ciphertext.Length, mac.Length);
                        
                        return result;
                    }
                }
            }
        }
        
        /// <summary>
        /// Decrypt data using AES-256-CBC with HMAC verification.
        /// Throws exception if MAC verification fails (tampered data).
        /// </summary>
        public static string DecryptAES(byte[] encryptedData, byte[] key)
        {
            if (encryptedData.Length < IV_SIZE + 32) // IV + at least 1 block + MAC
            {
                throw new CryptographicException("Invalid encrypted data length");
            }
            
            // Extract components
            byte[] iv = new byte[IV_SIZE];
            byte[] mac = new byte[32]; // SHA256 HMAC is 32 bytes
            byte[] ciphertext = new byte[encryptedData.Length - IV_SIZE - 32];
            
            Buffer.BlockCopy(encryptedData, 0, iv, 0, IV_SIZE);
            Buffer.BlockCopy(encryptedData, IV_SIZE, ciphertext, 0, ciphertext.Length);
            Buffer.BlockCopy(encryptedData, IV_SIZE + ciphertext.Length, mac, 0, 32);
            
            // Verify HMAC
            using (var hmac = new HMACSHA256(key))
            {
                byte[] dataToAuthenticate = new byte[iv.Length + ciphertext.Length];
                Buffer.BlockCopy(iv, 0, dataToAuthenticate, 0, iv.Length);
                Buffer.BlockCopy(ciphertext, 0, dataToAuthenticate, iv.Length, ciphertext.Length);
                byte[] computedMac = hmac.ComputeHash(dataToAuthenticate);
                
                // Constant-time comparison
                bool valid = mac.Length == computedMac.Length;
                for (int i = 0; i < mac.Length && i < computedMac.Length; i++)
                {
                    valid &= mac[i] == computedMac[i];
                }
                
                if (!valid)
                {
                    throw new CryptographicException("HMAC verification failed - data may be tampered");
                }
            }
            
            // Decrypt
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                
                using (var decryptor = aes.CreateDecryptor(key, iv))
                using (var ms = new MemoryStream(ciphertext))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        
        /// <summary>
        /// Generate a random authentication code in format XXX-XX-XXX
        /// </summary>
        public static string GenerateAuthCode()
        {
            var random = new Random();
            return $"{random.Next(100, 999):D3}-{random.Next(10, 99):D2}-{random.Next(100, 999):D3}";
        }
    }
}
