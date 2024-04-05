
namespace POP
{
    using System;
    using System.Collections.Generic;
    using static System.ArgumentNullException;
    using static Helpers;

    public class Planner
    {
        public static bool PRINT_START_FINISH_ORDERINGS = false, PRINT_AFTER_CONVERTING_VARIABLES = true,
            PRINT_DEBUG_INFO = false;


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

            // Initialize the plan
            Action start = new Action("Start", problem.InitialState, new List<Literal>(), []);
            Action finish = new Action("Finish", new List<Literal>(), problem.GoalState, []);
            this.plan = new PartialPlan(
                new HashSet<Action> { start, finish }, // {aₒ, a∞}
                new HashSet<CausalLink>(),             // ∅ or {}
                new BindingConstraints(),      // ∅ or {}
                new HashSet<Tuple<Action, Action>> { new Tuple<Action, Action>(start, finish) } // {aₒ ≺ a∞}
            );

            this.agenda = new Agenda(problem, plan);

            // Initialize the agenda ==> {a∞} x Preconds(a∞)
            foreach (Literal goalPrecondition in problem.GoalState)
            {
                agenda.Add(new(finish, goalPrecondition));
            }

            // Initialize the variable prefixes and counters to all operators and literals
            for (int i = 0; i < variableCounter.Length; i++) variableCounter[i] = 0;
        }

        public PartialPlan? POP()
        {
            if (agenda == null || agenda.Count == 0)  // If the agenda is empty ∅, return the current plan 
                return plan; // π

            /*  
            *   For this function and all non-deterministic searches here, we will implement A* search Algorithm, 
            *   where each node in the queue will contain a parital plan
            */
            // Node => (PartialPlan, Agenda, pathCost, parent)

            // Create a new PriorityQueue to store the partial plans with the cost of the path from the start node (root) to the current node -> (partial plan, agenda, path cost, parent node)
            PriorityQueue<Node, int> aStarQueue = new();

            // Use the initial plan as the root node
            Node root = new Node(plan, agenda, 0, null);
            aStarQueue.Enqueue(root, Eval_Fn(root));

            Console.Write("Searching");

            while (aStarQueue.Count > 0)
            {
                Console.Write(".");
                Helpers.println("============================\nSearching...\n===========================\n");
                Node current = aStarQueue.Dequeue();


                // Check if the current plan DAG is cyclic
                if (current.partialPlan.OrderingConstraints.Count > 0)
                {
                    Graph<Action> graph = new Graph<Action>();
                    graph.InitializeGraph(current.partialPlan.OrderingConstraints);

                    if (graph.IsCyclic()) continue; // skip the current node if the plan DAG is cyclic
                }

                // Check if the current node is a goal node
                if (current.agenda.Count == 0) // if the agenda is empty
                    return current.partialPlan; // return the partial plan


                EXPAND(current, aStarQueue);

            }
            return null;
        }


        public Node createNode(Node? n = null, PartialPlan? plan = null, Agenda? agenda = null, int pathCost = 0)
        {
            if (n is not null)
            {
                return new Node((PartialPlan)n.partialPlan.Clone(), (Agenda)n.agenda.Clone(), n.pathCost + 1, n);
            }
            if (plan is null || agenda is null)
                return new Node(this.plan, this.agenda, 0, null);
            return new Node(plan, agenda, pathCost, null);
        }

        private int Eval_Fn(Node node)
        {
            /* 
             *      f(n) = h(n) + g(n)
             *      h(n) = open preconditions of the partial plan (inside the agenda) 
             *      g(n) = path cost from the start node to the current node
            */
            return node.pathCost + node.agenda.Count;
        }


        private void EXPAND(Node current, PriorityQueue<Node, int> aStarQueue)
        {
            // Expand the current node by applying each of the achievers to the current node

            // First, select any Pair (a, p) from the agenda (based on heuristic described in the Agenda class)
            Helpers.println(current.agenda.ToString(current.partialPlan) + "\n");
            Tuple<Action, Literal> chosenAgendaPair = current.agenda.Remove();
            Helpers.println("--------\nCurrent Plan:\n" + current.partialPlan.ToString() + "\n");
            Helpers.println($"****\nSelected Action: {current.partialPlan.ActionToString(chosenAgendaPair.Item1)} ,,,\n Literal: {current.partialPlan.LiteralToString(chosenAgendaPair.Item2)}\n-----------------\n");


            // Find the list of achievers for the selected literal p
            // If the list of achievers is empty, the Agenda class will detect it and throw an exception, indicating that the problem is unsolvable
            List<Operator> achievers =
            [
                .. current.partialPlan.getListOfActionsAchievers(chosenAgendaPair.Item2, chosenAgendaPair.Item1),
                .. problem.GetListOfAchievers(chosenAgendaPair.Item2),
            ];

            // Apply each of the achievers to the current node
            foreach (Operator achiever in achievers)
            {
                // clone the current plan and agenda
                Node newNode = createNode(current);

                // Add the new action to the new plan
                Action? newAction = null;
                CausalLink? newCausalLink = null;
                bool appliedSuccessfully = ApplyAchiever(achiever, chosenAgendaPair, newNode, ref newAction, ref newCausalLink);

                if (!appliedSuccessfully) continue; // skip the current achiever if it couldn't be applied

                // check for threats
                if (newAction is null || newCausalLink is null) continue;

                // if there is a threat, resolve it and push the nodes with the new plans to the queue
                searchResolveThreats(newAction, newCausalLink, aStarQueue, newNode);
            }
        }


        private bool ApplyAchiever(Operator achiever, Tuple<Action, Literal> agendaActionLiteralPair, Node node, ref Action? newActionToReturn, ref CausalLink? newCausalLinkToReturn)
        {
            // true if the action is added to the plan, false if couldn't satisfy binding constraints or otherwise
            PartialPlan plan = node.partialPlan;
            Agenda agenda = node.agenda;

            // Create a new action from the operator
            Action newAction = (achiever is Action action) ? action : createAction(achiever, node.partialPlan.BindingConstraints);
            if (newAction.Name == "") return false;
            newActionToReturn = newAction;

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
            CausalLink newCausalLink = new CausalLink(newAction, agendaActionLiteralPair.Item2, agendaActionLiteralPair.Item1);
            plan.CausalLinks.Add(newCausalLink);
            newCausalLinkToReturn = newCausalLink;

            // Update the plan's ordering constraints (≺)
            plan.OrderingConstraints.Add(new Tuple<Action, Action>(newAction, agendaActionLiteralPair.Item1));

            // Update the plan's binding constraints (B)
            bool successfullyBinded = false;
            foreach (Literal effect in newAction.Effects)
            {
                if (effect.Name == agendaActionLiteralPair.Item2.Name
                && effect.IsPositive == agendaActionLiteralPair.Item2.IsPositive
                && effect.Variables.Length == agendaActionLiteralPair.Item2.Variables.Length)
                {

                    Dictionary<Expression, List<Expression>>? μ = Helpers.Unify(effect, agendaActionLiteralPair.Item2, plan.BindingConstraints);
                    if (μ is not null)
                    {
                        successfullyBinded = true;
                        foreach (KeyValuePair<Expression, List<Expression>> entry in μ)
                        {
                            // let bound be the first constant in the list of expressions or the first variable if there is no constant
                            string bound = entry.Value[0].Name;
                            foreach (Expression e in entry.Value)
                            {
                                if (e.IsConstant)
                                {
                                    bound = e.Name;
                                    break;
                                }
                            }
                            // check if the variable is already in the binding constraints
                            string? boundEq = plan.BindingConstraints.getBoundEq(entry.Key.Name);
                            if (boundEq is not null && boundEq != bound)
                            {
                                if (IsUpper(boundEq[0]) && IsUpper(bound[0]))
                                    return false;
                            }
                            // add the new binding constraint to the plan
                            bool successful = plan.BindingConstraints.setEqual(entry.Key.Name, bound);
                            if (!successful) return false;

                        }
                        break;
                    }
                }
            }
            if (agendaActionLiteralPair.Item1.hasConflictingPreconditionsOrEffects(plan.BindingConstraints))
                return false;

            return successfullyBinded;
        }

        public Action createAction(Operator op, BindingConstraints bindingConstraints)
        {
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

            // Before creating the new literals of preconditions and effects, let's add the binded constants from the binding constraints
            foreach (string vr in bindingConstraints.Variables)
            {
                string? bound = bindingConstraints.getBoundEq(vr);
                if (bound is not null && IsUpper(bound[0]))
                {
                    boundedVariablesSoFar.TryAdd(vr, bound);
                }
            }

            // Create the preconditions and effects of the new action
            foreach (Literal l in op.Preconditions)
            {
                Literal newPreCondLiteral = createLiteral(l, boundedVariablesSoFar, bindingConstraints);
                if (newPreCondLiteral.Name == "") return new Action("", [], [], []);

                preconditions.Add(newPreCondLiteral);
            }
            foreach (Literal l in op.Effects)
            {
                Literal newEffectLiteral = createLiteral(l, boundedVariablesSoFar, bindingConstraints);
                if (newEffectLiteral.Name == "") return new Action("", [], [], []);

                effects.Add(newEffectLiteral);
            }

            // Create the variables of the new action
            string[] variables = new string[op.Variables.Length];
            variables = op.Variables.Select(v => boundedVariablesSoFar[v]).ToArray(); // convert the new bounded variables to an array

            // Create the new action
            return new Action(op.Name, effects, preconditions, variables);
        }

        private Literal createLiteral(Literal l, Dictionary<string, string> boundedVariablesSoFar, BindingConstraints bindingConstraints)
        {
            string varPrefix = l.Name.ToLower()[0].ToString();
            int count = variableCounter[l.Name.ToLower()[0] - 'a'];
            bool usedCount = false;

            string[] variables = new string[l.Variables.Length];
            bool[] isAlreadyBound = new bool[l.Variables.Length];

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
                    isAlreadyBound[i] = true;
                }
            }

            // if the variable was originally a constant, then we should add it to the binding constraints
            for (int i = 0; i < l.Variables.Length; i++)
            {
                if (IsUpper(l.Variables[i][0]) && !isAlreadyBound[i])
                {
                    // bindingConstraints.Add(new BindingConstraint(variables[i], l.Variables[i], true));
                    bool successful = bindingConstraints.setEqual(variables[i], l.Variables[i]);
                    if (!successful) return new Literal("", [], false);
                }
            }

            // increment the variable counter if the count was used
            if (usedCount) variableCounter[l.Name.ToLower()[0] - 'a']++;
            return new Literal(l.Name, variables, l.IsPositive);
        }

        public void searchResolveThreats(Action action, CausalLink causalLink, PriorityQueue<Node, int> aStarQueue, Node node)
        {
            Queue<Node> nodes = new Queue<Node>();
            nodes.Enqueue(node);
            while (nodes.Count > 0)
            {
                Node current = nodes.Dequeue();


                CausalLink threatenedLink;
                bool threatFound = false;
                foreach (CausalLink cl in current.partialPlan.CausalLinks)
                {
                    if (action.hasPossibleNegatedEffectOf(cl.LinkCondition))
                    {
                        threatenedLink = cl;

                        // check if the action is a threat to the causal link
                        if (isThreat(action, threatenedLink, current))
                        {
                            Helpers.println($"***\nThreatened Link: {current.partialPlan.ActionToString(threatenedLink.Produceri)} --{current.partialPlan.LiteralToString(threatenedLink.LinkCondition)}--> {current.partialPlan.ActionToString(threatenedLink.Consumerj)} Action: {current.partialPlan.ActionToString(action)} \n****\n");

                            // try promotion of action
                            Node promotedNode = createNode(current);
                            promotedNode.partialPlan.OrderingConstraints.Add(new Tuple<Action, Action>(threatenedLink.Consumerj, action));
                            if (threatenedLink.Consumerj != new Action("Finish", new List<Literal>(), problem.GoalState, []))
                            {
                                // before queuing the promoted node, check if there is a threat to the new causal link
                                if (!searchResolveThreatsForNewCausalLink(causalLink, nodes, promotedNode))
                                    nodes.Enqueue(promotedNode);
                            }

                            // try demotion of the causal link
                            Node demotedNode = createNode(current);
                            demotedNode.partialPlan.OrderingConstraints.Add(new Tuple<Action, Action>(action, threatenedLink.Produceri));
                            if (threatenedLink.Produceri != new Action("Start", problem.InitialState, new List<Literal>(), []))
                            {
                                // before queuing the demoted node, check if there is a threat to the new causal link
                                if (!searchResolveThreatsForNewCausalLink(causalLink, nodes, demotedNode))
                                    nodes.Enqueue(demotedNode);
                            }

                            // try adding new Binding Constraints
                            // Node newBindingNode = createNode(current);
                            // bool noconflict = true;
                            // foreach (Literal effect in action.Effects)
                            // {
                            //     if (effect.Name == threatenedLink.LinkCondition.Name
                            //     && effect.IsPositive == !threatenedLink.LinkCondition.IsPositive
                            //     && effect.Variables.Length == threatenedLink.LinkCondition.Variables.Length)
                            //     {
                            //         bool successful = newBindingNode.partialPlan.BindingConstraints.setNotEqual(effect.Variables[0], threatenedLink.LinkCondition.Variables[0]);
                            //         noconflict = noconflict && successful;
                            //         if (!successful) break;
                            //     }

                            // }
                            // if (noconflict) 
                            // {
                            //      //before queuing the new binding node, check if there is a threat to the new causal link
                            //      if (!searchResolveThreatsForNewCausalLink(causalLink, nodes, newBindingNode))
                            //      nodes.Enqueue(newBindingNode);
                            // }

                            threatFound = true;
                        }
                    }
                }
                if (!threatFound)
                {
                    // check if there is a threat to the new causal link
                    threatFound = threatFound || searchResolveThreatsForNewCausalLink(causalLink, nodes, current);
                }
                if (!threatFound) aStarQueue.Enqueue(current, Eval_Fn(current));
            }
        }

        public bool searchResolveThreatsForNewCausalLink(CausalLink causalLink, Queue<Node> queue, Node current)
        {
            Action threateningAction;
            bool threatFound = false;
            foreach (Action a in current.partialPlan.Actions)
            {
                if (a.hasPossibleNegatedEffectOf(causalLink.LinkCondition))
                {
                    threateningAction = a;

                    // check if the action is a threat to the new causal link
                    if (isThreat(threateningAction, causalLink, current))
                    {
                        Helpers.println($"***\n Action: {current.partialPlan.ActionToString(threateningAction)},\nThreatened Link: {current.partialPlan.ActionToString(causalLink.Produceri)} --{current.partialPlan.LiteralToString(causalLink.LinkCondition)}--> {current.partialPlan.ActionToString(causalLink.Consumerj)} \n****\n");

                        // try promotion of action
                        Node promotedNode = createNode(current);
                        promotedNode.partialPlan.OrderingConstraints.Add(new Tuple<Action, Action>(causalLink.Consumerj, threateningAction));
                        if (causalLink.Consumerj != new Action("Finish", new List<Literal>(), problem.GoalState, [])
                                    && threateningAction != new Action("Start", problem.InitialState, new List<Literal>(), []))
                            queue.Enqueue(promotedNode);


                        // try demotion of the causal link
                        Node demotedNode = createNode(current);
                        demotedNode.partialPlan.OrderingConstraints.Add(new Tuple<Action, Action>(threateningAction, causalLink.Produceri));
                        if (causalLink.Produceri != new Action("Start", problem.InitialState, new List<Literal>(), []))
                            queue.Enqueue(demotedNode);


                        // try adding new Binding Constraints
                        // Node newBindingNode = createNode(current);
                        // bool noconflict = true;
                        // foreach (Literal effect in threateningAction.Effects)
                        // {
                        //     if (effect.Name == causalLink.LinkCondition.Name
                        //     && effect.IsPositive == !causalLink.LinkCondition.IsPositive
                        //     && effect.Variables.Length == causalLink.LinkCondition.Variables.Length)
                        //     {
                        //         bool successful = newBindingNode.partialPlan.BindingConstraints.setNotEqual(effect.Variables[0], causalLink.LinkCondition.Variables[0]);
                        //         noconflict = noconflict && successful;
                        //         if (!successful) break;
                        //     }

                        // }
                        // if (noconflict)
                        // {queue.Enqueue(newBindingNode, Eval_Fn(newBindingNode));}
                        threatFound = true;
                    }
                }
            }
            return threatFound;
        }


        public bool isThreat(Action a, CausalLink cl, Node current)
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

            PartialPlan plan = current.partialPlan;

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
                            string? boundEq = plan.BindingConstraints.getBoundEq(entry.Key.Name);
                            // get first constant in the list of expressions or the first variable if there is no constant
                            string bound = entry.Value[0].Name;
                            foreach (Expression e in entry.Value)
                            {
                                if (e.IsConstant)
                                {
                                    bound = e.Name;
                                    break;
                                }
                            }
                            // check if the variable is already in the binding constraints and is bound to a different constant
                            // (the variable is bound to x and the new binding is to y, and x is constant or y is constant, then it is false not a threat)
                            if (boundEq is not null && boundEq != bound && IsUpper(boundEq[0]) && IsUpper(bound[0]))
                            {
                                return false;
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