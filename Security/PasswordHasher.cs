using System.Security.Cryptography;

namespace Project02.Security
{
    public static class PasswordHasher
    {
        private const int Iterations = 100000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static (byte[] hash, byte[] salt) HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            using var pdkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pdkdf2.GetBytes(HashSize);
            return (hash, salt);
        }

        public static bool Verify(string password, byte[] salt, byte[] expectedHash, int iterations = Iterations)
        {
            using var pdbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var actual = pdbkdf2.GetBytes(expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expectedHash);
        }
    }
}
