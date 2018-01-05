﻿using System.Collections.Generic;

namespace StepCoin.Hash
{
    public class HashCode
    {
        public string Code { get; }

        public override bool Equals(object obj) => (obj as HashCode) is null ? false : Code == (obj as HashCode).Code;

        public static bool operator ==(HashCode first, HashCode second) => first is null ? false : first.Equals(second);

        public static bool operator !=(HashCode first, HashCode second) => first is null ? false : !first.Equals(second);

        public override string ToString() => Code;

        public override int GetHashCode()
        {
            var hashCode = -1470883279;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Code);
            return hashCode;
        }

        public virtual HashCode Clone => new HashCode(Code);

        public HashCode() { }

        public HashCode(string code) => Code = code;

        public static bool IsNullOrWhiteSpace(HashCode hash) => hash is null ? false : string.IsNullOrWhiteSpace(hash.Code);
    }
}