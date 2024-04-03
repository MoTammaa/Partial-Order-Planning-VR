
namespace POP
{
    using System;
    using System.Collections.Generic;
    using static System.ArgumentNullException;

    public class Action : Operator, ICloneable, IEquatable<Action>
    {
        private Dictionary<string, string> boundVariables = new Dictionary<string, string>();
        public Dictionary<string, string> BoundVariables
        {
            get { return boundVariables; }
            set { boundVariables = value; }
        }
        public Action(string name, List<Literal> effects, List<Literal> preconditions, string[]? variables = null, Dictionary<string, string>? boundVariables = null)
            : base(name, effects, preconditions, variables)
        {
            // if (this.Variables.Length != 0)
            //     ThrowIfNull(boundVariables, nameof(boundVariables));

            this.BoundVariables = boundVariables ?? new Dictionary<string, string>();
        }
        public Action(Operator op, Dictionary<string, string> boundVariables)
            : base(op.Name, op.Effects, op.Preconditions, op.Variables)
        {
            ThrowIfNull(boundVariables, nameof(boundVariables));
            this.BoundVariables = boundVariables;
        }

        public bool hasConflictingPreconditionsOrEffects(BindingConstraints bc)
        {
            for (int i = 0; i < Preconditions.Count; i++)
            {
                for (int j = i + 1; j < Preconditions.Count; j++)
                {
                    if (Preconditions[i].Equals(Preconditions[j]))
                    {
                        for (int k = 0; k < Preconditions[i].Variables.Length; k++)
                        {
                            if (bc.getBoundEq(Preconditions[i].Variables[k]) == bc.getBoundEq(Preconditions[j].Variables[k])
                                && Preconditions[i].IsPositive != Preconditions[j].IsPositive)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < Effects.Count; i++)
            {
                for (int j = i + 1; j < Effects.Count; j++)
                {
                    if (Effects[i].absoluteEquals(Effects[j]))
                    {
                        for (int k = 0; k < Effects[i].Variables.Length; k++)
                        {
                            if (bc.getBoundEq(Effects[i].Variables[k]) == bc.getBoundEq(Effects[j].Variables[k]))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public new object Clone()
        {
            List<Literal> effects = Effects.Select(l => new Literal(l)).ToList();
            List<Literal> preconditions = Preconditions.Select(l => new Literal(l)).ToList();

            return new Action(this.Name, effects, preconditions, this.Variables?.Clone() as string[] ?? Array.Empty<string>(), new Dictionary<string, string>(this.BoundVariables));
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Action);
        }
        public override int GetHashCode()
        {
            int hash = Name != null ? Name.GetHashCode() : 0;
            if (Variables is null)
                return hash;
            foreach (var variable in Variables)
            {
                hash = hash * 31 + (variable != null ? variable.GetHashCode() : 0);
            }
            return hash;
        }

        public bool Equals(Action? other)
        {
            if (other is null)
                return false;
            if ((this.Variables is null && other.Variables is not null) || (this.Variables is not null && other.Variables is null))
                return false;
            if (this.Variables is null || other.Variables is null) return this.Name == other.Name;
            if (this.Variables.Length != other.Variables.Length)
                return false;
            for (int i = 0; i < this.Variables.Length; i++)
            {
                if (this.Variables[i] != other.Variables[i])
                    return false;
            }
            return this.Name == other.Name;

        }

        public static bool operator ==(Action? left, Action? right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(Action? left, Action? right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(Action? left, object right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(Action? left, object right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(object left, Action? right) { return right is null ? (left is null && right is null) : right.Equals(left); }
        public static bool operator !=(object left, Action? right) { return !(right is null ? (left is null && right is null) : right.Equals(left)); }

        public override string ToString()
        {
            return $"{Name}({(Variables != null ? string.Join(", ", Variables.Select(v => BoundVariables is not null && BoundVariables.ContainsKey(v) ? BoundVariables[v] : v)) : "")})";
        }
    }
}