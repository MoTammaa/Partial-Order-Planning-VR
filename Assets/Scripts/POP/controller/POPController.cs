using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace POP
{
    public class POPController
    {
        private Node currentNode;
        public PriorityQ<Node, int> SearchQueue { get; }
        public Stack<Node> DFSQueue { get; }
        public Node CurrentNode { get { return currentNode; } }
        public SearchStrategy Strategy { get; }
        public int MaxDepth { get; }

        private Planner planner;

        public Planner Planner { get { return planner; } }

        public POPController(PlanningProblem problem, SearchStrategy strategy = SearchStrategy.AStar, int maxDepth = -1)
        {
            if (strategy == SearchStrategy.DFS) { DFSQueue = new Stack<Node>(); }
            else { SearchQueue = new PriorityQ<Node, int>(); }
            Strategy = strategy;
            planner = new Planner(problem, Strategy, maxDepth);
            MaxDepth = planner.MaxDepth;

            Node root = new Node(planner.PartialPlan, planner.Agenda, 0, null);
            if (Strategy == SearchStrategy.DFS) { DFSQueue.Push(root); }
            else { SearchQueue.Enqueue(root, planner.Eval_Fn(root)); }
        }

        public bool NextStep()
        {
            return NextStep(out _, out _, out _, out _);
        }

        public bool NextStep(out bool isThereDifferencesThanLastStep, out List<Tuple<Action, bool>> actionsDifference,
            out List<Tuple<CausalLink, bool>> causalLinksDifference, out List<Tuple<Tuple<Action, Action>, bool>> orderingConstraintsDifference)
        {
            isThereDifferencesThanLastStep = false;
            actionsDifference = new();
            causalLinksDifference = new();
            orderingConstraintsDifference = new();

            // false in this context means that there is a failure or the search queue is empty and we can't continue
            if (Strategy == SearchStrategy.DFS && DFSQueue.Count == 0) { return false; }
            if (Strategy != SearchStrategy.DFS && SearchQueue.Count == 0) { return false; }

            Node current = Strategy is SearchStrategy.DFS ? DFSQueue.Pop() : SearchQueue.Dequeue();
            if (current != null)
            {

                if (DifferenceBetweenPartialPlans(currentNode?.partialPlan, current.partialPlan, out actionsDifference, out causalLinksDifference, out orderingConstraintsDifference))
                {
                    // The partial plans are different
                    isThereDifferencesThanLastStep = true;

                    // print the differences
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Difference between partial plans:\n");
                    sb.Append("Actions:\n");
                    foreach (var actionDiff in actionsDifference)
                        sb.Append($"{(actionDiff.Item2 ? "+ " : "- ")}: {current.partialPlan.ActionToString(actionDiff.Item1)}\n");
                    sb.Append("\nCausal Links:\n");
                    foreach (var causalLinkDiff in causalLinksDifference)
                        sb.Append($"{(causalLinkDiff.Item2 ? "+ " : "- ")}: {current.partialPlan.CausalLinkToString(causalLinkDiff.Item1)}\n");
                    sb.Append("\nOrdering Constraints:\n");
                    foreach (var orderingConstraintDiff in orderingConstraintsDifference)
                        sb.Append($"{(orderingConstraintDiff.Item2 ? "+ " : "- ")}: {current.partialPlan.ActionToString(orderingConstraintDiff.Item1.Item1)} < {current.partialPlan.ActionToString(orderingConstraintDiff.Item1.Item2)}\n");

                    // Debug.Log(sb.ToString());
                }
            }
            currentNode = current.Clone() as Node;

            if (current.pathCost > MaxDepth) return true;

            // Check if the current plan DAG is cyclic
            if (current.partialPlan.OrderingConstraints.Count > 0)
            {
                Graph<Action> graph = new Graph<Action>();
                graph.InitializeGraph(current.partialPlan.OrderingConstraints);

                if (graph.IsCyclic()) return true; // skip the current node if the plan DAG is cyclic
            }

            // Check if the current node is a goal node
            if (planner.GoalTest(current)) return true;

            planner.EXPAND(current, SearchQueue, DFSQueue);
            return true;
        }

        public bool DifferenceBetweenPartialPlans(PartialPlan plan1, PartialPlan plan2
                    , out List<Tuple<Action, bool>> actionsDiff, out List<Tuple<CausalLink, bool>> causalLinksDiff, out List<Tuple<Tuple<Action, Action>, bool>> orderingConstraintsDiff)
        {
            // Each tuple contains the action and a boolean value that indicates if 
            //  the difference is an addition or a removal with respect to the 2nd plan.
            actionsDiff = new();
            causalLinksDiff = new();
            orderingConstraintsDiff = new();

            bool theSame = true;

            if (plan1 is null && plan2 is null) return false;
            if (plan1 is null || plan2 is null) return true;

            // Check if the actions are the same
            if (!plan1.Actions.SetEquals(plan2.Actions))
            {
                // additions (plan2 - plan1)
                actionsDiff = plan2.Actions.Except(plan1.Actions).Select(action => new Tuple<Action, bool>(action, true)).ToList();
                // removals (plan1 - plan2)
                actionsDiff.AddRange(plan1.Actions.Except(plan2.Actions).Select(action => new Tuple<Action, bool>(action, false)));
                theSame = false;
            }

            // Check if the causal links are the same
            if (!plan1.CausalLinks.SetEquals(plan2.CausalLinks))
            {
                // additions (plan2 - plan1)
                causalLinksDiff = plan2.CausalLinks.Except(plan1.CausalLinks).Select(link => new Tuple<CausalLink, bool>(link, true)).ToList();
                // removals (plan1 - plan2)
                causalLinksDiff.AddRange(plan1.CausalLinks.Except(plan2.CausalLinks).Select(link => new Tuple<CausalLink, bool>(link, false)));
                theSame = false;
            }

            // Check if the ordering constraints are the same
            if (!plan1.OrderingConstraints.SetEquals(plan2.OrderingConstraints))
            {
                // additions (plan2 - plan1)
                orderingConstraintsDiff = plan2.OrderingConstraints.Except(plan1.OrderingConstraints).Select(constraint => new Tuple<Tuple<Action, Action>, bool>(constraint, true)).ToList();
                // removals (plan1 - plan2)
                orderingConstraintsDiff.AddRange(plan1.OrderingConstraints.Except(plan2.OrderingConstraints).Select(constraint => new Tuple<Tuple<Action, Action>, bool>(constraint, false)));
                theSame = false;
            }

            // Check if the binding constraints are the same
            if (!plan1.BindingConstraints.Equals(plan2.BindingConstraints))
            {
                theSame = false;
            }

            return !theSame;
        }

        public bool GoalTest(Node node)
        {
            return planner.GoalTest(node);
        }



    }
}