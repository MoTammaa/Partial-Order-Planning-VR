
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
        }

        public PartialPlan POP()
        {
            if (agenda == null || agenda.Count == 0)  // If the agenda is empty ∅, return the current plan 
                return plan; // π




        }




    }
}