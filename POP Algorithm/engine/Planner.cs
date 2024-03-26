
namespace POP
{
    using System;
    using System.Collections.Generic;
    using static System.ArgumentNullException;

    using Node = Tuple<PartialPlan, Agenda, int>;

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
            Action start = new Action("Start", problem.InitialState, new List<Literal>(), []);
            Action finish = new Action("Finish", new List<Literal>(), problem.GoalState, []);
            this.plan = new PartialPlan(
                new HashSet<Action> { start, finish }, // {aₒ, a∞}
                new HashSet<CausalLink>(),             // ∅ or {}
                new List<BindingConstraint>(),      // ∅ or {}
                new HashSet<Tuple<Action, Action>> { new Tuple<Action, Action>(start, finish) } // {aₒ ≺ a∞}
            );

            // Initialize the agenda ==> {a∞} x Preconds(a∞)
            foreach (Literal goalPrecondition in problem.GoalState)
            {
                agenda.Add(new(finish, goalPrecondition));
            }

            // Initialize the variable prefixes and counters to all operators and literals
        }

        public PartialPlan? POP()
        {
            if (agenda == null || agenda.Count == 0)  // If the agenda is empty ∅, return the current plan 
                return plan; // π

            // Select any Pair (a, p) from the agenda (based on heuristic described in the Agenda class)
            // Tuple<Action, Literal> pair = agenda.Remove();

            // // Find the list of achievers for the selected literal p
            // // If the list of achievers is empty, the Agenda class will detect it and throw an exception, indicating that the problem is unsolvable
            // List<Operator> achievers = problem.GetListOfAchievers(pair.Item2);

            // // TODO: Non-deterministically select an operator a from the list of achievers
            // // for now, we just select randomly
            // Operator achiever = achievers[new Random().Next(achievers.Count)];

            return NonDetermenisticAchieverSearch();







            //return POP();

        }


        public Node createNode(Node? n = null, PartialPlan? plan = null, Agenda? agenda = null, int pathCost = 0)
        {
            if (n is not null)
            {
                return new Node((PartialPlan)n.Item1.Clone(), (Agenda)n.Item2.Clone(), n.Item3 + 1);
            }
            if (plan is null || agenda is null)
                return new Node(this.plan, this.agenda, 0);
            return new Node(plan, agenda, pathCost);
        }

        private int Eval_Fn(Node node)
        {
            /* 
             *      f(n) = h(n) + g(n)
             *      h(n) = open preconditions of the partial plan (inside the agenda) 
             *      g(n) = path cost from the start node to the current node
            */
            // Node => Item1: PartialPlan, Item2: Agenda, Item3: path cost integer
            return node.Item3 + node.Item2.Count;
        }


        private PartialPlan? NonDetermenisticAchieverSearch()
        {
            /* For this function and all non-deterministic searches here, we will implement A* search Algorithm, where each node in the queue will contain a parital plan */

            // using Node = Tuple<PartialPlan, Agenda, int>;

            // Create a new PriorityQueue to store the partial plans with the cost of the path from the start node (root) to the current node -> Pair (partial plan, path cost, agenda)
            PriorityQueue<Node, int> queue = new PriorityQueue<Node, int>();

            // Use the initial plan as the root node
            Node root = new Node(plan, agenda, 0);
            queue.Enqueue(root, Eval_Fn(root));

            while (queue.Count > 0)
            {
                Node current = queue.Dequeue();

                // Check if the current plan DAG is cyclic
                if (current.Item1.OrderingConstraints.Count > 0)
                {
                    Graph<Action> graph = new Graph<Action>();
                    graph.InitializeGraph(current.Item1.OrderingConstraints);
                    if (!graph.IsAcyclic())
                    {
                        continue; // skip the current node if the plan DAG is cyclic
                    }
                }

                // Check if the current node is a goal node
                if (current.Item2.Count == 0) // if the agenda is empty
                {
                    return current.Item1; // return the partial plan
                }

                // Expand the current node by applying each of the achievers to the current node
                // First, select any Pair (a, p) from the agenda (based on heuristic described in the Agenda class)
                Tuple<Action, Literal> chosenAgendaPair = current.Item2.Remove();

                // Find the list of achievers for the selected literal p
                // If the list of achievers is empty, the Agenda class will detect it and throw an exception, indicating that the problem is unsolvable
                List<Operator> achievers =
                [
                    .. plan.getListOfActionsAchievers(chosenAgendaPair.Item2),
                    .. problem.GetListOfAchievers(chosenAgendaPair.Item2),
                ];

                // Apply each of the achievers to the current node
                foreach (Operator achiever in achievers)
                {
                    // clone the current plan and agenda
                    Node newNode = createNode(current);

                    // Add the new action to the new plan
                    ApplyAchiever(achiever, chosenAgendaPair, newNode);

                    // return the new node if it is a goal node
                    if (newNode.Item2.Count == 0)
                        return newNode.Item1;

                    queue.Enqueue(newNode, Eval_Fn(newNode));


                }

                // check for threats
                searchResolveThreats(chosenAgendaPair.Item1, queue, current);


            }
            return null;


        }


        private void ApplyAchiever(Operator achiever, Tuple<Action, Literal> agendaActionLiteralPair, Node node)
        {
            // Node => Item1: PartialPlan, Item2: Agenda, Item3: path cost integer
            PartialPlan plan = node.Item1;
            Agenda agenda = node.Item2;

            // Create a new action from the operator
            Action newAction = (achiever is Action action) ? action : createAction(achiever);

            // Add the new action to the plan
            if (!plan.Actions.Contains(newAction))
            {
                plan.Actions.Add(newAction);

                // add ordering constraints that the new action is after the start action and before the finish action
                Action? start = plan.GetActionByName("Start"), finish = plan.GetActionByName("Finish");
                if (start is null || finish is null)
                    throw new Exception("Start or Finish action not found in the plan");

                plan.OrderingConstraints.Add(new Tuple<Action, Action>(start, newAction));
                plan.OrderingConstraints.Add(new Tuple<Action, Action>(newAction, finish));


                // add the new action preconditions to the agenda
                foreach (Literal precondition in newAction.Preconditions)
                {
                    agenda.Add(newAction, precondition);
                }
            }

            // Add the new action to the plan's causal links (L ∪ {C(A🇳 --Pⁱ--> Aⁱ})
            plan.CausalLinks.Add(new CausalLink(newAction, agendaActionLiteralPair.Item2, agendaActionLiteralPair.Item1));

            // Update the plan's ordering constraints (≺)
            plan.OrderingConstraints.Add(new Tuple<Action, Action>(newAction, agendaActionLiteralPair.Item1));

            // Update the plan's binding constraints (B)
            foreach (Literal effect in newAction.Effects)
            {
                if (effect.Name == agendaActionLiteralPair.Item2.Name
                && effect.IsPositive == agendaActionLiteralPair.Item2.IsPositive
                && effect.Variables.Length == agendaActionLiteralPair.Item2.Variables.Length)
                {

                    Dictionary<Expression, List<Expression>>? μ = Helpers.Unify(effect, agendaActionLiteralPair.Item2, plan.BindingConstraints);
                    if (μ != null)
                    {
                        foreach (KeyValuePair<Expression, List<Expression>> entry in μ)
                        {
                            bool found = false;
                            for (int i = 0; i < plan.BindingConstraints.Count; i++)
                            {
                                if (plan.BindingConstraints[i].Variable == entry.Key.Name && plan.BindingConstraints[i].IsEqBelong)
                                {
                                    plan.BindingConstraints[i].Bounds.AddRange(entry.Value.Select(e => e.Name));
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                plan.BindingConstraints.Add(new BindingConstraint(entry.Key.Name, entry.Value.Select(e => e.Name).ToList(), true));
                            }
                        }
                    }
                    break;
                }
            }
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

        public void searchResolveThreats(Action action, PriorityQueue<Node, int> queue, Node current)
        {
            CausalLink threatenedLink;
            foreach (CausalLink cl in current.Item1.CausalLinks)
            {
                if (action.Effects.Contains(cl.LinkCondition))
                {
                    threatenedLink = cl;

                    // check if the action is a threat to the causal link
                    if (isThreat(action, threatenedLink))
                    {
                        // try promotion of action
                        Node promotedNode = createNode(current);
                        promotedNode.Item1.OrderingConstraints.Add(new Tuple<Action, Action>(threatenedLink.Produceri, action));
                        queue.Enqueue(promotedNode, Eval_Fn(promotedNode));


                        // try demotion of the causal link
                        Node demotedNode = createNode(current);
                        demotedNode.Item1.OrderingConstraints.Add(new Tuple<Action, Action>(action, threatenedLink.Consumerj));
                        queue.Enqueue(demotedNode, Eval_Fn(demotedNode));


                        // try adding new Binding Constraints
                        Node newBindingNode = createNode(current);
                        foreach (Literal effect in action.Effects)
                        {
                            if (effect.Name == threatenedLink.LinkCondition.Name
                            && effect.IsPositive == !threatenedLink.LinkCondition.IsPositive
                            && effect.Variables.Length == threatenedLink.LinkCondition.Variables.Length)
                            {
                                newBindingNode.Item1.BindingConstraints.Add(new BindingConstraint(effect.Variables[0], [threatenedLink.LinkCondition.Variables[0]], false));
                            }

                        }
                        queue.Enqueue(newBindingNode, Eval_Fn(newBindingNode));
                    }
                }
            }
        }


        public bool isThreat(Action a, CausalLink cl)
        {
            /*  check if the action a is a threat to the causal link cl
            *   ak is a threat to cl if:
            *       1- ek unifies with pij, where ~ek ∈ effects(ak);
            *       2- the MGU of ek and pij is consistent with B; and
            *       3- (≺) U {ai ≺ ak, ak ≺ aj} is consistent.
            *       4- ak is not the same as the action that is supposed to achieve the effect of cl
            */
            //// 3 can be checked by checking if the ordering constraints are acyclic


            // check if the action a is the same as the action that is supposed to achieve the effect of cl
            if (a.Equals(cl.Produceri))
                return false;

            bool canUnify = false;
            // check if the effect of a unifies with the negative effect of cl
            foreach (Literal effect in a.Effects)
            {
                if (effect.Name == cl.LinkCondition.Name
                && effect.IsPositive == !cl.LinkCondition.IsPositive
                && effect.Variables.Length == cl.LinkCondition.Variables.Length)
                {
                    Dictionary<Expression, List<Expression>>? μ = Helpers.Unify(effect, cl.LinkCondition, plan.BindingConstraints);
                    if (μ != null)
                    {
                        canUnify = true;
                        // check if the MGU of effect and cl.LinkCondition is consistent with B
                        foreach (KeyValuePair<Expression, List<Expression>> entry in μ)
                        {
                            for (int i = 0; i < plan.BindingConstraints.Count; i++)
                            {
                                if (plan.BindingConstraints[i].Variable == entry.Key.Name && !plan.BindingConstraints[i].IsEqBelong)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            if (!canUnify)
                return false;

            // check if the ordering constraints are acyclic when adding the new action a between causal link actions
            Graph<Action> graph = new Graph<Action>();
            graph.InitializeGraph(plan.OrderingConstraints);
            graph.AddEdge(cl.Produceri, a);
            graph.AddEdge(a, cl.Consumerj);

            return graph.IsAcyclic();
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