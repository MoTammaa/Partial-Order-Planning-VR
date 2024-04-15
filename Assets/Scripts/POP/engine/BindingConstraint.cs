
namespace POP
{
    using System;

    using static System.ArgumentNullException;



    public class BindingConstraint : ICloneable, IEquatable<BindingConstraint>
    {
#nullable enable
        private string variable;
        private string bound;

        private bool isEqBelong;

        public string Variable
        {
            get { return variable; }
            set { variable = value; }
        }
        public string Bound
        {
            get { return bound; }
            set { bound = value; }
        }
        public bool IsEqBelong
        {
            get { return isEqBelong; }
            set { isEqBelong = value; }
        }


#nullable disable warnings
        public BindingConstraint(string variable, string bound, bool isEqBelong)
        {
            Helpers.ThrowIfNull(variable, nameof(variable));
            Helpers.ThrowIfNull(bound, nameof(bound));

            this.Variable = variable;
            this.Bound = bound;
            this.IsEqBelong = isEqBelong;
        }

#nullable restore warnings
        public object Clone()
        {
            return new BindingConstraint((string)this.Variable.Clone(), new string(this.Bound), this.IsEqBelong);
        }

        public override string ToString()
        {
            return $"{Variable} {(IsEqBelong ? "=" : "!=")} {string.Join(", ", Bound)}";
        }

        public bool Equals(BindingConstraint? other)
        {
            if (other is null)
            {
                return false;
            }

            return this.Variable.Equals(other.Variable) && this.Bound.Equals(other.Bound) && this.IsEqBelong == other.IsEqBelong;
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as BindingConstraint);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Variable, Bound, IsEqBelong);
        }
        public static bool operator ==(BindingConstraint? left, BindingConstraint? right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(BindingConstraint? left, BindingConstraint? right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(BindingConstraint? left, object right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(BindingConstraint? left, object right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(object left, BindingConstraint? right) { return right is null ? (left is null && right is null) : right.Equals(left); }
        public static bool operator !=(object left, BindingConstraint? right) { return !(right is null ? (left is null && right is null) : right.Equals(left)); }
    }
}