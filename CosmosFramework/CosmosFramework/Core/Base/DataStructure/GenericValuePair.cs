using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public struct GenericValuePair<TValue, KValue> : IEquatable<GenericValuePair<TValue, KValue>>
    {
        public GenericValuePair(TValue tVar, KValue kVar)
        {
            TVar = tVar;
            KVar = kVar;
        }
        public TValue TVar { get; private set; }
        public KValue KVar { get; private set; }
        public bool Equals(GenericValuePair<TValue, KValue> other)
        {
            return TVar.Equals(other.TVar)&& KVar.Equals(other.KVar);
        }
        public static bool operator ==(GenericValuePair<TValue, KValue> a, GenericValuePair<TValue, KValue> b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(GenericValuePair<TValue, KValue> a, GenericValuePair<TValue, KValue> b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            return obj is GenericValuePair<TValue, KValue> && Equals((GenericValuePair<TValue, KValue>)obj);
        }
        public override int GetHashCode()
        {
            return TVar.GetHashCode() ^ KVar.GetHashCode();
        }
        public override string ToString()
        {
            if (TVar == null)
                throw new ArgumentNullException($"GenericValuePair: {typeof(TValue)} is  invalid");
            if (KVar == null)
                throw new ArgumentNullException($"GenericValuePair: {typeof(KValue)} is  invalid");
            return $"{typeof (TValue)}：{TVar}；{typeof(KValue)}：{KVar}";
        }
    }
}
