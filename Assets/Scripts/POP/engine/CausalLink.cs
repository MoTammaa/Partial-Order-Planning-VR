
namespace POP
{
    using System;
    using static System.ArgumentNullException;

    public class CausalLink : ICloneable, IEquatable<CausalLink>
    {
#nullable enable
        private Action produceri;
        private Literal linkCondition;
        private Action consumerj;

        public Action Produceri
        {
            get { return produceri; }
            set { produceri = value; }
        }
        public Literal LinkCondition
        {
            get { return linkCondition; }
            set { linkCondition = value; }
        }
        public Action Consumerj
        {
            get { return consumerj; }
            set { consumerj = value; }
        }

#nullable disable warnings
        public CausalLink(Action produceri, Literal linkCondition, Action consumerj)
        {
            Helpers.ThrowIfNull(produceri, nameof(produceri));
            Helpers.ThrowIfNull(linkCondition, nameof(linkCondition));
            Helpers.ThrowIfNull(consumerj, nameof(consumerj));

            this.Produceri = produceri;
            this.LinkCondition = linkCondition;
            this.Consumerj = consumerj;
        }

#nullable restore warnings
        public object Clone()
        {
            return new CausalLink((Action)this.Produceri.Clone(), new Literal(this.LinkCondition), (Action)this.Consumerj.Clone());
        }

        public override string ToString()
        {
            return $"{Produceri} --{LinkCondition}--> {Consumerj}";
        }

        public bool Equals(CausalLink? other)
        {
            if (other is null)
            {
                return false;
            }

            return this.Produceri.Equals(other.Produceri) && this.LinkCondition.exactEquals(other.LinkCondition) && this.Consumerj.Equals(other.Consumerj);
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as CausalLink);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Produceri, LinkCondition, Consumerj);
        }
        public static bool operator ==(CausalLink? left, CausalLink? right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(CausalLink? left, CausalLink? right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(CausalLink? left, object right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(CausalLink? left, object right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(object left, CausalLink? right) { return right is null ? (left is null && right is null) : right.Equals(left); }
        public static bool operator !=(object left, CausalLink? right) { return !(right is null ? (left is null && right is null) : right.Equals(left)); }
    }
}