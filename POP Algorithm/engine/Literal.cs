using System;
using System.Collections.Generic;

namespace POP
{
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

        public Literal()
        {

        }
    }
}