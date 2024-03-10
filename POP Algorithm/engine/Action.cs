
namespace POP
{
    using System;
    using System.Collections.Generic;
    using static System.ArgumentNullException;

    public class Action : Operator
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
            ThrowIfNull(boundVariables, nameof(boundVariables));

            this.BoundVariables = boundVariables ?? new Dictionary<string, string>();
        }
        public Action(Operator op, Dictionary<string, string> boundVariables)
            : base(op.Name, op.Effects, op.Preconditions, op.Variables)
        {
            ThrowIfNull(boundVariables, nameof(boundVariables));
            this.BoundVariables = boundVariables;
        }

    }
}