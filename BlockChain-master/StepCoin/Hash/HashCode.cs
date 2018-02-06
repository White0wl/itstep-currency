using System.Collections.Generic;
using System.Runtime.Serialization;

namespace StepCoin.Hash
{
    [DataContract]
    public class HashCode
    {
        [DataMember]
        public string Code { get; private set; }
        public override bool Equals(object obj) => (obj as HashCode) != null && Code == ((HashCode)obj).Code;
        public static bool operator ==(HashCode first, HashCode second) => first?.Equals(second) is true;
        public static bool operator !=(HashCode first, HashCode second) => first?.Equals(second) is false;
        public override string ToString() => Code;
        public override int GetHashCode()
        {
            var hash = 13;
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            hash = (hash * 7) + Code.GetHashCode();
            return hash;
        }
        /// <summary>
        /// Клонирует хэш-объект
        /// </summary>
        /// <returns></returns>
        public virtual HashCode Clone() => new HashCode(Code);

        public HashCode(string code) => Code = code;

        public static bool IsNullOrWhiteSpace(HashCode hash) => string.IsNullOrWhiteSpace(hash?.Code);
    }
}
