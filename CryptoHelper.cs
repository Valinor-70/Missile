using System;
using System.Security.Cryptography;
using System.Text;

namespace MissileSimulator
{
    /// <summary>
    /// Helper class for cryptographic operations
    /// Demonstrates secure password handling and AES encryption
    /// </summary>
    public static class CryptoHelper
    {
        private const int SaltSize = 32; // 256 bits
        private const int KeySize = 32; // 256 bits for AES-256
        private const int Iterations = 100000; // PBKDF2 iterations
        
        /// <summary>
        /// Hash a password using PBKDF2 with a random salt
        /// This demonstrates secure password storage
        /// </summary>
        public static string HashPassword(string password, out string salt)
        {
            byte[] saltBytes = new byte[SaltSize];
            RandomNumberGenerator.Fill(saltBytes);
            salt = Convert.ToBase64String(saltBytes);
            
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(KeySize);
                return Convert.ToBase64String(hash);
            }
        }
        
        /// <summary>
        /// Verify a password against a stored hash
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(KeySize);
                string computedHash = Convert.ToBase64String(hash);
                return computedHash == storedHash;
            }
        }
        
        /// <summary>
        /// Derive an encryption key from a passphrase using PBKDF2
        /// </summary>
        public static byte[] DeriveKey(string passphrase, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(passphrase, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(KeySize);
            }
        }
        
        /// <summary>
        /// Encrypt data using AES-GCM (or AES-CBC with HMAC for compatibility)
        /// Returns Base64-encoded ciphertext with IV and authentication tag
        /// </summary>
        public static string EncryptAES(string plaintext, string passphrase)
        {
            try
            {
                // Generate random salt and IV
                byte[] salt = new byte[16];
                byte[] iv = new byte[16];
                RandomNumberGenerator.Fill(salt);
                RandomNumberGenerator.Fill(iv);
                
                // Derive key from passphrase
                byte[] key = DeriveKey(passphrase, salt);
                
                // Encrypt using AES-CBC
                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    
                    using (var encryptor = aes.CreateEncryptor())
                    {
                        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                        byte[] ciphertext = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
                        
                        // Compute HMAC for authentication
                        using (var hmac = new HMACSHA256(key))
                        {
                            byte[] dataToAuthenticate = new byte[salt.Length + iv.Length + ciphertext.Length];
                            Buffer.BlockCopy(salt, 0, dataToAuthenticate, 0, salt.Length);
                            Buffer.BlockCopy(iv, 0, dataToAuthenticate, salt.Length, iv.Length);
                            Buffer.BlockCopy(ciphertext, 0, dataToAuthenticate, salt.Length + iv.Length, ciphertext.Length);
                            
                            byte[] tag = hmac.ComputeHash(dataToAuthenticate);
                            
                            // Combine salt + iv + ciphertext + tag
                            byte[] result = new byte[salt.Length + iv.Length + ciphertext.Length + tag.Length];
                            Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
                            Buffer.BlockCopy(iv, 0, result, salt.Length, iv.Length);
                            Buffer.BlockCopy(ciphertext, 0, result, salt.Length + iv.Length, ciphertext.Length);
                            Buffer.BlockCopy(tag, 0, result, salt.Length + iv.Length + ciphertext.Length, tag.Length);
                            
                            return Convert.ToBase64String(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Encryption failed: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Decrypt AES-encrypted data
        /// </summary>
        public static string DecryptAES(string ciphertext, string passphrase)
        {
            try
            {
                byte[] data = Convert.FromBase64String(ciphertext);
                
                // Extract components
                byte[] salt = new byte[16];
                byte[] iv = new byte[16];
                byte[] tag = new byte[32];
                byte[] encrypted = new byte[data.Length - 64];
                
                Buffer.BlockCopy(data, 0, salt, 0, 16);
                Buffer.BlockCopy(data, 16, iv, 0, 16);
                Buffer.BlockCopy(data, 32, encrypted, 0, encrypted.Length);
                Buffer.BlockCopy(data, data.Length - 32, tag, 0, 32);
                
                // Derive key
                byte[] key = DeriveKey(passphrase, salt);
                
                // Verify HMAC
                using (var hmac = new HMACSHA256(key))
                {
                    byte[] dataToAuthenticate = new byte[salt.Length + iv.Length + encrypted.Length];
                    Buffer.BlockCopy(salt, 0, dataToAuthenticate, 0, salt.Length);
                    Buffer.BlockCopy(iv, 0, dataToAuthenticate, salt.Length, iv.Length);
                    Buffer.BlockCopy(encrypted, 0, dataToAuthenticate, salt.Length + iv.Length, encrypted.Length);
                    
                    byte[] computedTag = hmac.ComputeHash(dataToAuthenticate);
                    
                    if (!ByteArraysEqual(tag, computedTag))
                    {
                        throw new CryptographicException("Authentication failed - data may have been tampered with");
                    }
                }
                
                // Decrypt
                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    
                    using (var decryptor = aes.CreateDecryptor())
                    {
                        byte[] plaintext = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                        return Encoding.UTF8.GetString(plaintext);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Decryption failed: " + ex.Message);
            }
        }
        
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
        
        /// <summary>
        /// Generate a random launch code in XXX-XX-XXX format
        /// </summary>
        public static string GenerateLaunchCode()
        {
            byte[] randomBytes = new byte[4];
            
            RandomNumberGenerator.Fill(randomBytes);
            int num1 = Math.Abs(BitConverter.ToInt32(randomBytes, 0)) % 1000;
            
            RandomNumberGenerator.Fill(randomBytes);
            int num2 = Math.Abs(BitConverter.ToInt32(randomBytes, 0)) % 100;
            
            RandomNumberGenerator.Fill(randomBytes);
            int num3 = Math.Abs(BitConverter.ToInt32(randomBytes, 0)) % 1000;
            
            return $"{num1:D3}-{num2:D2}-{num3:D3}";
        }
    }
}
