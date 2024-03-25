
namespace POP
{
    using System;
    using System.Collections.Generic;
    using static System.ArgumentNullException;

    public class PlanningProblem
    {
        private readonly HashSet<Operator> operators;
        private List<Literal> initialState;
        private List<Literal> goalState;
        private readonly HashSet<Literal> literals = new HashSet<Literal>();

        public HashSet<Operator> Operators
        {
            get { return operators; }
        }
        public List<Literal> InitialState
        {
            get { return initialState; }
        }
        public List<Literal> GoalState
        {
            get { return goalState; }
        }

        public HashSet<Literal> Literals
        {
            get { return literals; }
        }

#nullable disable warnings
        public PlanningProblem(HashSet<Operator> operators, List<Literal> initialState, List<Literal> goalState)
        {
            ThrowIfNull(operators, nameof(operators));
            ThrowIfNull(initialState, nameof(initialState));
            ThrowIfNull(goalState, nameof(goalState));

            this.operators = operators;
            this.initialState = initialState;
            this.goalState = goalState;

            foreach (Operator op in operators)
            {
                foreach (Literal l in op.Effects)
                {
                    if (!literals.Contains(l))
                    {
                        literals.Add(new Literal(l));
                    }
                }
                foreach (Literal l in op.Preconditions)
                {
                    if (!literals.Contains(l))
                    {
                        literals.Add(new Literal(l));
                    }
                }
            }

            foreach (Literal l in initialState)
            {
                if (!literals.Contains(l))
                {
                    literals.Add(new Literal(l));
                }
            }
            foreach (Literal l in goalState)
            {
                if (!literals.Contains(l))
                {
                    literals.Add(new Literal(l));
                }
            }
        }

#nullable restore warnings

        public List<Operator> GetListOfAchievers(Literal l)
        {
            List<Operator> achievers = new List<Operator>();
            // check if the literal is in the effects of the operator
            foreach (Operator op in operators)
            {
                foreach (Literal effect in op.Effects)
                {
                    if (effect.Name == l.Name && effect.IsPositive == l.IsPositive
                        && effect.Variables.Length == l.Variables.Length)
                    {
                        achievers.Add(op);
                        break;
                    }
                }
            }
            return achievers;
        }

        public override string ToString()
        {
            string str = "Operators:\n";
            foreach (Operator op in operators)
            {
                str += op.ToString() + "\n";
            }
            str += "Initial State:\n";
            foreach (Literal l in initialState)
            {
                str += l.ToString() + "\n";
            }
            str += "Goal State:\n";
            foreach (Literal l in goalState)
            {
                str += l.ToString() + "\n";
            }
            return str;
        }
    }
}