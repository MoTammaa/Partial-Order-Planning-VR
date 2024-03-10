using System;
using System.Collections.Generic;

namespace POP
{
    public class Action : Operator
    {
        private Dictionary<string, string> boundVariables = new Dictionary<string, string>();
        public Dictionary<string, string> BoundVariables
        {
            get { return boundVariables; }
            set { boundVariables = value; }
        }
        public Action()
        {

        }

    }
}