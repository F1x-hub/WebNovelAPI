using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace BasicWebNovelAPI.Extensions
{
    public static class HashExtensions
    {
        private const int _saltSize = 16;
        private const int _hashSize = 32;
        private const int _iterations = 100000;

        private static readonly HashAlgorithmName algorithm = HashAlgorithmName.SHA512;

        public static string PasswordHash(this string passwordForHash)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(_saltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(passwordForHash, salt, _iterations, algorithm, _hashSize);

            return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
        }

        public static bool PasswordVerify(this string password, string passwordHash)
        {
            string[] splited = passwordHash.Split('-');

            byte[] hash = Convert.FromHexString(splited.First());
            byte[] salt = Convert.FromHexString(splited.Last());

            byte[] enteredPasswordHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, _iterations, algorithm, _hashSize);

            if (!enteredPasswordHash.SequenceEqual(hash))
            {
                return false;
            }

            return true;
        }
    }
}
