using System.Security.Cryptography;
using System.Text;

namespace Tenanzia.API.Helpers
{
    public static class PasswordHelper
    {
        //public static string HashPassword(string password)
        //{
        //    using var sha = SHA256.Create();
        //    var bytes = Encoding.UTF8.GetBytes(password);
        //    var hash = sha.ComputeHash(bytes);
        //    return Convert.ToBase64String(hash);
        //}


        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

    }
}
