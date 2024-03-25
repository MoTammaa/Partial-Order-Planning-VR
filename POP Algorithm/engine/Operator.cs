
namespace POP
{
    using System.Collections.Generic;
    using static System.ArgumentNullException;

    public class Operator : ICloneable, IEquatable<Operator>
    {

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
            ThrowIfNull(name, nameof(name));
            ThrowIfNull(effects, nameof(effects));
            ThrowIfNull(preconditions, nameof(preconditions));
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

        public override string ToString()
        {
            return $"{Name}({string.Join(", ", Variables)})";
        }
    }
}
