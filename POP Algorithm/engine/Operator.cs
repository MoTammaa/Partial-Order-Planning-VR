
namespace POP
{
    using System.Collections.Generic;
    using static System.ArgumentNullException;

    public class Operator
    {

        private string name;
        private List<Literal> effects, preconditions;
        private List<string> variables;

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

        public List<string> Variables
        {
            get { return variables; }
            set { variables = value; }
        }

#nullable disable warnings
        public Operator(string name, List<Literal> effects, List<Literal> preconditions, List<string> variables)
        {
            ThrowIfNull(name, nameof(name));
            ThrowIfNull(effects, nameof(effects));
            ThrowIfNull(preconditions, nameof(preconditions));
            ThrowIfNull(variables, nameof(variables));

            this.Name = name;
            this.Effects = effects;
            this.Preconditions = preconditions;
            this.Variables = variables;

        }
#nullable restore warnings
    }
}
