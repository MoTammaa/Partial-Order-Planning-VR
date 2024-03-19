
namespace POP
{
    using System;
    using System.Collections.Generic;
    using static System.ArgumentNullException;

    public class Planner
    {

        private PlanningProblem problem;
        private PartialPlan plan;
        private Agenda agenda;

        // this is used to keep track of the variables that are assigned to the operators & literals
        private int[] variableCounter = new int[26];

        public PlanningProblem Problem
        {
            get { return problem; }
            set { problem = value; }
        }

        public Planner(PlanningProblem problem)
        {
            ThrowIfNull(problem, nameof(problem));

            this.problem = problem;
            this.agenda = new Agenda(problem);

            // Initialize the plan
            Action start = new Action("Start", problem.InitialState, new List<Literal>());
            Action finish = new Action("Finish", new List<Literal>(), problem.GoalState);
            this.plan = new PartialPlan(
                new HashSet<Action> { start, finish }, // {aₒ, a∞}
                new HashSet<CausalLink>(),             // ∅ or {}
                new List<BindingConstraint>(),      // ∅ or {}
                new List<Tuple<Action, Action>> { new Tuple<Action, Action>(start, finish) } // {aₒ ≺ a∞}
            );

            // Initialize the agenda ==> {a∞} x Preconds(a∞)
            foreach (Literal goalPrecondition in problem.GoalState)
            {
                agenda.Add(new(finish, goalPrecondition));
            }

            // Initialize the variable prefixes and counters to all operators and literals
        }

        public PartialPlan POP()
        {
            if (agenda == null || agenda.Count == 0)  // If the agenda is empty ∅, return the current plan 
                return plan; // π

            // Select any Pair (a, p) from the agenda (based on heuristic described in the Agenda class)
            Tuple<Action, Literal> pair = agenda.Remove();

            // Find the list of achievers for the selected literal p
            // If the list of achievers is empty, the Agenda class will detect it and throw an exception, indicating that the problem is unsolvable
            List<Operator> achievers = problem.GetListOfAchievers(pair.Item2);

            // TODO: Non-deterministically select an operator a from the list of achievers
            // for now, we just select randomly
            Operator achiever = achievers[new Random().Next(achievers.Count)];







            return POP();

        }

        public Action createAction(Operator op)
        {
            //////////////
            ///// TODO: add the BindingConstraints to the set
            //////////////
            List<Literal> preconditions = new List<Literal>();
            List<Literal> effects = new List<Literal>();

            Dictionary<string, string> boundedVariablesSoFar = new Dictionary<string, string>();
            string varPrefix = op.Name.ToLower()[0].ToString();

            int count = variableCounter[op.Name.ToLower()[0] - 'a']++;

            for (int i = 0; i < op.Variables.Length; i++)
            {
                string var = op.Variables[i];
                string boundedVar = repeat(i + 1, varPrefix);
                boundedVariablesSoFar.Add(var, boundedVar + count.ToString());
            }

            // Create the preconditions and effects of the new action
            foreach (Literal l in op.Preconditions)
            {
                Literal newPreCondLiteral = createLiteral(l, boundedVariablesSoFar);

                preconditions.Add(newPreCondLiteral);
            }
            foreach (Literal l in op.Effects)
            {
                Literal newEffectLiteral = createLiteral(l, boundedVariablesSoFar);

                effects.Add(newEffectLiteral);
            }

            // Create the variables of the new action
            string[] variables = new string[op.Variables.Length];
            variables = op.Variables.Select(v => boundedVariablesSoFar[v]).ToArray(); // convert the new bounded variables to an array

            // Create the new action
            return new Action(op.Name, effects, preconditions, variables);
        }

        private Literal createLiteral(Literal l, Dictionary<string, string> boundedVariablesSoFar)
        {
            string varPrefix = l.Name.ToLower()[0].ToString();
            int count = variableCounter[l.Name.ToLower()[0] - 'a'];
            bool usedCount = false;

            string[] variables = new string[l.Variables.Length];
            for (int i = 0; i < l.Variables.Length; i++)
            {
                if (!boundedVariablesSoFar.TryGetValue(l.Variables[i], out string? value)) // this syntax is to avoid double lookup again in Dictionary in the else part
                {
                    boundedVariablesSoFar.Add(l.Variables[i], repeat(i + 1, varPrefix) + count.ToString());
                    variables[i] = repeat(i + 1, varPrefix) + count.ToString();
                    usedCount = true;
                }
                else
                {
                    variables[i] = value;
                }
            }

            if (usedCount) variableCounter[l.Name.ToLower()[0] - 'a']++;
            return new Literal(l.Name, variables, l.IsPositive);
        }

        private static string repeat(int n, string? s = null, char? c = null)
        {
            if (s != null)
                return string.Concat(Enumerable.Repeat(s, n));
            if (c != null)
                return new string((char)c, n);
            return "";
        }




    }
}