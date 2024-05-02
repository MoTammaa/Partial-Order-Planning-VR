using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ForceDirectedGraph;
using POP;
using System;
using Unity.VisualScripting;
using System.Linq;

public class POPEngineDriverController : MonoBehaviour
{
    #region Fields/Properties
    /// <summary>
    /// The graph displaying the network.
    /// </summary>
    [SerializeField]
    [Tooltip("The graph displaying the network.")]
    private static GraphManager Graph;

    /// <summary>
    /// The Network that contains the graph data.
    /// </summary>
    private static ForceDirectedGraph.DataStructure.Network Network;

    /// <summary>
    /// POP Backend Controller Reference.
    /// </summary>
    private static POPController POPController;

    /// <summary>
    /// The Planning Problem Constants to choose from.
    /// </summary>
    private static HashSet<string> problemConstants = new();
    public static HashSet<string> ProblemConstants { get { return problemConstants; } }

    #endregion


    #region Initialization
    /// <summary>
    /// Executes once on start.
    /// </summary>
    // Start is called before the first frame update
    void Start()
    {
        Graph = FindObjectOfType<GraphManager>();

        if (PlayerPrefs.HasKey("Mode")) if (PlayerPrefs.GetString("Mode") == "Spectator")
            {
                SearchStrategy searchStrategy = SearchStrategy.AStar;
                int maxRecommendedDepth;
                PlanningProblem planningProblem = PlanningProblem.GroceriesBuyProblem(out maxRecommendedDepth);
                StartCoroutine(StartPOPEngine(planningProblem, searchStrategy, maxRecommendedDepth));
            }
            else
            {
                InitializePlannerAndControllerFromPlayerPrefs();
                // PlayerHelperController.InitOperatorsMenu();
            }
    }
    #endregion

    // Update is called once per frame
    void Update()
    {

    }

    #region Methods

    /// <summary>
    /// Sets Operators Menu to choose from.
    /// </summary>



    /// <summary>
    /// Starts the POP Engine.
    /// </summary>
    public static IEnumerator StartPOPEngine(PlanningProblem PlanningProblem, SearchStrategy searchStrategy = SearchStrategy.AStar, int maxDepth = -1)
    {
        POPController ??= new POPController(PlanningProblem, searchStrategy, maxDepth);
        bool nextStep = true;
        Node currentNode = POPController.CurrentNode;
        int i = 0;
        List<Tuple<POP.Action, bool>> actionsDifference = new();

        yield return new WaitForSeconds(2);
        while (nextStep && POPController.GoalTest(currentNode) == false)
        {
            /****** TODO: A Good Idea, but unfortunately, it's not working as expected. So, further investigation is needed.  ******/
            /****** TODO: The idea is to only update the network/graph part that has changed in the partial plan not the whole. ******/
            // i++;
            // currentNode = POPController.CurrentNode;
            // if (Network is null && currentNode is not null)
            // {
            //     GenerateNetwork(currentNode.partialPlan);
            //     yield return new WaitForSeconds(3);
            // }

            // bool isThereDifferencesThanLastStep; List<Tuple<POP.Action, bool>> actionsDifference; List<Tuple<CausalLink, bool>> causalLinksDifference; List<Tuple<Tuple<POP.Action, POP.Action>, bool>> orderingConstraintsDifference;
            // nextStep = POPController.NextStep(out isThereDifferencesThanLastStep, out actionsDifference, out causalLinksDifference, out orderingConstraintsDifference);
            // if (isThereDifferencesThanLastStep && currentNode is not null)
            // {
            //     UpdateNetwork(currentNode.partialPlan, actionsDifference, causalLinksDifference, orderingConstraintsDifference);
            //     yield return new WaitForSeconds(3);
            // }
            // UpdateNodesText(currentNode is null ? null : currentNode.partialPlan);

            currentNode = POPController.CurrentNode;
            // Display the new Plan
            GenerateNetwork(currentNode is null ? null : currentNode.partialPlan);
            if (actionsDifference?.Count > 0) UpdateAllNodesColor(actionsDifference);

            nextStep = POPController.NextStep(out _, out actionsDifference, out _, out _);
            i++;
            yield return new WaitForSeconds(3);
        }
        if (POPController.GoalTest(currentNode))
        {
            yield return new WaitForSeconds(2);
            // Display the lineariztion of the final plan
            // AddLinearizedActionsToNetwork(new Graph<POP.Action>(currentNode.partialPlan.OrderingConstraints).Linearize(), currentNode.partialPlan);

            Graph.MaxOutRepulsionForce();
            yield return new WaitForSeconds(2);
            Graph.ResetForces();
        }

        Debug.Log($"Done after {(i - 1)} iterations: {currentNode}");
    }

    /// <summary>
    /// Generates a network from a Partial Order Plan and displays it on the graph.
    /// </summary>
    /// <param name="partialPlan">The Partial Order Plan to visualize.</param>
    public static void GenerateNetwork(POP.PartialPlan partialPlan)
    {

        // Start a new network
        Network = new ForceDirectedGraph.DataStructure.Network();

        // Generate the network
        if (partialPlan == null)
        {
            return;
        }
        // Create a node for each operator
        foreach (POP.Action action in partialPlan.Actions)
        {
            ForceDirectedGraph.DataStructure.Node node = new ForceDirectedGraph.DataStructure.Node(action, partialPlan);
            Network.Nodes.Add(node);
        }

        // Create a link for each causal link
        foreach (POP.CausalLink cl in partialPlan.CausalLinks)
        {
            ForceDirectedGraph.DataStructure.Link linkCheck = GetLinkByActions(cl.Produceri, cl.Consumerj, partialPlan);
            if (linkCheck is not null)
            {
                linkCheck.Condition += "," + partialPlan.LiteralToString(cl.LinkCondition);
            }
            else
            {
                ForceDirectedGraph.DataStructure.Link link = new ForceDirectedGraph.DataStructure.Link(GetNodeByAction(cl.Produceri), GetNodeByAction(cl.Consumerj),
                             0.01f, CausalLinkCondition: partialPlan.LiteralToString(cl.LinkCondition));
                Network.Links.Add(link);
            }
        }

        // Create a link for each ordering constraint
        foreach (Tuple<POP.Action, POP.Action> oc in partialPlan.OrderingConstraints)
        {
            // Skip the start and finish ordering constraints ... as all actions are connected between them
            if (oc.Item1.Name == "Start" || oc.Item2.Name == "Finish")
                continue;
            ForceDirectedGraph.DataStructure.Link linkCheck = GetLinkByActions(oc.Item1, oc.Item2, partialPlan);
            if (linkCheck is not null)
                continue;

            ForceDirectedGraph.DataStructure.Link link = new ForceDirectedGraph.DataStructure.Link(GetNodeByAction(oc.Item1), GetNodeByAction(oc.Item2),
                         0.001f, isOrderingConstraint: true);
            Network.Links.Add(link);
        }

        // Display the network
        if (Graph is null)
        {
            print("Graph is null");
        }
        Graph.Initialize(Network);
    }

    /// <summary>
    /// Updates the new nodes' colors in the network.
    /// </summary>
    /// <param name="actionsDifference">The differences in actions between the two partial plans.</param>
    public static void UpdateAllNodesColor(List<Tuple<POP.Action, bool>> actionsDifference)
    {
        if (Network is null)
        {
            return;
        }

        // Update the old nodes' colors to red
        foreach (ForceDirectedGraph.DataStructure.Node node in Network.Nodes)
        {
            if (node.Name == "Start()" || node.Name == "Finish()")
                continue;
            GraphNode graphNode = Graph.GraphNodes[node.Id];
            if (graphNode is not null)
            {
                Graph.ChangeNodeColor(node, Color.red);
            }
        }

        // Update the new nodes' colors to green
        foreach (Tuple<POP.Action, bool> actionDiff in actionsDifference)
        {
            Debug.Log(actionDiff.Item1);
            ForceDirectedGraph.DataStructure.Node node = GetNodeByAction(actionDiff.Item1);
            if (node is not null)
            {
                GraphNode graphNode = Graph.GraphNodes[node.Id];
                if (graphNode is not null && actionDiff.Item2)
                {
                    Graph.ChangeNodeColor(node, Color.green);
                }
            }
        }
    }



    /// <summary>
    /// Adds the list of linearized actions to the network to display.
    /// </summary>
    /// <param name="linearizedActions">The list of linearized actions to add to the network.</param>
    public static void AddLinearizedActionsToNetwork(List<POP.Action> linearizedActions, PartialPlan partialPlan)
    {
        if (Network is null)
        {
            Network = new ForceDirectedGraph.DataStructure.Network();
        }

        // Create a node for each Action
        foreach (POP.Action action in linearizedActions)
        {
            ForceDirectedGraph.DataStructure.Node node = new ForceDirectedGraph.DataStructure.Node(action, partialPlan);
            Network.Nodes.Add(node);
        }

        // Create a link between each pair of consecutive actions as an ordering constraint
        for (int i = 0; i < linearizedActions.Count - 1; i++)
        {
            ForceDirectedGraph.DataStructure.Link link = new ForceDirectedGraph.DataStructure.Link(Network.Nodes[Network.Nodes.Count - linearizedActions.Count + i], Network.Nodes[Network.Nodes.Count - linearizedActions.Count + i + 1],
                         0.3f, isOrderingConstraint: true);
            Network.Links.Add(link);
        }

        // Display the network
        Graph.Initialize(Network);
    }


    /// <summary>
    /// Updates the network with the differences between two partial plans.
    /// </summary>
    /// <param name="actionsDifference">The differences in actions between the two partial plans.</param>
    /// <param name="causalLinksDifference">The differences in causal links between the two partial plans.</param>
    /// <param name="orderingConstraintsDifference">The differences in ordering constraints between the two partial plans.</param>
    /// <param name="partialPlan">The partial plan to sync the network with.</param>
    public static void UpdateNetwork(PartialPlan partialPlan, List<Tuple<POP.Action, bool>> actionsDifference, List<Tuple<POP.CausalLink, bool>> causalLinksDifference, List<Tuple<Tuple<POP.Action, POP.Action>, bool>> orderingConstraintsDifference)
    {
        if (Network is null)
        {
            return;
        }

        // Update the network with the differences
        foreach (Tuple<POP.Action, bool> actionDiff in actionsDifference)
        {
            if (actionDiff.Item2)
            {
                // Add the action to the network
                ForceDirectedGraph.DataStructure.Node node = new ForceDirectedGraph.DataStructure.Node(actionDiff.Item1, partialPlan);
                Network.Nodes.Add(node);
                Graph.AddDisplayNode(node);
            }
            else
            {
                // Remove the action from the network
                ForceDirectedGraph.DataStructure.Node node = GetNodeByAction(actionDiff.Item1);
                Network.Nodes.Remove(node);
                Graph.RemoveNode(node);
            }
        }

        foreach (Tuple<POP.CausalLink, bool> causalLinkDiff in causalLinksDifference)
        {
            if (causalLinkDiff.Item2)
            {
                // Add the causal link to the network
                ForceDirectedGraph.DataStructure.Link link = new ForceDirectedGraph.DataStructure.Link(GetNodeByAction(causalLinkDiff.Item1.Produceri), GetNodeByAction(causalLinkDiff.Item1.Consumerj),
                             0.01f, CausalLinkCondition: partialPlan.LiteralToString(causalLinkDiff.Item1.LinkCondition));
                Network.Links.Add(link);
                Graph.AddDisplayLink(link);
            }
            else
            {
                // Remove the causal link from the network
                ForceDirectedGraph.DataStructure.Link link = GetLinkByActions(causalLinkDiff.Item1.Produceri, causalLinkDiff.Item1.Consumerj, partialPlan);
                // Check first if there is no other link with the same actions
                bool isThereAnotherLink = partialPlan.OrderingConstraints.Contains(new(causalLinkDiff.Item1.Produceri, causalLinkDiff.Item1.Consumerj));
                if (Graph.RemoveLink(link, isThereAnotherLink, partialPlan.LiteralToString(causalLinkDiff.Item1.LinkCondition)))
                    if (isThereAnotherLink) link.IsOrderingConstraint = true;
                    else Network.Links.Remove(link);

            }
        }

        foreach (Tuple<Tuple<POP.Action, POP.Action>, bool> orderingConstraintDiff in orderingConstraintsDifference)
        {
            if (orderingConstraintDiff.Item2)
            {
                // Add the ordering constraint to the network
                ForceDirectedGraph.DataStructure.Link link = new ForceDirectedGraph.DataStructure.Link(GetNodeByAction(orderingConstraintDiff.Item1.Item1), GetNodeByAction(orderingConstraintDiff.Item1.Item2),
                             0.001f, isOrderingConstraint: true);
                Network.Links.Add(link);
                Graph.AddDisplayLink(link);
            }
            else
            {
                // Remove the ordering constraint from the network
                ForceDirectedGraph.DataStructure.Link link = GetLinkByActions(orderingConstraintDiff.Item1.Item1, orderingConstraintDiff.Item1.Item2, partialPlan);

                Network.Links.Remove(link);
                Graph.RemoveLink(link, false);
            }
        }

    }


    /// <summary>
    /// Updates all the nodes' data in the network based on a new partial plan.
    /// </summary>
    /// <param name="partialPlan">The new partial plan to update the nodes' names with.</param>
    public static void UpdateNodesText(PartialPlan partialPlan)
    {
        if (Network is null)
        {
            return;
        }

        // Update the nodes' data
        foreach (ForceDirectedGraph.DataStructure.Node node in Network.Nodes)
        {
            node.UpdateName(partialPlan.ActionToString);
            GraphNode graphNode = Graph.GraphNodes[node.Id];
            if (graphNode is not null)
            {
                graphNode.UpdatePrecondions(node.Action.Preconditions, partialPlan.LiteralToString);
                graphNode.UpdateEffects(node.Action.Effects, partialPlan.LiteralToString);
            }
        }
    }


    /// <summary>
    /// Gets Node by Name.
    /// </summary>
    /// <param name="name">The name of the node to get.</param>
    /// <returns>The node with the given name.</returns>
    /// <returns>null if no node with the given name is found.</returns>
    public static ForceDirectedGraph.DataStructure.Node GetNodeByName(string name)
    {
        foreach (ForceDirectedGraph.DataStructure.Node node in Network.Nodes)
        {
            if (node.Name == name)
            {
                return node;
            }
        }
        return null;
    }



    /// <summary>
    /// Gets Node by Action.
    /// </summary>
    /// <param name="action">The action of the node to get.</param>
    /// <returns>The node with the given name.</returns>
    /// <returns>null if no node with the given name is found.</returns>
    public static ForceDirectedGraph.DataStructure.Node GetNodeByAction(POP.Action action)
    {
        foreach (ForceDirectedGraph.DataStructure.Node node in Network.Nodes)
        {
            if (node.Action == action)
            {
                return node;
            }
        }
        return null;
    }


    /*/// <summary>
    /// Gets Link by Condition.
    /// </summary>
    /// <param name="condition">The condition of the link to get.</param>
    /// <returns>The link with the given condition.</returns>
    /// <returns>null if no link with the given condition is found.</returns>
    public static ForceDirectedGraph.DataStructure.Link GetLinkByCondition(string condition)
    {
        foreach (ForceDirectedGraph.DataStructure.Link link in Network.Links)
        {
            if (link.Condition == condition)
            {
                return link;
            }
        }
        return null;
    }*/

    /// <summary>
    /// Gets Link by Actions.
    /// </summary>
    /// <param name="producer">The producer action of the link to get.</param>
    /// <param name="consumer">The consumer action of the link to get.</param>
    /// <returns>The link with the given producer and consumer actions.</returns>
    /// <returns>null if no link with the given producer and consumer actions is found.</returns>
    public static ForceDirectedGraph.DataStructure.Link GetLinkByActions(POP.Action producer, POP.Action consumer, PartialPlan partialPlan)
    {
        foreach (ForceDirectedGraph.DataStructure.Link link in Network.Links)
        {
            if (link.FirstNodeId == GetNodeByAction(producer)?.Id && link.SecondNodeId == GetNodeByAction(consumer)?.Id)
            {
                return link;
            }
        }
        return null;
    }

    /// <summary>
    /// Initializes Planner and Controller from the Player Preferences.
    /// </summary>
    public static void InitializePlannerAndControllerFromPlayerPrefs()
    {
        PlanningProblem planningProblem = GetPlanningProblemFromPlayerPrefs();
        SearchStrategy searchStrategy = GetSearchStrategyFromPlayerPrefs();
        int maxDepth = GetMaxDepthFromPlayerPrefs();
        POPController = new POPController(planningProblem, searchStrategy, maxDepth);

        // gather the problem constants from initial state and goal state (only consider the string vars of upper case)
        foreach (POP.Literal lit in POPController.Planner.Problem.InitialState.Concat(POPController.Planner.Problem.GoalState))
        {
            foreach (string var in lit.Variables)
            {
                if (Helpers.IsUpper(var[0])) problemConstants.Add(var);
            }
        }
        PlayerHelperController.Problem = planningProblem;
    }


    /// <summary>
    /// Gets Search Strategy from the Player Preferences.
    /// </summary>
    /// <returns>The search strategy object based on the player preferences.</returns>
    /// <returns>SearchStrategy.AStar if no search strategy is found in the player preferences.</returns>
    public static SearchStrategy GetSearchStrategyFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("SearchStrategy"))
        {
            string searchStrategy = PlayerPrefs.GetString("SearchStrategy");
            return (SearchStrategy)Enum.Parse(typeof(SearchStrategy), searchStrategy == "A*" ? "AStar" : searchStrategy);
        }
        return SearchStrategy.AStar;
    }


    /// <summary>
    /// Gets Max Depth from the Player Preferences.
    /// </summary>
    /// <returns>The max depth based on the player preferences.</returns>
    /// <returns>-1 if no max depth is found in the player preferences.</returns>
    public static int GetMaxDepthFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("MaxDepth"))
        {
            return PlayerPrefs.GetInt("MaxDepth");
        }
        return -1;
    }


    /// <summary>
    /// Gets the Planning Problem from the Player Preferences.
    /// </summary>
    /// <returns>The planning problem object based on the player preferences.</returns>
    /*
        SS: Socks and Shoes Problem
        MBCD: Milk, Bananas, Cordless Drill Problem
        GB: Groceries Buying Problem
        SP: Spare Tires Problem
    */
    public static PlanningProblem GetPlanningProblemFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("ProblemName"))
        {
            string planningProblem = PlayerPrefs.GetString("ProblemName");
            return planningProblem switch
            {
                "SS" => PlanningProblem.SocksShoesProblem(out _),
                "MBCD" => PlanningProblem.MilkBananasCordlessDrillProblem(out _),
                "GB" => PlanningProblem.GroceriesBuyProblem(out _),
                "SP" => PlanningProblem.SpareTiresProblem(out _),
                _ => PlanningProblem.GroceriesBuyProblem(out _) // Default to Groceries Buy Problem
            };
        }
        return PlanningProblem.GroceriesBuyProblem(out _);
    }



    #endregion
}
