using System;
using UnityEngine;

namespace ForceDirectedGraph.DataStructure
{
    [Serializable]
    public class Link
    {

        #region Constructors

        /// <summary>
        /// Minimal constructor.
        /// </summary>
        /// <param name="firstNodeId">The first node connected to the edge of the link.</param>
        /// <param name="secondNodeId">The second node connected to the edge of the link.</param>
        public Link(Guid firstNodeId, Guid secondNodeId)
            : this(firstNodeId, secondNodeId, 0.5f, color: Color.white)
        {
        }

        /// <summary>
        /// Default constructor.
        /// <param name="firstNodeId">The first node connected to the edge of the link.</param>
        /// <param name="secondNodeId">The second node connected to the edge of the link.</param>
        /// <param name="width">Normalized width of the link [0-1].</param>
        /// <param name="color">The color used when representing the link.</param>
        /// </summary>
        public Link(Guid firstNodeId, Guid secondNodeId, float width, bool isOrderingConstraint = false, Color color = default, string CausalLinkCondition = "")
        {
            _FirstNodeId = firstNodeId;
            _SecondNodeId = secondNodeId;
            _Width = width;
            _Color = Color.white;
            _Condition = CausalLinkCondition;
            _IsOrderingConstraint = isOrderingConstraint;
        }

        /// <summary>
        /// Default constructor.
        /// <param name="firstNodeId">The first node connected to the edge of the link.</param>
        /// <param name="secondNodeId">The second node connected to the edge of the link.</param>
        /// <param name="width">Normalized width of the link [0-1].</param>
        /// <param name="color">The color used when representing the link.</param>
        /// </summary>
        public Link(Node firstNode, Node secondNode, float width, bool isOrderingConstraint = false, Color color = default, string CausalLinkCondition = "")
        : this(firstNode.Id, secondNode.Id, width, isOrderingConstraint, color, CausalLinkCondition)
        {
        }

        /// <summary>
        /// Clone constructor.
        /// </summary>
        /// <param name="link">Instance to clone.</param>
        public Link(Link link)
            : this(link.FirstNodeId, link.SecondNodeId, link.Width, link.IsOrderingConstraint, link.Color)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// The first node connected to the edge of the link.
        /// </summary>
        [SerializeField]
        [Tooltip("The first node connected to the edge of the link.")]
        private Guid _FirstNodeId;

        /// <summary>
        /// The first node connected to the edge of the link.
        /// </summary>
        public Guid FirstNodeId { get { return _FirstNodeId; } set { _FirstNodeId = value; } }



        /// <summary>
        /// The second node connected to the edge of the link.
        /// </summary>
        [SerializeField]
        [Tooltip("The second node connected to the edge of the link.")]
        private Guid _SecondNodeId;

        /// <summary>
        /// The second node connected to the edge of the link.
        /// </summary>
        public Guid SecondNodeId { get { return _SecondNodeId; } set { _SecondNodeId = value; } }



        /// <summary>
        /// Normalized width of the link [0-1].
        /// </summary>
        [SerializeField]
        [Tooltip("Normalized width of the link [0-1].")]
        private float _Width;

        /// <summary>
        /// Normalized width of the link [0-1].
        /// </summary>
        public float Width { get { return _Width; } }



        /// <summary>
        /// The color used when representing the link.
        /// </summary>
        [SerializeField]
        [Tooltip("The color used when representing the link.")]
        private Color _Color;

        /// <summary>
        /// The color used when representing the link.
        /// </summary>
        public Color Color { get { return _Color; } }



        /// <summary>
        /// Link Condition (if it is a causal link).
        /// </summary>
        [SerializeField]
        [Tooltip("Link Condition (if it is a causal link).")]
        private string _Condition;

        /// <summary>
        /// Link Condition (if it is a causal link).
        /// </summary>
        public string Condition { get { return _Condition; } set { _Condition = value; } }



        /// <summary>
        /// A boolean to check if the link is a Ordering Constraint or not(CausalLink). 
        /// </summary>
        [SerializeField]
        [Tooltip("A boolean to check if the link is a Ordering Constraint or not(CausalLink).")]
        private bool _IsOrderingConstraint;

        /// <summary>
        /// A boolean to check if the link is a Ordering Constraint or not(CausalLink).
        /// </summary>
        public bool IsOrderingConstraint { get { return _IsOrderingConstraint; } set { _IsOrderingConstraint = value; } }

        #endregion

    }
}
