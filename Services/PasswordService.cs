using System.Security.Cryptography;

namespace LoanApp.Services
{
    public class PasswordService : IPasswordService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        public (string hash, string salt) HashPassword(string password)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, HashSize);

            return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
        }

        public bool Verify(string password, string hash, string salt)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash) || string.IsNullOrWhiteSpace(salt))
            {
                return false;
            }

            byte[] expectedHash;
            byte[] saltBytes;

            try
            {
                expectedHash = Convert.FromBase64String(hash);
                saltBytes = Convert.FromBase64String(salt);
            }
            catch (FormatException)
            {
                return false;
            }

            var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
        }
    }
}