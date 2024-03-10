
namespace POP
{
    using System;
    using System.Collections.Generic;
    using static System.ArgumentNullException;
    using Agenda = System.Collections.Generic.List<System.Tuple<POP.Action, POP.Literal>>;

    public class Planner
    {

        private PlanningProblem problem;
        private PartialPlan plan;
        private Agenda agenda = new Agenda();

        public PlanningProblem Problem
        {
            get { return problem; }
            set { problem = value; }
        }

        public Planner(PlanningProblem problem)
        {
            ThrowIfNull(problem, nameof(problem));

            this.problem = problem;

            // Initialize the plan
            Action start = new Action("Start", problem.InitialState, new List<Literal>());
            Action end = new Action("End", new List<Literal>(), problem.GoalState);
            this.plan = new PartialPlan(
                new List<Action> { start, end },// {aₒ, a∞}
                new List<CausalLink>(),         // ∅ or {}
                new List<BindingConstraint>(),  // ∅ or {}
                new List<Tuple<Action, Action>> { new Tuple<Action, Action>(start, end) } // {aₒ ≺ a∞}
            );

            // Initialize the agenda ==> {a∞} x Preconds(a∞)
            foreach (Literal goalPrecondition in problem.GoalState)
            {
                agenda.Add(new Tuple<Action, Literal>(end, goalPrecondition));
            }
        }


    }
}