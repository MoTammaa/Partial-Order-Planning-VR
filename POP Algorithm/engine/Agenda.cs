
namespace POP
{
    using System.Collections.Generic;
    using static System.ArgumentNullException;

    public class Agenda : IComparer<System.Tuple<POP.Action, POP.Literal>>, ICloneable, IEquatable<Agenda>
    {
        private PlanningProblem problem;
        private PartialPlan? partialPlan;
        private PriorityQueue<System.Tuple<POP.Action, POP.Literal>, System.Tuple<POP.Action, POP.Literal>> priorityQueue;
        // Constructor
        public Agenda(PlanningProblem problem, PartialPlan? partialPlan = null)
        {
            ThrowIfNull(problem, nameof(problem));
            this.problem = problem;
            this.partialPlan = partialPlan;
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

            List<Action> xAchieversActions = partialPlan?.getListOfActionsAchievers(x.Item2) ?? [];
            List<Action> yAchieversActions = partialPlan?.getListOfActionsAchievers(y.Item2) ?? [];
            if (xAchievers.Count == 0 || yAchievers.Count == 0)
                if (xAchievers.Count == 0 && xAchieversActions.Count == 0 || yAchievers.Count == 0 && yAchieversActions.Count == 0)
                    throw new Exception("Literal " + (xAchievers.Count == 0 ? x.Item2 : y.Item2) + " is not achievable. Problem is unsolvable");

            if (xAchievers.Count.CompareTo(yAchievers.Count) != 0)
                return (xAchievers.Count + (x.Item2.IsPositive ? -2 : 0)).CompareTo(yAchievers.Count + (y.Item2.IsPositive ? -2 : 0));

            if (xAchieversActions.Count.CompareTo(yAchieversActions.Count) != 0)
                return (xAchievers.Count + (x.Item2.IsPositive ? -2 : 0)).CompareTo(yAchieversActions.Count + (y.Item2.IsPositive ? -2 : 0));

            // if list of achievers is the same, compare the number of preconditions for each operator (not searching each time for the open ones to speed up heuristic)
            return (x.Item1.Preconditions.Count + (x.Item2.IsPositive ? -2 : 0)).CompareTo(y.Item1.Preconditions.Count + (y.Item2.IsPositive ? -2 : 0));
        }

        public object Clone()
        {
            Agenda newAgenda = new Agenda(this.problem, this.partialPlan);
            PriorityQueue<System.Tuple<POP.Action, POP.Literal>, System.Tuple<POP.Action, POP.Literal>> this1 = new(newAgenda);
            try
            {
                while (this.Count > 0)
                {
                    Tuple<POP.Action, POP.Literal> item = this.Remove();
                    newAgenda.Add(new((Action)item.Item1.Clone(), (Literal)item.Item2.Clone()));
                    this1.Enqueue(item, item);
                }
            }
            finally
            {
                while (this1.Count > 0)
                    this.Add(this1.Dequeue());
            }
            return newAgenda;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Agenda);
        }
        public override int GetHashCode()
        {
            return priorityQueue.GetHashCode();
        }

        public bool Equals(Agenda? other)
        {
            if (other is null)
                return false;
            if (this.Count != other.Count)
                return false;

            PriorityQueue<System.Tuple<POP.Action, POP.Literal>, System.Tuple<POP.Action, POP.Literal>> this1 = new(), other1 = new();
            try
            {
                for (int i = 0; i < this.Count; i++)
                {
                    Tuple<POP.Action, POP.Literal> item = this.Remove();
                    this1.Enqueue(item, item);
                    Tuple<POP.Action, POP.Literal> otherItem = other.Remove();
                    other1.Enqueue(otherItem, otherItem);
                    if (!item.Equals(otherItem))
                        return false;
                }
            }
            finally
            {
                while (this1.Count > 0)
                    this.Add(this1.Dequeue());
                while (other1.Count > 0)
                    other.Add(other1.Dequeue());
            }
            return true;
        }

        public static bool operator ==(Agenda? left, Agenda? right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(Agenda? left, Agenda? right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(Agenda? left, object right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(Agenda? left, object right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(object left, Agenda? right) { return right is null ? (left is null && right is null) : right.Equals(left); }
        public static bool operator !=(object left, Agenda? right) { return !(right is null ? (left is null && right is null) : right.Equals(left)); }

        public override string ToString()
        {
            string str = "Agenda: ";
            PriorityQueue<System.Tuple<POP.Action, POP.Literal>, System.Tuple<POP.Action, POP.Literal>> this1 = new(this);
            try
            {
                while (this.Count > 0)
                {
                    Tuple<POP.Action, POP.Literal> item = this.Remove();
                    this1.Enqueue(item, item);
                    str += item.Item1 + " / " + item.Item2 + (this.Count > 0 ? ", " : "");
                }
            }
            finally
            {
                while (this1.Count > 0)
                    this.Add(this1.Dequeue());
            }
            return str;
        }
    }
}