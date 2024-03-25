
namespace POP
{
    using System;
    using System.Collections.Generic;
    using static System.ArgumentNullException;

    public class Helpers
    {
        public static Dictionary<Expression, List<Expression>>? Unify(Literal l1, Literal l2, List<BindingConstraint> bindingConstraints)
        {
            return Unify(Expression.Expressionize(l1, bindingConstraints), Expression.Expressionize(l2, bindingConstraints));
        }
        public static Dictionary<Expression, List<Expression>>? Unify(Expression e1, Expression e2)
        {
            return Unify(e1, e2, []);

        }
        public static Dictionary<Expression, List<Expression>>? Unify(Expression? e1, Expression? e2, Dictionary<Expression, List<Expression>>? μ)
        {
            if (e1 is null || e2 is null)
                return null; // failure
            if (μ is null)
                return null; // failure
            if (e1.Equals(e2))
                return μ;
            if (e1.IsVariable)
                return UnifyVar(e1, e2, μ);
            if (e2.IsVariable)
                return UnifyVar(e2, e1, μ);
            if (e1.IsConstant || e2.IsConstant) // if ATOM(e1) or ATOM(e2)
                return null; // failure
            if (!e1.Name.Equals(e2.Name) || e1.Arguments?.Count != e2.Arguments?.Count)
                return null; // failure
            if (e1.Arguments != null && e2.Arguments != null)
            {
                for (int i = 0; i < e1.Arguments.Count && i < e2.Arguments.Count; i++)
                {
                    μ = Unify(e1.Arguments[i], e2.Arguments[i], μ);
                    if (μ is null)
                        return null; // failure
                }
            }
            return μ;
        }

        public static Dictionary<Expression, List<Expression>>? UnifyVar(Expression xvar, Expression e, Dictionary<Expression, List<Expression>> μ)
        {
            if (μ.ContainsKey(xvar))
                // choose the first binding
                for (int i = 0; i < μ[xvar].Count; i++)
                    if (!μ[xvar][i].Equals(xvar))
                        return Unify(μ[xvar][i], e, μ);

            Expression t = Subst(μ, e);
            if (t.Contains(xvar))
                return null; // failure

            // compose {t/xvar} with μ <==> {t/xvar} ∘ μ

            μ[(Expression)xvar.Clone()] = [t];
            // create a new dictionary and add t/xvar
            Dictionary<Expression, List<Expression>> μ2 = new() { [xvar] = [t] };
            // update each occurrence of xvar in μ values to t
            foreach (Expression key in μ.Keys)
            {
                for (int i = 0; i < μ[key].Count; i++)
                {
                    μ[key][i] = Subst(μ2, μ[key][i]);
                }
            }
            return μ;
        }

        private static Expression Subst(Dictionary<Expression, List<Expression>> μ, Expression e)
        {
            if (e.IsVariable && μ.ContainsKey(e))
                return μ[e][0];
            if (e.IsPredicate && e.Arguments != null)
            {
                List<Expression> args = [];
                foreach (Expression arg in e.Arguments)
                {
                    args.Add(Subst(μ, arg));
                }
                return new Expression(e.Name, args);
            }
            return e;
        }



    }

    public class Expression : IEquatable<Expression>, ICloneable
    {
        public string Name { get; set; }
        public List<Expression>? Arguments { get; set; }

        private bool isVariable, isConstant;
        public bool IsPredicate => Arguments != null;
        public bool IsConstant
        {
            get { return isConstant; }
            set { isConstant = value && !IsPredicate; isVariable = !value && !IsPredicate; }
        }
        public bool IsVariable
        {
            get { return isVariable; }
            set { isVariable = value && !IsPredicate; isConstant = !value && !IsPredicate; }
        }

        public Expression(string name, List<Expression>? arguments, bool isConstant = false)
        {
            this.Name = name;
            this.Arguments = arguments;
            this.IsConstant = isConstant;
        }

        public static bool operator ==(Expression? left, Expression? right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(Expression? left, Expression? right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(Expression? left, object right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(Expression? left, object right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(object left, Expression? right) { return right is null ? (left is null && right is null) : right.Equals(left); }
        public static bool operator !=(object left, Expression? right) { return !(right is null ? (left is null && right is null) : right.Equals(left)); }


        public override bool Equals(object? obj)
        {
            if (obj is not Expression) return false;
            return this.Equals((Expression)obj);
        }

        public bool Equals(Expression? other)
        {
            if (other is null)
                return false;
            if (this.Name != other.Name)
                return false;
            if (this.Arguments is null && other.Arguments is null)
                return true;
            if (this.Arguments is null || other.Arguments is null)
                return false;
            if (this.Arguments.Count != other.Arguments.Count)
                return false;
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                if (!this.Arguments[i].Equals(other.Arguments[i]))
                    return false;
            }
            return true;
        }

        public bool Contains(Expression e)
        {
            if (this.Equals(e))
                return true;
            if (this.Arguments is null)
                return false;
            foreach (Expression arg in this.Arguments)
            {
                if (arg.Contains(e))
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            if (this.Arguments is null)
                return this.Name;
            string str = this.Name + "(";
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                str += this.Arguments[i].ToString();
                str += (i < this.Arguments.Count - 1) ? ", " : "";
            }
            str += ")";
            return str;
        }

        public object Clone()
        {
            List<Expression>? args = null;
            if (this.Arguments != null)
            {
                args = new List<Expression>();
                foreach (Expression arg in this.Arguments)
                {
                    args.Add((Expression)arg.Clone());
                }
            }
            return new Expression((string)this.Name.Clone(), args, this.IsConstant);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Arguments, Arguments is null ? 0 : Arguments.Count, IsConstant);
        }

        public static Expression Expressionize(Literal l, List<BindingConstraint> bindingConstraints)
        {
            List<Expression>? args = new List<Expression>();
            if (l.Variables != null)
            {
                foreach (string var in l.Variables)
                {
                    bool foundInConstraints = false;
                    if (bindingConstraints != null)
                    {
                        foreach (BindingConstraint bc in bindingConstraints)
                        {
                            if (bc.Variable.Equals(var))
                            {
                                args.Add(new Expression(bc.Bounds[0], null, true)); // add the first bound only (TODO: add all bounds or find a better way to handle this)
                                foundInConstraints = true;
                                break;
                            }
                        }
                    }
                    if (!foundInConstraints)
                        args.Add(new Expression(var, null, var[0] >= 'A' && var[0] <= 'Z'));
                }
            }
            return new Expression(l.Name, args);
        }
    }
    public class Graph
    {
        private Dictionary<POP.Action, List<POP.Action>> adjList;

        public Graph()
        {
            adjList = new Dictionary<POP.Action, List<POP.Action>>();
        }

        public Graph(HashSet<Tuple<POP.Action, POP.Action>> orderingConstraints)
        : this() { InitializeGraph(orderingConstraints); }

        public void InitializeGraph(HashSet<Tuple<POP.Action, POP.Action>> orderingConstraints)
        {
            foreach (Tuple<POP.Action, POP.Action> tuple in orderingConstraints)
            {
                AddEdge(tuple.Item1, tuple.Item2);
            }
        }

        public void AddEdge(Action u, Action v)
        {
            if (!adjList.ContainsKey(u))
                adjList[u] = new List<Action>();
            adjList[u].Add(v);
        }

        private bool IsCyclicUtil(Action v, HashSet<Action> visited, HashSet<Action> recStack)
        {
            if (!visited.Contains(v))
            {
                visited.Add(v);
                recStack.Add(v);

                if (adjList.ContainsKey(v))
                {
                    foreach (Action neighbour in adjList[v])
                    {
                        if (!visited.Contains(neighbour) && IsCyclicUtil(neighbour, visited, recStack))
                            return true;
                        else if (recStack.Contains(neighbour))
                            return true;
                    }
                }
            }

            recStack.Remove(v);
            return false;
        }

        public bool IsCyclic()
        {
            HashSet<Action> visited = new HashSet<Action>();
            HashSet<Action> recStack = new HashSet<Action>();

            foreach (var pair in adjList)
            {
                Action node = pair.Key;
                if (IsCyclicUtil(node, visited, recStack))
                    return true;
            }

            return false;
        }
        public static void printTest()
        {

            // test acyclicity graph
            Graph graph = new();
            HashSet<Tuple<Action, Action>> orderingConstraints = [];
            Action a = new("A", [], [], ["x"]);
            Action b = new("B", [], [], ["x"]);
            Action c = new("C", [], [], ["x"]);
            Action d = new("D", [], [], ["x"]);
            Action e = new("E", [], [], ["x"]);


            orderingConstraints.Add(new(a, b));
            orderingConstraints.Add(new(b, c));
            orderingConstraints.Add(new(c, d));
            orderingConstraints.Add(new(d, c));
            orderingConstraints.Add(new(d, e));

            foreach (Tuple<Action, Action> orderingConstraint in orderingConstraints)
            {
                Console.WriteLine(orderingConstraint.Item1 + " ≺ " + orderingConstraint.Item2);
            }

            graph.InitializeGraph(orderingConstraints);
            if (!graph.IsCyclic())
            {
                Console.WriteLine("\nGraph is acyclic");
            }
            else
            {
                Console.WriteLine("\nGraph is cyclic");
            }

            Console.WriteLine(graph);
        }

        public override string ToString()
        {
            string str = "";
            foreach (Action key in adjList.Keys)
            {
                str += key + " -> ";
                foreach (Action value in adjList[key])
                {
                    str += value + ", ";
                }
                str += "\n";
            }
            return str;
        }
    }
}

