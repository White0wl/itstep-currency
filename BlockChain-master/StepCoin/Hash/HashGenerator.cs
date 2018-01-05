﻿using System.Linq;
using System.Security.Cryptography;

namespace StepCoin.Hash
{
    public abstract class HashGenerator
    {
        public static byte[] Generate(HashAlgorithm algorithm, byte[] buffer) => buffer is null ? null : algorithm.ComputeHash(buffer);

        public static string GenerateString(HashAlgorithm algorithm, byte[] buffer) => ConvertToString(Generate(algorithm, buffer));

        public static string ConvertToString(byte[] buffer, string format = "X2") => string.Join(string.Empty, buffer is null ? new string[] { } : buffer.Select(b => b.ToString(format)));
    }
}
