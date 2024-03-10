
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
        public Literal(Literal l, Dictionary<string, string>? boundVariables = null)
        {
            ThrowIfNull(l, nameof(l));
            this.Name = l.Name;
            this.IsPositive = l.IsPositive;
            this.Variables = l.Variables;
            this.BoundVariables = boundVariables ?? new Dictionary<string, string>();
        }

        public override string ToString()
        {
            string s = "";
            if (!isPositive)
            {
                s += "¬ ";
            }
            s += name + "(";
            for (int i = 0; i < variables.Length; i++)
            {
                s += variables[i];
                if (i < variables.Length - 1)
                {
                    s += ",";
                }
            }
            s += ")";
            return s;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Literal l = (Literal)obj;
            if (l.Name != Name || l.IsPositive != IsPositive || l.Variables.Length != Variables.Length)
            {
                return false;
            }
            for (int i = 0; i < Variables.Length; i++)
            {
                if (l.Variables[i] != Variables[i])
                {
                    return false;
                }
            }
            return true;
        }


        public bool unifiesWith(Literal l)
        {
            // TODO: Implement unifiesWith
            return false;
        }
#nullable restore warnings
    }
}