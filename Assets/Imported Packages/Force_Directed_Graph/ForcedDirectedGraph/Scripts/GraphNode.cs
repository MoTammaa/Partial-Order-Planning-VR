﻿using ForceDirectedGraph.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace ForceDirectedGraph
{
    public class GraphNode : MonoBehaviour
    {

        #region Constants

        /// <summary>
        /// The maximum value the node's velocity can be at any time.
        /// </summary>
        private const float MAX_VELOCITY_MAGNITUDE = 400f;

        #endregion

        #region Initialization

        /// <summary>
        /// Executes once on start.
        /// </summary>
        private void Awake()
        {
            if (GetComponent<Rigidbody2D>() != null)
                Rigidbody2d = GetComponent<Rigidbody2D>();
            // else if (GetComponent<Rigidbody>() != null)
            //     Rigidbody = GetComponent<Rigidbody>();

            if (GetComponent<Draggable>() != null)
                Draggable = GetComponent<Draggable>();

            // Freeze rotation
            if (Rigidbody2d != null)
            {
                Rigidbody2d.angularVelocity = 0;
                Rigidbody2d.freezeRotation = true;
            }
            else if (Rigidbody != null)
            {
                Rigidbody.angularVelocity = Vector3.zero;
                Rigidbody.freezeRotation = true;
            }
        }

        /// <summary>
        /// Initializes the graph entity.
        /// </summary>
        /// <param name="node">The node being presented.</param>
        public void Initialize(Node node)
        {
            _Node = node;

            // Set the color
            if (GetComponent<SpriteRenderer>() != null)
                GetComponent<SpriteRenderer>().color = node.Color;

            // Set name
            UpdateName(node.Name);

            // Set preconditions
            UpdatePrecondions(node.Action.Preconditions);

            // Set effects
            UpdateEffects(node.Action.Effects);
        }
        void UpdateName(string name)
        {
            // update the text written on the block in this object >> Cube >> Operator Canvas F/B >> Background Button >> Text (TMP)
            transform.Find("Cube").Find("Operator Canvas F").Find("Background Button").Find("Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>().text = name;
            transform.Find("Cube").Find("Operator Canvas B").Find("Background Button").Find("Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>().text = name;
        }

        public void UpdatePrecondions(string preconditions)
        {
            // update the text written on the block in this object >> Cube >> Preconds Canvas >> Background Button >> Text (TMP)
            transform.Find("Cube").Find("Preconds Canvas").Find("Background Button").Find("Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>().text = preconditions;
        }

        public void UpdatePrecondions(List<POP.Literal> preconditions, Func<POP.Literal, string> literalToString = null)
        {
            literalToString ??= _Node.LiteralToString;
            // update the text written on the block in this object >> Cube >> Preconds Canvas >> Background Button >> Text (TMP)
            string preconds = "Preconditions: \n" + (preconditions.Count > 0 ? ">" : "") + string.Join("\n>", preconditions.Select(literalToString).ToArray());
            transform.Find("Cube").Find("Preconds Canvas").Find("Background Button").Find("Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>().text = preconds;
        }

        public void UpdateEffects(string effects)
        {
            // update the text written on the block in this object >> Cube >> Effects Canvas >> Background Button >> Text (TMP)
            transform.Find("Cube").Find("Effects Canvas").Find("Background Button").Find("Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>().text = effects;
        }

        public void UpdateEffects(List<POP.Literal> effects, Func<POP.Literal, string> literalToString = null)
        {
            literalToString ??= _Node.LiteralToString;
            // update the text written on the block in this object >> Cube >> Effects Canvas >> Background Button >> Text (TMP)
            string effectsString = "Effects: \n" + (effects.Count > 0 ? ">" : "") + string.Join("\n>", effects.Select(literalToString).ToArray());
            transform.Find("Cube").Find("Effects Canvas").Find("Background Button").Find("Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>().text = effectsString;
        }


        #endregion

        #region Fields/Properties

        /// <summary>
        /// The node being presented.
        /// </summary>
        [SerializeField]
        [Tooltip("The node being presented.")]
        private Node _Node;

        /// <summary>
        /// The node being presented.
        /// </summary>
        public Node Node { get { return _Node; } }



        /// <summary>
        /// References the rigid body that handles the movements of the node.
        /// </summary>
        private Rigidbody2D Rigidbody2d;
        private Rigidbody Rigidbody;

        /// <summary>
        /// References the draggable script that will notify us if the node is being dragged.
        /// </summary>
        private Draggable Draggable;



        /// <summary>
        /// List of all forces to apply.
        /// </summary>
        private List<Vector2> Forces;



        /// <summary>
        /// An instance of a Partial Plan Action.
        /// </summary>
        public POP.Action Action { get { return _Node.Action; } }

        #endregion

        #region Movement

        /// <summary>
        /// Apply forces to the node.
        /// </summary>
        /// <param name="applyImmediately">States whether we should apply the forces immediately or wait till the next frame.</param>
        public void ApplyForces(List<Vector2> forces, bool applyImmediately = false)
        {
            if (applyImmediately)
            {
                foreach (var force in forces)
                {
                    if (Rigidbody2d != null)
                        Rigidbody2d.AddForce(force);
                    else if (Rigidbody != null)
                        Rigidbody.AddForce(force);
                }
            }
            else
                Forces = forces;
        }

        /// <summary>
        /// Updates the forces applied to the node.
        /// </summary>
        private void Update()
        {
            bool DraggableNotNullAndBeingDragged = Draggable != null;
            if (DraggableNotNullAndBeingDragged) DraggableNotNullAndBeingDragged = Draggable.IsBeingDragged;



            // Check if the object is being dragged
            if (DraggableNotNullAndBeingDragged)
            {
                // Do nothing
            }

            // The object is not being dragged
            else
            {
                if (Rigidbody2d != null)
                    Rigidbody2d.velocity = Vector3.zero;
                else if (Rigidbody != null)
                    Rigidbody.velocity = Vector3.zero;

                Vector2 velocity = Vector2.zero;
                if (Forces != null)
                    foreach (var force in Forces)
                        velocity += force;

                velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, 0f, MAX_VELOCITY_MAGNITUDE);

                if (Rigidbody2d != null)
                    Rigidbody2d.AddForce(velocity);
                else if (Rigidbody != null)
                    Rigidbody.AddForce(velocity);
            }
            UpdateName(_Node.Name);
        }

        #endregion

    }
}