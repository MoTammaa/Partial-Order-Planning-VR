using ForceDirectedGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneManager : MonoBehaviour
{

    #region Initialization

    /// <summary>
    /// Executes once on start.
    /// </summary>
    private void Start()
    {
        // Display the app version
        DisplayVersion();

        // Generate a sample to visualize
        GenerateSample();
    }

    #endregion

    #region Fields/Properties

    /// <summary>
    /// The graph displaying the network.
    /// </summary>
    [SerializeField]
    [Tooltip("The graph displaying the network.")]
    private GraphManager Graph;

    /// <summary>
    /// Text UI element displaying the app version.
    /// </summary>
    [SerializeField]
    [Tooltip("Text UI element displaying the app version.")]
    private Text Version;

    #endregion

    #region Methods

    /// <summary>
    /// Displays current project's version.
    /// </summary>
    private void DisplayVersion()
    {
        // Version.text = string.Format("Version: {0}", Application.version);
    }

    /// <summary>
    /// Generates a network sample and displays it on the graph.
    /// </summary>
    public void GenerateSample()
    {
        // Start a new network
        ForceDirectedGraph.DataStructure.Network network = new ForceDirectedGraph.DataStructure.Network();

        // // Add a triangle network
        // GenerateTriangleSample(network);

        // // Add a center network
        // GenerateCenterSample(network);

        // // Add a sqaure network
        // GenerateSample(network, 4);

        // // Add a star network
        // GenerateSample(network, 5);

        // Add a DAG network
        GenerateDAGSample(network);

        // Display network
        Graph.Initialize(network);
    }

    /// <summary>
    /// Generates nodes and connects them together to form a triangle.
    /// </summary>
    /// <param name="network">The network used to append the items and links to.</param>
    private void GenerateTriangleSample(ForceDirectedGraph.DataStructure.Network network)
    {
        // Create nodes
        ForceDirectedGraph.DataStructure.Node item1 = new ForceDirectedGraph.DataStructure.Node(Guid.NewGuid(), "Item 1", Color.red);
        ForceDirectedGraph.DataStructure.Node item2 = new ForceDirectedGraph.DataStructure.Node(Guid.NewGuid(), "Item 2", Color.blue);
        ForceDirectedGraph.DataStructure.Node item3 = new ForceDirectedGraph.DataStructure.Node(Guid.NewGuid(), "Item 3", Color.green);

        // Add nodes to network
        network.Nodes.Add(item1);
        network.Nodes.Add(item2);
        network.Nodes.Add(item3);

        // Create links and add to network
        network.Links.Add(new ForceDirectedGraph.DataStructure.Link(item1.Id, item2.Id, 0.5f, Color.white)); // Item 1 -> Item 2
        network.Links.Add(new ForceDirectedGraph.DataStructure.Link(item1.Id, item3.Id, 0.5f, Color.white)); // Item 1 -> Item 3
        network.Links.Add(new ForceDirectedGraph.DataStructure.Link(item2.Id, item3.Id, 0.5f, Color.white)); // Item 2 -> Item 3
    }

    /// <summary>
    /// Generates nodes and connects them all to a center node.
    /// </summary>
    /// <param name="network">The network used to append the items and links to.</param>
    private void GenerateCenterSample(ForceDirectedGraph.DataStructure.Network network)
    {
        int start = 4;
        int count = 5;

        // Create nodes
        List<ForceDirectedGraph.DataStructure.Node> nodes = new List<ForceDirectedGraph.DataStructure.Node>();
        for (int i = start; i < start + count; i++)
            nodes.Add(new ForceDirectedGraph.DataStructure.Node
            (
                id: Guid.NewGuid(),
                name: string.Format("Item {0}", i),
                color: i == start ? Color.white : Color.black
            ));

        // Add nodes to graph
        network.Nodes.AddRange(nodes);

        // Create links and add to network
        for (int i = 1; i < nodes.Count; i++)
            network.Links.Add(new ForceDirectedGraph.DataStructure.Link(nodes[0].Id, nodes[i].Id, i == 1 ? 0.7f : 0.2f, Color.white));

    }

    /// <summary>
    /// Generates nodes and connects them all together.
    /// </summary>
    /// <param name="network">The network used to append the items and links to.</param>
    /// <param name="count">Number of nodes to add.</param>
    private void GenerateSample(ForceDirectedGraph.DataStructure.Network network, int count)
    {
        // Create nodes
        List<ForceDirectedGraph.DataStructure.Node> nodes = new List<ForceDirectedGraph.DataStructure.Node>();
        for (int i = 0; i < count; i++)
            nodes.Add(new ForceDirectedGraph.DataStructure.Node
            (
                id: Guid.NewGuid(),
                name: string.Empty,
                color: Color.grey
            ));

        // Add nodes to graph
        network.Nodes.AddRange(nodes);

        // Create links and add to network
        for (int i = 0; i < nodes.Count - 1; i++)
            for (int j = i + 1; j < nodes.Count; j++)
                network.Links.Add(new ForceDirectedGraph.DataStructure.Link(nodes[i].Id, nodes[j].Id, 0.2f, Color.white));
    }

    private void GenerateDAGSample(ForceDirectedGraph.DataStructure.Network network)
    {
        // Create nodes
        ForceDirectedGraph.DataStructure.Node item0 = new ForceDirectedGraph.DataStructure.Node(Guid.NewGuid(), "Item 0", Color.white);
        ForceDirectedGraph.DataStructure.Node item1 = new ForceDirectedGraph.DataStructure.Node(Guid.NewGuid(), "Item 1", Color.red);
        ForceDirectedGraph.DataStructure.Node item2 = new ForceDirectedGraph.DataStructure.Node(Guid.NewGuid(), "Item 2", Color.blue);
        ForceDirectedGraph.DataStructure.Node item3 = new ForceDirectedGraph.DataStructure.Node(Guid.NewGuid(), "Item 3", Color.green);
        ForceDirectedGraph.DataStructure.Node item4 = new ForceDirectedGraph.DataStructure.Node(Guid.NewGuid(), "Item 4", Color.yellow);
        ForceDirectedGraph.DataStructure.Node item5 = new ForceDirectedGraph.DataStructure.Node(Guid.NewGuid(), "Item 5", Color.cyan);
        ForceDirectedGraph.DataStructure.Node item6 = new ForceDirectedGraph.DataStructure.Node(Guid.NewGuid(), "Item 6", Color.magenta);
        ForceDirectedGraph.DataStructure.Node item7 = new ForceDirectedGraph.DataStructure.Node(Guid.NewGuid(), "Item 7", Color.black);

        // Add nodes to network
        network.Nodes.Add(item0);
        network.Nodes.Add(item1);
        network.Nodes.Add(item2);
        network.Nodes.Add(item3);
        network.Nodes.Add(item4);
        network.Nodes.Add(item5);
        network.Nodes.Add(item6);
        network.Nodes.Add(item7);

        // Create links and add to network
        network.Links.Add(new ForceDirectedGraph.DataStructure.Link(item0.Id, item1.Id, 0.1f, Color.white)); // Item 0 -> Item 1
        network.Links.Add(new ForceDirectedGraph.DataStructure.Link(item1.Id, item2.Id, 0.1f, Color.white)); // Item 1 -> Item 2
        network.Links.Add(new ForceDirectedGraph.DataStructure.Link(item1.Id, item3.Id, 0.1f, Color.white)); // Item 1 -> Item 3
        network.Links.Add(new ForceDirectedGraph.DataStructure.Link(item2.Id, item4.Id, 0.1f, Color.white)); // Item 2 -> Item 4
        network.Links.Add(new ForceDirectedGraph.DataStructure.Link(item3.Id, item4.Id, 0.1f, Color.white)); // Item 3 -> Item 4
        network.Links.Add(new ForceDirectedGraph.DataStructure.Link(item4.Id, item5.Id, 0.1f, Color.white)); // Item 4 -> Item 5
        network.Links.Add(new ForceDirectedGraph.DataStructure.Link(item5.Id, item6.Id, 0.1f, Color.white)); // Item 5 -> Item 6
        network.Links.Add(new ForceDirectedGraph.DataStructure.Link(item6.Id, item7.Id, 0.1f, Color.white)); // Item 6 -> Item 7
        network.Links.Add(new ForceDirectedGraph.DataStructure.Link(item0.Id, item7.Id, 0.001f, Color.white)); // Item 6 -> Item 7


    }

    #endregion

}
