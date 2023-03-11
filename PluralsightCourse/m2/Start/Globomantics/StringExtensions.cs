using System.Security.Cryptography;
using System.Text;

namespace Globomantics
{
    public static class StringExtensions
    {
        public static string Sha256(this string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var hash = SHA256.HashData(bytes);

            return Convert.ToBase64String(hash);
        }
    }
}
