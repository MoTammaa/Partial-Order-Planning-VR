using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ForceDirectedGraph;
using POP;
using System;

public class POPEngineDriverController : MonoBehaviour
{
    #region Fields/Properties
    /// <summary>
    /// The graph displaying the network.
    /// </summary>
    [SerializeField]
    [Tooltip("The graph displaying the network.")]
    private GraphManager Graph;

    /// <summary>
    /// The Network that contains the graph data.
    /// </summary>
    private ForceDirectedGraph.DataStructure.Network Network;

    /// <summary>
    /// POP Backend Controller Reference.
    /// </summary>
    private POPController POPController;

    #endregion


    #region Initialization
    /// <summary>
    /// Executes once on start.
    /// </summary>
    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(StartPOPEngine());
    }
    #endregion

    // Update is called once per frame
    void Update()
    {

    }

    #region Methods

    /// <summary>
    /// Starts the POP Engine.
    /// </summary>
    public IEnumerator StartPOPEngine()
    {
        POPController = new POPController(PlanningProblem.GroceriesBuyProblem(false));
        bool nextStep = true;
        Node currentNode = POPController.CurrentNode;
        while (nextStep && POPController.GoalTest(currentNode) == false)
        {
            currentNode = POPController.CurrentNode;
            // Display the new Plan
            GenerateNetwork(currentNode is null ? null : currentNode.partialPlan);

            yield return new WaitForSeconds(3);
            nextStep = POPController.NextStep();
        }

        Debug.Log("Done " + currentNode);
    }

    /// <summary>
    /// Generates a network from a Partial Order Plan and displays it on the graph.
    /// </summary>
    /// <param name="partialPlan">The Partial Order Plan to visualize.</param>
    public void GenerateNetwork(POP.PartialPlan partialPlan)
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
                ForceDirectedGraph.DataStructure.Link link = new ForceDirectedGraph.DataStructure.Link(GetNodeByAction(cl.Produceri).Id, GetNodeByAction(cl.Consumerj).Id,
                             0.01f, Color.white, partialPlan.LiteralToString(cl.LinkCondition));
                Network.Links.Add(link);
            }
        }

        // Display the network
        Graph.Initialize(Network);
    }

    /// <summary>
    /// Gets Node by Name.
    /// </summary>
    /// <param name="name">The name of the node to get.</param>
    /// <returns>The node with the given name.</returns>
    /// <returns>null if no node with the given name is found.</returns>
    public ForceDirectedGraph.DataStructure.Node GetNodeByName(string name)
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
    public ForceDirectedGraph.DataStructure.Node GetNodeByAction(POP.Action action)
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
    public ForceDirectedGraph.DataStructure.Link GetLinkByCondition(string condition)
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
    public ForceDirectedGraph.DataStructure.Link GetLinkByActions(POP.Action producer, POP.Action consumer, PartialPlan partialPlan)
    {
        foreach (ForceDirectedGraph.DataStructure.Link link in Network.Links)
        {
            if (link.FirstNodeId == GetNodeByAction(producer).Id && link.SecondNodeId == GetNodeByAction(consumer).Id)
            {
                return link;
            }
        }
        return null;
    }


    #endregion
}
