using System.Linq;
using System.Security.Cryptography;

namespace StepCoin.Hash
{
    public abstract class HashGenerator
    {
        public static byte[] Generate(HashAlgorithm algorithm, byte[] buffer)
        {
            if (buffer is null) return null;
            return  algorithm.ComputeHash(buffer);
        }

        public static string GenerateString(HashAlgorithm algorithm, byte[] buffer)
        {
            if (algorithm is null) return string.Empty;
            return ConvertToString(Generate(algorithm, buffer));
        }

        public static string ConvertToString(byte[] buffer, string format = "X2") => string.Join(string.Empty, buffer is null ? new string[] { } : buffer.Select(b => b.ToString(format)));
    }
}
