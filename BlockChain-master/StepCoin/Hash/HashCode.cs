using System.Collections.Generic;

namespace StepCoin.Hash
{
    public class HashCode
    {
        public string Code { get; }

        public override bool Equals(object obj)
        {
            if ((obj as HashCode) is null)
                return false;
            return Code == (obj as HashCode).Code;
        }

        public static bool operator ==(HashCode first, HashCode second)
        {
            if (first is null) return false;
            return first.Equals(second);
        }

        public static bool operator !=(HashCode first, HashCode second)
        {
            if (first is null) return false;
            return !first.Equals(second);
        }

        public override string ToString() => Code;

        public override int GetHashCode()
        {
            var hashCode = -1470883279;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Code);
            return hashCode;
        }
        /// <summary>
        /// Клонирует хэш-объект
        /// </summary>
        /// <returns></returns>
        public virtual HashCode Clone() => new HashCode(Code);

        public HashCode(string code) => Code = code;

        public static bool IsNullOrWhiteSpace(HashCode hash)
        {
            if (hash is null) return true;
            return string.IsNullOrWhiteSpace(hash.Code);
        }
    }
}
