
namespace POP
{
    using System.Collections.Generic;
    using static System.ArgumentNullException;

    public class Operator
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
    }
}
