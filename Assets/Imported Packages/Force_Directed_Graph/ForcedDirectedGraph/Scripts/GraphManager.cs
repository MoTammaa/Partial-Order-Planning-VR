using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace ForceDirectedGraph
{
    public class GraphManager : MonoBehaviour
    {

        #region Constants


        /// <summary>
        /// The repulsion force between any two nodes.
        /// </summary>
        private float REPULSION_FORCE = 7000f;//40000f;

        /// <summary>
        /// The maximum distance for applying repulsion forces.
        /// </summary>
        private float REPULSION_DISTANCE = 1.5f;

        /// <summary>
        /// The attraction force between any two nodes.
        /// </summary>
        private float ATTRACTION_FORCE = 10000f;

        /// <summary>
        /// The default position of the graph.
        /// </summary>
        private Vector3 GraphPosition;

        /// <summary>
        /// The width of the graph display area.
        /// </summary>
        private float GRAPH_WIDTH = 13.5f;

        /// <summary>
        /// The height of the graph display area.
        /// </summary>
        private float GRAPH_HEIGHT = 7.5f;


        /// <summary>
        /// Cancels all forces applied to the nodes and sets them to 0.
        /// </summary>
        public void CancelForces()
        {
            REPULSION_FORCE = 0;
            ATTRACTION_FORCE = 0;
        }

        /// <summary>
        /// Resets the forces to their default values.
        /// </summary>
        public void ResetForces()
        {
            REPULSION_FORCE = 7000f;
            ATTRACTION_FORCE = 10000f;
        }

        /// <summary>
        /// Maxes out REPULSION_FORCE to value = 60000f.
        /// </summary>
        public void MaxOutRepulsionForce()
        {
            REPULSION_FORCE = 60000f;
        }
        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the graph.
        /// </summary>
        /// <param name="network">The netwok being displayed.</param>
        public void Initialize(DataStructure.Network network)
        {
            Debug.Log("Initializing graph");
            GraphPosition = transform.position;
            _Network = network;
            GRAPH_WIDTH *= transform.localScale.x;
            GRAPH_HEIGHT *= transform.localScale.y;
            Display();
        }

        #endregion

        #region Fields/Properties

        [Header("Nodes")]

        /// <summary>
        /// References the parent holding all nodes.
        /// </summary>
        [SerializeField]
        [Tooltip("References the parent holding all nodes.")]
        private GameObject NodesParent;

        /// <summary>
        /// Template used for initiating nodes.
        /// </summary>
        [SerializeField]
        [Tooltip("Template used for initiating nodes.")]
        private GameObject[] NoteTemplate;

        /// <summary>
        /// List of all nodes displayed on the graph.
        /// </summary>
        private Dictionary<Guid, GraphNode> _GraphNodes;

        /// <summary>
        /// List of all nodes displayed on the graph.
        /// </summary>
        public Dictionary<Guid, GraphNode> GraphNodes { get { return _GraphNodes; } }


        [Header("Links")]

        /// <summary>
        /// References the parent holding all links.
        /// </summary>
        [SerializeField]
        [Tooltip("References the parent holding all links.")]
        private GameObject LinksParent;

        /// <summary>
        /// Template used for initiating links.
        /// </summary>
        [SerializeField]
        [Tooltip("Template used for initiating links.")]
        private GameObject LinkTemplate;

        /// <summary>
        /// Template used for initiating links.
        /// </summary>
        [SerializeField]
        [Tooltip("Template used for initiating links.")]
        private GameObject OrderingLinkTemplate;

        /// <summary>
        /// List of all links displayed on the graph.
        /// </summary>
        private List<GraphLink> graphLinks;
        public List<GraphLink> GraphLinks { get { return graphLinks; } }

        /// <summary>
        /// Arrow head template.
        /// </summary>
        [SerializeField]
        [Tooltip("Template used for initiating arrow heads.")]
        private GameObject ArrowHeadTemplate;

        /// <summary>
        /// Arrow head template.
        /// </summary>
        [SerializeField]
        [Tooltip("Template used for initiating arrow heads.")]
        private GameObject OrderingArrowHeadTemplate;

        [Header("Data")]

        /// <summary>
        /// The netwok being displayed.
        /// </summary>
        [SerializeField]
        [Tooltip("The netwok being displayed.")]
        private DataStructure.Network _Network;

        /// <summary>
        /// The netwok being displayed.
        /// </summary>
        public DataStructure.Network Network { get { return _Network; } }

        #endregion

        #region Display Methods

        /// <summary>
        /// Displays the network.
        /// </summary>
        private void Display()
        {
            // Clear everything
            Clear();
            Debug.Log("Displaying network");

            // Display nodes
            DisplayNodes();

            // Display links
            DisplayLinks();

            // Shuffle the nodes
            ShuffleNodes();
        }

        /// <summary>
        /// Deletes all nodes and links in the graph.
        /// </summary>
        private void Clear()
        {
            // Clear nodes
            _GraphNodes = new Dictionary<Guid, GraphNode>();
            foreach (Transform entity in NodesParent.transform)
                GameObject.Destroy(entity.gameObject);

            // Clear paths
            graphLinks = new List<GraphLink>();
            foreach (Transform path in LinksParent.transform)
                GameObject.Destroy(path.gameObject);
        }

        /// <summary>
        /// Displays nodes on the graph.
        /// </summary>
        private void DisplayNodes()
        {
            // For each position, create an entity
            foreach (var node in Network?.Nodes)
            {
                AddDisplayNode(node);
            }
        }


        /// <summary>
        /// Adds & displays a new node to the graph.
        /// </summary>
        /// <param name="node">The node to add.</param>
        public void AddDisplayNode(DataStructure.Node node, Color color = default, Vector3 position = default, GameObject existingNode = null)
        {
            Color red = Color.red, green = Color.green;
            if (color == default) color = red;
            if (position == default) position = Vector3.zero;

            // Create a new entity instance
            int index = (node.Name == "Start()" || node.Name == "Finish()") ? 2 : color == red ? 0 : color == green ? 1 : UnityEngine.Random.Range(0, 2);
            GameObject graphNode = existingNode ?? Instantiate(NoteTemplate[index], NodesParent.transform);
            if (!existingNode)
            {
                graphNode.transform.position = position;
                // add the starting position offset to the node
                graphNode.transform.position = new Vector3(graphNode.transform.position.x + GraphPosition.x, graphNode.transform.position.y + GraphPosition.y, graphNode.transform.position.z + GraphPosition.z);
                graphNode.transform.localRotation = Quaternion.Euler(Vector3.zero);

                if (position == Vector3.zero)
                    graphNode.transform.localPosition = new Vector3(-GRAPH_WIDTH / 2 + 0.1f, -GRAPH_HEIGHT / 2 + 0.1f, 0);
            }

            // Extract the script
            GraphNode script = graphNode.GetComponent<GraphNode>();

            // Initialize data
            script.Initialize(node);

            // Add to list
            _GraphNodes?.Add(node.Id, script);
        }

        /// <summary>
        /// Removes a node from the graph and all its links.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        /// <returns>True if the node was removed, false otherwise.</returns>
        public bool RemoveNode(DataStructure.Node node)
        {
            // Find the node
            if (!_GraphNodes.ContainsKey(node.Id))
                return false;
            GraphNode graphNode = _GraphNodes[node.Id];

            // Remove the node
            _GraphNodes.Remove(node.Id);
            Network.Nodes.Remove(node);

            // Remove all links connected to the node
            List<GraphLink> linksToRemove = new List<GraphLink>();
            foreach (var link in graphLinks)
                if (link.FirstNode.Node.Id == node.Id || link.SecondNode.Node.Id == node.Id)
                    linksToRemove.Add(link);
            foreach (var link in linksToRemove)
            {
                graphLinks.Remove(link);
                Network.Links.Remove(link.Link);
                Destroy(link.gameObject);
            }
            // Destroy the node from the scene
            Destroy(graphNode.gameObject);

            return true;
        }

        /// <summary>
        /// Replaces a node and changes its color to the given color with the same attributes.
        /// </summary>
        /// <param name="node">The node to replace.</param>
        /// <param name="color">The color to change the node to.</param>
        /// <returns>The new node.</returns>
        public DataStructure.Node ChangeNodeColor(DataStructure.Node node, Color color)
        {

            // Find the node
            if (!_GraphNodes.ContainsKey(node.Id))
                return null;
            GraphNode graphNode = _GraphNodes[node.Id];

            // Remove the old node
            _GraphNodes.Remove(node.Id);
            Destroy(graphNode.gameObject);

            // Create a new node
            DataStructure.Node newNode = new DataStructure.Node(node);
            AddDisplayNode(newNode, color, graphNode.transform.localPosition);

            // Update links to the new replaced node
            foreach (var link in graphLinks)
            {
                if (link.FirstNode.Node.Id == node.Id)
                    link.FirstNode = _GraphNodes[newNode.Id];
                if (link.SecondNode.Node.Id == node.Id)
                    link.SecondNode = _GraphNodes[newNode.Id];
            }

            return newNode;
        }


        /// <summary>
        /// Displays links on the graph.
        /// </summary>
        private void DisplayLinks()
        {
            // For each position, create an entity
            foreach (var link in Network?.Links)
            {
                AddDisplayLink(link);
            }
        }

        /// <summary>
        /// Adds & displays a new link to the graph.
        /// </summary>
        /// <param name="link">The link to add.</param>
        public void AddDisplayLink(DataStructure.Link link)
        {
            if (link == null) return;
            // Find graph nodes
            if (!_GraphNodes.ContainsKey(link.FirstNodeId)
                || !_GraphNodes.ContainsKey(link.SecondNodeId))
                return;
            GraphNode firstNode = _GraphNodes?[link.FirstNodeId];
            GraphNode secondNode = _GraphNodes?[link.SecondNodeId];

            // Create a new entity instance
            GameObject theTemplateForEdgeLink = link.IsOrderingConstraint ? OrderingLinkTemplate : LinkTemplate;
            GameObject graphLink = Instantiate(theTemplateForEdgeLink, LinksParent.transform);
            graphLink.transform.position = Vector3.zero;
            graphLink.transform.localRotation = Quaternion.Euler(Vector3.zero);

            // Instantiate the arrow head
            GameObject theTemplateForArrowHead = link.IsOrderingConstraint ? OrderingArrowHeadTemplate : ArrowHeadTemplate;
            GameObject linkConditionArrow = Instantiate(theTemplateForArrowHead, graphLink.transform);
            linkConditionArrow.transform.localPosition = Vector3.zero;
            linkConditionArrow.transform.localRotation = Quaternion.Euler(Vector3.zero);

            // Extract the script
            GraphLink script = graphLink.GetComponent<GraphLink>();

            // Initialize data
            script.Initialize(link, firstNode, secondNode, linkConditionArrow);

            // Add to list
            graphLinks.Add(script);
        }

        /// <summary>
        /// Removes a link from the graph.
        /// </summary>
        /// <param name="link">The link to remove.</param>
        /// <returns>True if the link was removed, false otherwise.</returns>
        public bool RemoveLink(DataStructure.Link link, bool thereIsOrderingConstraintInsteadOfThisCausalLink, string linkCondition = null)
        {
            // Find the link
            GraphLink graphLink = graphLinks.FirstOrDefault(l => l?.Link?.FirstNodeId == link?.FirstNodeId && l?.Link?.SecondNodeId == link?.SecondNodeId);
            if (graphLink == null)
                return false;
            if (linkCondition != null && link.Condition != null)
            {
                string[] linkConditionsSplit = link.Condition.Split("),");
                if (linkConditionsSplit.Length > 0)
                {
                    string newCondition = string.Join("),", linkConditionsSplit.Where(c => !c.Equals(linkCondition + ")")));
                    link.Condition = newCondition + ")";
                    return false; // didn't completely remove the link
                }
            }

            // Remove the link
            graphLinks.Remove(graphLink);
            Destroy(graphLink.gameObject);

            if (thereIsOrderingConstraintInsteadOfThisCausalLink)
            {
                // add the ordering constraint instead of the causal link
                AddDisplayLink(new DataStructure.Link(link.FirstNodeId, link.SecondNodeId, 0.001f, true));
            }

            return true;
        }

        /// <summary>
        /// Shuffles the nodes randomly.
        /// </summary>
        public void ShuffleNodes()
        {
            System.Random random = new System.Random();
            foreach (var node in _GraphNodes.Values)
                node.ApplyForces(new List<Vector2>() { new Vector2(random.Next(-10, 10) / 10f, random.Next(-10, 10) / 10f) }, true);
        }

        #endregion

        #region Force Methods

        /// <summary>
        /// Continuously apply forces to nodes.
        /// </summary>
        private void Update()
        {
            ApplyForces();
        }

        /// <summary>
        /// Computes and applies forces to nodes.
        /// </summary>
        private void ApplyForces()
        {
            if (_GraphNodes == null)
                return;

            // Stores all the forces to be applied to each node
            Dictionary<GraphNode, List<Vector2>> nodeForces = new Dictionary<GraphNode, List<Vector2>>();
            foreach (var node1 in _GraphNodes.Values)
                nodeForces.Add(node1, new List<Vector2>());

            // Compute repulsion forces
            foreach (var node1 in _GraphNodes.Values)
                foreach (var node2 in _GraphNodes.Values)
                    if (node1 != node2)
                        nodeForces[node1].Add(ComputeRepulsiveForce(node1, node2));

            // Compute attraction forces
            foreach (var link in graphLinks)
            {
                var force = ComputeAttractionForce(link);
                nodeForces[link.FirstNode].Add(-force);
                nodeForces[link.SecondNode].Add(force);
            }

            // Apply forces
            foreach (var node in nodeForces.Keys)
                node.ApplyForces(nodeForces[node]);
        }

        /// <summary>
        /// Computes the distance between two nodes.
        /// </summary>
        private float ComputeDistance(GraphNode node1, GraphNode node2)
        {
            return (float)
                Math.Sqrt
                (
                    Math.Pow(node1.transform.position.x - node2.transform.position.x, 2)
                    +
                    Math.Pow(node1.transform.position.y - node2.transform.position.y, 2)
                );
        }

        /// <summary>
        /// Computes the repulsive force against a node.
        /// </summary>
        private Vector2 ComputeRepulsiveForce(GraphNode node, GraphNode repulsiveNode)
        {
            // Compute distance
            float distance = ComputeDistance(node, repulsiveNode);
            if (distance > REPULSION_DISTANCE)
                return Vector3.zero;

            // Compute force direction
            Vector2 forceDirection = (node.transform.position - repulsiveNode.transform.position).normalized;
            if (forceDirection == Vector2.zero)
                forceDirection = new Vector2(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1)).normalized;

            // Compute distance force
            float distanceForce = (REPULSION_DISTANCE - distance) / REPULSION_DISTANCE;

            // Compute repulsive force
            return forceDirection * distanceForce * REPULSION_FORCE * Time.deltaTime;
        }

        /// <summary>
        /// Computes the attraction force between two nodes.
        /// </summary>
        private Vector2 ComputeAttractionForce(GraphLink link)
        {
            if (link.FirstNode == null || link.SecondNode == null)
                return Vector2.zero;
            // Compute force direction
            Vector2 forceDirection = (link.FirstNode.transform.position - link.SecondNode.transform.position).normalized;

            // Compute repulsive force
            return forceDirection * link.Link.Width * ATTRACTION_FORCE * Time.deltaTime;
        }

        #endregion

    }
}
