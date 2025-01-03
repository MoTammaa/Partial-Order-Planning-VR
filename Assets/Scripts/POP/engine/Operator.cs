
namespace POP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static System.ArgumentNullException;

    public class Operator : ICloneable, IEquatable<Operator>
    {
#nullable enable
        private string name;
        private List<Literal> effects, preconditions;
        private string[] variables;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }


        public List<Literal> Effects
        {
            get { return effects; }
            set { effects = value; }
        }

        public List<Literal> Preconditions
        {
            get { return preconditions; }
            set { preconditions = value; }
        }

        public string[] Variables
        {
            get { return variables; }
            set { variables = value; }
        }

#nullable disable warnings
        public Operator(string name, List<Literal> effects, List<Literal> preconditions, string[]? variables = null)
        {
            Helpers.ThrowIfNull(name, nameof(name));
            Helpers.ThrowIfNull(effects, nameof(effects));
            Helpers.ThrowIfNull(preconditions, nameof(preconditions));
            this.Variables = variables ?? new string[0];


            this.Name = name;
            this.Effects = effects;
            this.Preconditions = preconditions;
            this.Variables = variables;

        }

#nullable restore warnings
        public object Clone()
        {
            List<Literal> effects = Effects.Select(l => (Literal)l.Clone()).ToList();
            List<Literal> preconditions = Preconditions.Select(l => (Literal)l.Clone()).ToList();

            return new Operator(this.Name, effects, preconditions, this.Variables.Clone() as string[] ?? Array.Empty<string>());
        }

        public bool Equals(Operator? other)
        {
            if (other is null)
                return false;
            // check if this operator is instance of an action
            if (this is Action a && other is Action b)
                return a.Equals(b);

            return this.Name == other.Name
                && this.Variables.Length == other.Variables.Length;

        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Variables);
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as Operator);
        }
        public static bool operator ==(Operator? left, Operator? right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(Operator? left, Operator? right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(Operator? left, object right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(Operator? left, object right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(object left, Operator? right) { return right is null ? (left is null && right is null) : right.Equals(left); }
        public static bool operator !=(object left, Operator? right) { return !(right is null ? (left is null && right is null) : right.Equals(left)); }

        public override string ToString()
        {
            return $"{Name}({string.Join(", ", Variables)})";
        }

        public string GetFullStringDetails()
        {
            string preconditions = string.Join(", ", Preconditions.Select(l => l.ToString()));
            string effects = string.Join(", ", Effects.Select(l => l.ToString()));

            return $"{Name}({string.Join(", ", Variables)})\nPreconditions: {(Preconditions?.Count > 0 ? "\n\t" : "--")}{preconditions}\nEffects: {(Effects?.Count > 0 ? "\n\t" : "--")}{effects}";
        }
    }
}
