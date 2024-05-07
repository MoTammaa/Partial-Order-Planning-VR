using System;
using POP;
using UnityEngine;

namespace ForceDirectedGraph.DataStructure
{
    [Serializable]
    public class Node
    {

        #region Constructors

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public Node()
            : this(new POP.Action("Action", new System.Collections.Generic.List<POP.Literal> { }, new System.Collections.Generic.List<POP.Literal> { }))
        {
        }

        /// <summary>
        /// Minimal constructor.
        /// </summary>
        /// <param name="name">The displayed name of the node.</param>
        public Node(POP.Action action, POP.PartialPlan partialPlan = null, string name = null)
            : this(Guid.NewGuid(), action, partialPlan, name)
        {
        }

        /// <summary>
        /// Default constructor.
        /// <param name="id">Unique identifier used to link nodes together.</param>
        /// <param name="name">The displayed name of the node.</param>
        /// <param name="color">The color used when representing the node.</param>
        /// </summary>
        public Node(Guid id, POP.Action action, POP.PartialPlan partialPlan = null, string name = null)
        {
            _Id = id;
            _Name = name ?? (partialPlan != null ? partialPlan.ActionToString(action) : action.ToString());
            _Color = Color.white;
            _Action = action;
            _PartialPlan = partialPlan;
        }

        /// <summary>
        /// Clone constructor.
        /// </summary>
        /// <param name="node">Instance to clone.</param>
        public Node(Node node)
            : this(node.Id, node._Action, node._PartialPlan, node.Name)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Unique identifier used to link nodes together.
        /// </summary>
        [SerializeField]
        [Tooltip("Unique identifier used to link nodes together.")]
        private Guid _Id;

        /// <summary>
        /// Unique identifier used to link nodes together.
        /// </summary>
        public Guid Id { get { return _Id; } }



        /// <summary>
        /// The displayed name of the node.
        /// </summary>
        [SerializeField]
        [Tooltip("The displayed name of the node.")]
        private string _Name;

        /// <summary>
        /// The displayed name of the node.
        /// </summary>
        public string Name { get { return _Name; } }



        /// <summary>
        /// The color used when representing the node.
        /// </summary>
        [SerializeField]
        [Tooltip("The color used when representing the node.")]
        private Color _Color;

        /// <summary>
        /// The color used when representing the node.
        /// </summary>
        public Color Color { get { return _Color; } }


        /// <summary>
        /// The action that this node represents.
        /// </summary>
        [SerializeField]
        [Tooltip("The action that this node represents.")]
        private POP.Action _Action;

        /// <summary>
        /// The action that this node represents.
        /// </summary>
        public POP.Action Action { get { return _Action; } }



        /// <summary>
        /// A reference to the current Partial Plan.
        /// </summary>
        [SerializeField]
        [Tooltip("A reference to the current Partial Plan.")]
        private POP.PartialPlan _PartialPlan;


        #endregion

        #region Methods

        /// <summary>
        /// Uses the id of the cluster for hash coding.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Uses the id of the cluster for comparison.
        /// </summary>
        public override bool Equals(object obj)
        {
            Node item = obj as Node;
            return item?.Id == Id;
        }

        /// <summary>
        /// Displays the name of the node.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Convert Action to string.
        /// </summary>
        /// <param name="action">The action to convert.</param>
        /// <returns>The string representation of the action.</returns>
        public string ActionToString(POP.Action action)
        {
            return _PartialPlan.ActionToString(action);
        }

        /// <summary>
        /// Convert Literal to string.
        /// </summary>
        /// <param name="literal">The literal to convert.</param>
        /// <returns>The string representation of the literal.</returns>
        public string LiteralToString(POP.Literal literal)
        {
            return _PartialPlan.LiteralToString(literal);
        }

        /// <summary>
        /// Updates the node's name based on a new partial plan.
        /// </summary>
        public void UpdateName(Func<POP.Action, string> actionToString = null)
        {
            _Name = actionToString != null ? actionToString(_Action) : _Name;
        }

        /// <summary>
        /// Updates the node's name
        /// </summary>
        public void UpdateName(string name)
        {
            _Name = name;
        }

        #endregion

    }
}
