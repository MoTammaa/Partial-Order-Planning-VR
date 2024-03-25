
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
            return HashCode.Combine(Name, Variables);
        }

        public bool Equals(Action? other)
        {
            if (other is null)
                return false;
            if (this.Variables.Length != other.Variables.Length)
                return false;
            for (int i = 0; i < this.Variables.Length; i++)
            {
                if (this.Variables[i] != other.Variables[i])
                    return false;
            }
            return this.Name == other.Name;

        }

        public override string ToString()
        {
            return $"{Name}({(Variables != null ? string.Join(", ", Variables.Select(v => BoundVariables is not null && BoundVariables.ContainsKey(v) ? BoundVariables[v] : v)) : "")})";
        }
    }
}