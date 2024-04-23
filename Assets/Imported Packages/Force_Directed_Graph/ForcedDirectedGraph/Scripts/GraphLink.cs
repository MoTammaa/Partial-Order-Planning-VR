using ForceDirectedGraph.DataStructure;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ForceDirectedGraph
{
    public class GraphLink : MonoBehaviour
    {

        #region Initialization

        /// <summary>
        /// Executes once on start.
        /// </summary>
        private void Awake()
        {
            LineRenderer = GetComponent<LineRenderer>();
        }

        /// <summary>
        /// Initializes the graph entity.
        /// </summary>
        /// <param name="link">The link being presented.</param>
        /// <param name="firstNode">The first graph node this entity is attached to.</param>
        /// <param name="secondNode">The second graph node this entity is attached to.</param>
        public void Initialize(Link link, GraphNode firstNode, GraphNode secondNode, GameObject linkConditionArrow)
        {
            _Link = link;
            _FirstNode = firstNode;
            _SecondNode = secondNode;

            // Set color
            LineRenderer.startColor = link.Color;
            LineRenderer.endColor = link.Color;

            // Set width
            float width = link.Width == 0 ? 0 : link.Width * 0.08f + 0.02f; // [0.02 -> 0.1]
            LineRenderer.startWidth = width;
            LineRenderer.endWidth = width;

            // Set arrow head
            _LinkConditionArrow = linkConditionArrow;
            // Set the text on the arrow
            UpdateLinkText(link.Condition);
        }

        #endregion

        #region Fields/Properties

        /// <summary>
        /// The link being presented.
        /// </summary>
        [SerializeField]
        [Tooltip("The link being presented.")]
        private Link _Link;

        /// <summary>
        /// The link being presented.
        /// </summary>
        public Link Link { get { return _Link; } }



        /// <summary>
        /// The first graph node this entity is attached to.
        /// </summary>
        [SerializeField]
        private GraphNode _FirstNode;

        /// <summary>
        /// The first graph node this entity is attached to.
        /// </summary>
        public GraphNode FirstNode { get { return _FirstNode; } }



        /// <summary>
        /// The second graph node this entity is attached to.
        /// </summary>
        [SerializeField]
        private GraphNode _SecondNode;

        /// <summary>
        /// The second graph node this entity is attached to.
        /// </summary>
        public GraphNode SecondNode { get { return _SecondNode; } }



        /// <summary>
        /// References the line renderer that displays the link.
        /// </summary>
        private LineRenderer LineRenderer;



        /// <summary>
        /// Arrow head object.
        /// </summary>
        [SerializeField]
        [Tooltip("Arrow head object.")]
        private GameObject _LinkConditionArrow;

        /// <summary>
        /// Arrow head object.
        /// </summary>
        public GameObject LinkConditionArrow { get { return _LinkConditionArrow; } }

        #endregion

        #region Methods

        /// <summary>
        /// Update the line to keep the two nodes connected.
        /// </summary>
        private void Update()
        {
            LineRenderer.useWorldSpace = true;

            Vector3 firstPosition = _FirstNode.transform.position + (_SecondNode.transform.position - _FirstNode.transform.position).normalized * 0.1f;
            Vector3 secondPosition = _SecondNode.transform.position + (_FirstNode.transform.position - _SecondNode.transform.position).normalized * 0.1f;

            LineRenderer.SetPosition(0, firstPosition);
            LineRenderer.SetPosition(1, secondPosition);

            if (_LinkConditionArrow != null)
            {
                // set the position of the arrow head to the middle of the line
                _LinkConditionArrow.transform.position = (firstPosition + secondPosition) / 2;
                // set the rotation of the arrow head to point towards the second node
                if (firstPosition != secondPosition)
                {
                    _LinkConditionArrow.transform.rotation = Quaternion.LookRotation(secondPosition - firstPosition, Vector3.up);
                }

                // set the text to update if the condition changes
                UpdateLinkText(_Link.Condition);
            }
        }

        /// <summary>
        /// Update the text on the link.
        /// </summary>
        /// <param name="text">The text to be displayed.</param>
        public void UpdateLinkText(string text)
        {
            var textF = _LinkConditionArrow.transform.Find("BodyText").Find("TextF").GetComponent<TMPro.TextMeshPro>();
            var textB = _LinkConditionArrow.transform.Find("BodyText").Find("TextB").GetComponent<TMPro.TextMeshPro>();

            if (textF.text != text)
            {
                // make the font size smaller if the text is too long based on the number of commas count ... for every comma, reduce the font size by 0.05 from the original size 3.5
                textF.fontSize = 3.5f - 0.05f * text.Split(',').Length;
                textF.text = text;
            }

            if (textB.text != text)
            {
                // make the font size smaller if the text is too long based on the number of commas count ... for every comma, reduce the font size by 0.05 from the original size 3.5
                textB.fontSize = 3.5f - 0.05f * text.Split(',').Length;
                textB.text = text;
            }
        }

        #endregion

    }
}
