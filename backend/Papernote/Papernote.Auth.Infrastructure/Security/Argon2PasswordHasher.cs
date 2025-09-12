using Konscious.Security.Cryptography;
using Papernote.Auth.Core.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Papernote.Auth.Infrastructure.Security;

public class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int DegreeOfParallelism = 1;
    private const int Iterations = 2;
    private const int MemorySize = 65536; // 64 MB

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        var salt = GenerateSalt();
        var hash = HashPasswordWithSalt(password, salt);
        
        return Convert.ToBase64String(CombineSaltAndHash(salt, hash));
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;
        
        if (string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        try
        {
            var combined = Convert.FromBase64String(hashedPassword);
            var (salt, hash) = ExtractSaltAndHash(combined);
            var computedHash = HashPasswordWithSalt(password, salt);
            
            return CryptographicOperations.FixedTimeEquals(hash, computedHash);
        }
        catch
        {
            return false;
        }
    }

    private static byte[] GenerateSalt()
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);
        return salt;
    }

    private static byte[] HashPasswordWithSalt(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            Iterations = Iterations,
            MemorySize = MemorySize
        };
        
        return argon2.GetBytes(HashSize);
    }

    private static byte[] CombineSaltAndHash(byte[] salt, byte[] hash)
    {
        var combined = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, combined, salt.Length, hash.Length);
        return combined;
    }

    private static (byte[] salt, byte[] hash) ExtractSaltAndHash(byte[] combined)
    {
        if (combined.Length != SaltSize + HashSize)
            throw new ArgumentException("Invalid hash format");

        var salt = new byte[SaltSize];
        var hash = new byte[HashSize];
        
        Buffer.BlockCopy(combined, 0, salt, 0, SaltSize);
        Buffer.BlockCopy(combined, SaltSize, hash, 0, HashSize);
        
        return (salt, hash);
    }
}