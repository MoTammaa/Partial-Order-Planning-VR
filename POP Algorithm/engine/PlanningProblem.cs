
namespace POP
{
    using System;
    using System.Collections.Generic;
    using static System.ArgumentNullException;

    public class PlanningProblem
    {
        private List<Operator> operators;
        private List<Literal> initialState;
        private List<Literal> goalState;
        private List<Literal> literals = new List<Literal>();

        public List<Operator> Operators
        {
            get { return operators; }
            set { operators = value; }
        }
        public List<Literal> InitialState
        {
            get { return initialState; }
            set { initialState = value; }
        }
        public List<Literal> GoalState
        {
            get { return goalState; }
            set { goalState = value; }
        }

        public List<Literal> Literals
        {
            get { return literals; }
            set { literals = value; }
        }

#nullable disable warnings
        public PlanningProblem(List<Operator> operators, List<Literal> initialState, List<Literal> goalState)
        {
            ThrowIfNull(operators, nameof(operators));
            ThrowIfNull(initialState, nameof(initialState));
            ThrowIfNull(goalState, nameof(goalState));

            this.Operators = operators;
            this.InitialState = initialState;
            this.GoalState = goalState;

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
    }
}