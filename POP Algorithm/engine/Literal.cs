
namespace POP
{
    using System;
    using System.Collections.Generic;
    using static System.ArgumentNullException;


#pragma warning disable CS0659
    public class Literal : ICloneable, IEquatable<Literal>
    {
        private string name;
        private bool isPositive = true;
        private string[] variables;
        //private Dictionary<string, string> boundVariables = new Dictionary<string, string>();

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
        // public Dictionary<string, string> BoundVariables
        // {
        //     get { return boundVariables; }
        //     set { boundVariables = value; }
        // }
#nullable disable warnings
        public Literal(string name, string[] variables, bool isPositive = true)/*, Dictionary<string, string> boundVariables*/
        {
            ThrowIfNull(name, nameof(name));
            ThrowIfNull(variables, nameof(variables));
            // ThrowIfNull(boundVariables, nameof(boundVariables));

            this.Name = name;
            this.IsPositive = isPositive;
            this.Variables = variables;
            //this.BoundVariables = boundVariables;

        }
        public Literal(Literal l, Dictionary<string, string>? boundVariables = null)
        {
            ThrowIfNull(l, nameof(l));
            this.Name = l.Name.Clone() as string ?? "";
            this.IsPositive = l.IsPositive;
            this.Variables = l.Variables.Clone() as string[] ?? Array.Empty<string>();
            //this.BoundVariables = boundVariables ?? new Dictionary<string, string>();
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


        public bool unifiesWith(Literal l)
        {
            // TODO: Implement unifiesWith
            return false;
        }

        public object Clone()
        {
            return new Literal(this);
        }

        public bool Equals(Literal? other)
        {
            if (other == null) return false;

            if (Name != other.Name || IsPositive != other.IsPositive || Variables.Length != other.Variables.Length)
                return false;

            return true;
        }
#nullable restore warnings


    }
#pragma warning restore CS0659
}