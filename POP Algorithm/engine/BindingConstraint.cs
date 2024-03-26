
namespace POP
{
    using System;

    using static System.ArgumentNullException;



    public class BindingConstraint : ICloneable, IEquatable<BindingConstraint>
    {
        private string variable;
        private List<string> bounds;

        private bool isEqBelong;

        public string Variable
        {
            get { return variable; }
            set { variable = value; }
        }
        public List<string> Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }
        public bool IsEqBelong
        {
            get { return isEqBelong; }
            set { isEqBelong = value; }
        }


#nullable disable warnings
        public BindingConstraint(string variable, List<string> bounds, bool isEqBelong)
        {
            ThrowIfNull(variable, nameof(variable));
            ThrowIfNull(bounds, nameof(bounds));

            this.Variable = variable;
            this.Bounds = bounds;
            this.IsEqBelong = isEqBelong;
        }

#nullable restore warnings
        public object Clone()
        {
            return new BindingConstraint((string)this.Variable.Clone(), new List<string>(this.Bounds), this.IsEqBelong);
        }

        public override string ToString()
        {
            return $"{Variable} {(IsEqBelong ? "=" : "!=")} {string.Join(", ", Bounds)}";
        }

        public bool Equals(BindingConstraint? other)
        {
            if (other is null)
            {
                return false;
            }

            return this.Variable.Equals(other.Variable) && this.Bounds.SequenceEqual(other.Bounds) && this.IsEqBelong == other.IsEqBelong;
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as BindingConstraint);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Variable, Bounds, IsEqBelong);
        }
        public static bool operator ==(BindingConstraint? left, BindingConstraint? right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(BindingConstraint? left, BindingConstraint? right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(BindingConstraint? left, object right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(BindingConstraint? left, object right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(object left, BindingConstraint? right) { return right is null ? (left is null && right is null) : right.Equals(left); }
        public static bool operator !=(object left, BindingConstraint? right) { return !(right is null ? (left is null && right is null) : right.Equals(left)); }
    }
}