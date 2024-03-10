using System;
using System.Runtime.CompilerServices;

namespace POP
{
    public class Operator
    {

        private string name;
        private List<Literal> effects;
        private List<Literal> preconditions;
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

        public Operator()
        {

        }
    }
}
