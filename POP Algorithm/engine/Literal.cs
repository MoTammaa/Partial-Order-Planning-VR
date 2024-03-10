
namespace POP
{
    using System;
    using System.Collections.Generic;
    using static System.ArgumentNullException;
    public class Literal
    {
        private string name;
        private bool isPositive;
        private string[] variables;
        private Dictionary<string, string> boundVariables = new Dictionary<string, string>();

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public bool IsPositive
        {
            get { return isPositive; }
            set { isPositive = value; }
        }
        public string[] Variables
        {
            get { return variables; }
            set { variables = value; }
        }
        public Dictionary<string, string> BoundVariables
        {
            get { return boundVariables; }
            set { boundVariables = value; }
        }
#nullable disable warnings
        public Literal(string name, bool isPositive, string[] variables, Dictionary<string, string> boundVariables)
        {
            ThrowIfNull(name, nameof(name));
            ThrowIfNull(variables, nameof(variables));
            ThrowIfNull(boundVariables, nameof(boundVariables));

            this.Name = name;
            this.IsPositive = isPositive;
            this.Variables = variables;
            this.BoundVariables = boundVariables;

        }
#nullable restore warnings
    }
}