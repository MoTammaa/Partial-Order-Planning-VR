
namespace POP
{
    using System.Collections.Generic;
    using static System.ArgumentNullException;

    public class Agenda : IComparer<System.Tuple<POP.Action, POP.Literal>>
    {
        private PlanningProblem problem;
        private PriorityQueue<System.Tuple<POP.Action, POP.Literal>, System.Tuple<POP.Action, POP.Literal>> priorityQueue;
        // Constructor
        public Agenda(PlanningProblem problem)
        {
            ThrowIfNull(problem, nameof(problem));
            this.problem = problem;
            this.priorityQueue = new PriorityQueue<System.Tuple<POP.Action, POP.Literal>, System.Tuple<POP.Action, POP.Literal>>(this);
        }

        public void Add(Tuple<POP.Action, POP.Literal> item)
        {
            priorityQueue.Enqueue(item, item);
        }
        public void Add(POP.Action action, POP.Literal literal)
        {
            Tuple<POP.Action, POP.Literal> item = new Tuple<POP.Action, POP.Literal>(action, literal);
            this.Add(item);
        }

        public Tuple<POP.Action, POP.Literal> Remove()
        {
            return priorityQueue.Dequeue();
        }
        public Tuple<POP.Action, POP.Literal> Peek()
        {
            return priorityQueue.Peek();
        }
        public int Count
        {
            get { return priorityQueue.Count; }
        }

        public int Compare(Tuple<Action, Literal>? x, Tuple<Action, Literal>? y)
        {

            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;


            // search and compare the list of achievers for this precondition or literal
            // if the list of achievers is empty, throw an exception to indicate that the literal is not achievable and the problem is unsolvable
            List<Operator> xAchievers = problem.GetListOfAchievers(x.Item2);
            List<Operator> yAchievers = problem.GetListOfAchievers(y.Item2);
            if (xAchievers.Count == 0 || yAchievers.Count == 0)
                throw new Exception("Literal " + (xAchievers.Count == 0 ? x.Item2 : y.Item2) + " is not achievable. Problem is unsolvable");

            if (xAchievers.Count.CompareTo(yAchievers.Count) != 0)
                return xAchievers.Count.CompareTo(yAchievers.Count);

            // if list of achievers is the same, compare the number of preconditions for each operator (not searching each time for the open ones to speed up heuristic)
            return x.Item1.Preconditions.Count.CompareTo(y.Item1.Preconditions.Count);
        }

    }
}