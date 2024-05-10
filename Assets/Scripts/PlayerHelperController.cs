using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using POP;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using System.Text;
using Action = POP.Action;
using Literal = POP.Literal;
using ForceDirectedGraph;

public class PlayerHelperController : MonoBehaviour
{
    private static PlayerHelperController Instance;
    private static PlanningProblem problem; // will be set by the POPEngineDriverController
    public static PlanningProblem Problem
    {
        get { return problem; }
        set
        {
            problem = value;
            // planner = new Planner(problem, POPEngineDriverController.GetSearchStrategyFromPlayerPrefs(), POPEngineDriverController.GetMaxDepthFromPlayerPrefs());
            // partialPlan = planner.PartialPlan;
            // partialPlan = planner.POP();
            // print("partialPlan: " + partialPlan);
            // InitOperatorsMenu();
            // ResetVariables();
            // DoneChoosingOperator();
        }
    }

    ////// new //////////////////////////////////////////

    private static POPController popController;
    public static POPController PopController
    {
        get { return popController; }
        set
        {
            popController = value;
            InitAgenda();
            // add the start and finish action nodes to the graph network
            POPEngineDriverController.Graph.CancelForces();
            GameObject StartNodeObject = gameObjects["AgendaCanvas"].transform.parent.Find("Actions").Find("Start").gameObject;
            GameObject FinishNodeObject = gameObjects["AgendaCanvas"].transform.parent.Find("Actions").Find("Finish").gameObject;
            // Add the actions to the network
            ForceDirectedGraph.DataStructure.Node node = new ForceDirectedGraph.DataStructure.Node(PopController.Planner.PartialPlan.GetActionByName("Start"), popController.Planner.PartialPlan);
            POPEngineDriverController.Network.Nodes.Add(node);
            POPEngineDriverController.Graph.AddDisplayNode(node, Color.green, existingNode: StartNodeObject);

            ForceDirectedGraph.DataStructure.Node node2 = new ForceDirectedGraph.DataStructure.Node(PopController.Planner.PartialPlan.GetActionByName("Finish"), popController.Planner.PartialPlan);
            POPEngineDriverController.Network.Nodes.Add(node2);
            POPEngineDriverController.Graph.AddDisplayNode(node2, Color.red, existingNode: FinishNodeObject);
            //Update the graph text
            POPEngineDriverController.UpdateNodesText(value.Planner.PartialPlan);
        }
    }

    private static PartialPlan ControllerPartialPlan { get { return popController.Planner.PartialPlan; } }

    private static Dictionary<string, GameObject> gameObjects = new();
    private static List<Tuple<Action, Literal>> tempAgendaList = new();

    private static List<List<Operator>> achievers = new();

    private static int currentAgendaIndex = 1;
    private static int currentAchieverIdx = 1;
    private static int currentAchieverJdx = 1;
    private static int currentCausalLinkIndex = 1;
    private static int currentOrderingConstraintIndex = 1;

    private static Action ThreatAction = null;
    private static CausalLink ThreatenedLink = null;

    private static List<CausalLink> CausalLinks = new();
    private static List<Tuple<Action, Action>> OrderingConstraints = new();

    private static bool ThereIsThreat = false;


    //////////////////////////////////////////////////////
    private static List<Operator> operators = new(); // will be set by the POPEngineDriverController
    public static List<Operator> Operators { get { return operators; } }
    private static List<List<string>> variables = new(); // will be set by the POPEngineDriverController

    private static Action currentAction;

    private static PartialPlan partialPlan { get { return popController.Planner.PartialPlan; } }
    public static PartialPlan PartialPlan { get { return partialPlan; } }

    private static Planner planner;

    private static int currentOperatorIndex = 1;
    private static int currentVariableIdxInVariables = 1;
    private static int currentVariableJdxInVariables = 1;
    private static int currentParameterIndex = 0;
    private static int numOfOperators = 0;

    private static GameObject OperatorCanvas;
    private static GameObject OperatorCanvasBody;
    private static GameObject OperatorDescriptionTextCanvas;
    private static GameObject OperatorButtons { get { return OperatorCanvas?.transform.Find("BodyTitle")?.Find("BodyCanvas")?.Find("Buttons")?.gameObject; } }
    private static GameObject VariableCanvas;
    private static GameObject VariableDescriptionTextCanvas;
    private static GameObject VariableOptionButtons { get { return VariableButtons?.transform.Find("Options")?.gameObject; } }
    private static GameObject VariableButtons { get { return VariableCanvas?.transform.Find("BodyTitle")?.Find("BodyCanvas")?.Find("Buttons")?.gameObject; } }
    private static GameObject RequestButton;
    private static GameObject VariablesNavigation { get { return VariableButtons?.transform.Find("Navigation")?.gameObject; } }
    private static GameObject VariablesNextButton { get { return VariableButtons?.transform.Find("Next")?.gameObject; } }
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        OperatorCanvas = GameObject.Find("Operators Menu");
        OperatorCanvasBody = OperatorCanvas?.transform.Find("BodyTitle")?.gameObject;
        OperatorDescriptionTextCanvas = OperatorCanvas?.transform.Find("BodyTitle")?.Find("DescriptionCanvas")?.Find("Text")?.gameObject;
        if (OperatorCanvas == null || OperatorDescriptionTextCanvas == null || OperatorButtons == null)
        {
            print("One of the OperatorCanvas, OperatorDescriptionTextCanvas, OperatorButtons is null");
        }
        VariableCanvas = OperatorCanvas.transform.Find("Variables Menu").gameObject;
        VariableDescriptionTextCanvas = VariableCanvas?.transform.Find("BodyTitle")?.Find("DescriptionCanvas")?.Find("Text")?.gameObject;
        RequestButton = VariableButtons?.transform.Find("Request")?.gameObject;
        if (VariableCanvas == null || VariableDescriptionTextCanvas == null || VariableOptionButtons == null || VariableButtons == null || RequestButton == null)
        {
            print("One of the VariableCanvas, VariableDescriptionTextCanvas, VariableButtons is null");
        }
        RequestButton.SetActive(false);
        VariableCanvas.SetActive(false);

        gameObjects.TryAdd("AgendaCanvas", GameObject.Find("Agenda Menu"));
        gameObjects.TryAdd("AgendaDescriptionTextCanvas", gameObjects["AgendaCanvas"]?.transform.Find("BodyTitle")?.Find("DescriptionCanvas")?.Find("Text")?.gameObject);
        gameObjects.TryAdd("AgendaButtons", gameObjects["AgendaCanvas"]?.transform.Find("BodyTitle")?.Find("BodyCanvas")?.Find("Buttons")?.gameObject);
        gameObjects.TryAdd("AgendaDoneButton", gameObjects["AgendaCanvas"]?.transform.Find("BodyTitle")?.Find("BodyCanvas")?.Find("Done")?.gameObject);
        gameObjects.TryAdd("AgendaUpDownButtons", gameObjects["AgendaButtons"]?.transform.Find("UpDown")?.gameObject);

        if (gameObjects["AgendaCanvas"] == null || gameObjects["AgendaDescriptionTextCanvas"] == null
        || gameObjects["AgendaButtons"] == null || gameObjects["AgendaDoneButton"] == null
        || gameObjects["AgendaUpDownButtons"] == null)
        {
            print("One of the AgendaCanvas, AgendaDescriptionTextCanvas, AgendaButtons, AgendaDoneButton is null");
        }

        gameObjects.TryAdd("AchieversCanvas", gameObjects["AgendaCanvas"]?.transform.Find("Achievers Menu")?.gameObject);
        gameObjects.TryAdd("AchieversDescriptionTextCanvas", gameObjects["AchieversCanvas"]?.transform.Find("BodyTitle")?.Find("DescriptionCanvas")?.Find("Text")?.gameObject);
        gameObjects.TryAdd("AchieversButtons", gameObjects["AchieversCanvas"]?.transform.Find("BodyTitle")?.Find("BodyCanvas")?.Find("Buttons")?.gameObject);
        gameObjects.TryAdd("AchieversDoneButton", gameObjects["AchieversButtons"]?.transform.Find("Done")?.gameObject);
        gameObjects.TryAdd("AchieversCancelButton", gameObjects["AchieversButtons"]?.transform.Find("Cancel")?.gameObject);
        gameObjects.TryAdd("AchieversNavigation", gameObjects["AchieversButtons"]?.transform.Find("Navigation")?.gameObject);
        gameObjects.TryAdd("AchieversOptionButtons", gameObjects["AchieversButtons"]?.transform.Find("Options")?.gameObject);
        gameObjects.TryAdd("AchieversRequestButton", gameObjects["AchieversButtons"]?.transform.Find("Request")?.gameObject);
        gameObjects.TryAdd("AchieversAlertTextCanvas", gameObjects["AchieversCanvas"]?.transform.Find("BodyTitle")?.Find("AlertCanvas")?.Find("Text")?.gameObject);

        if (gameObjects["AchieversCanvas"] == null || gameObjects["AchieversDescriptionTextCanvas"] == null
        || gameObjects["AchieversButtons"] == null || gameObjects["AchieversDoneButton"] == null
        || gameObjects["AchieversNavigation"] == null || gameObjects["AchieversOptionButtons"] == null
        || gameObjects["AchieversRequestButton"] == null || gameObjects["AchieversAlertTextCanvas"] == null
        || gameObjects["AchieversCancelButton"] == null)
        {
            print("One of the AchieversCanvas, AchieversDescriptionTextCanvas, AchieversButtons, AchieversDoneButton, AchieversNavigation, AchieversOptionButtons, AchieversNavigation is null");
        }

        gameObjects.TryAdd("ThreatsDescriptionTextCanvas", GameObject.Find("PC Setup").transform.Find("ThreatsMonitor").Find("MonitorCanvas").Find("Text").gameObject);
        if (gameObjects["ThreatsDescriptionTextCanvas"] == null)
        {
            print("ThreatsDescriptionTextCanvas is null");
        }

        gameObjects.TryAdd("CausalLinksCanvas", GameObject.Find("CausalLinks Menu"));
        gameObjects.TryAdd("CausalLinksDescriptionTextCanvas", gameObjects["CausalLinksCanvas"]?.transform.Find("BodyTitle")?.Find("DescriptionCanvas")?.Find("Text")?.gameObject);
        gameObjects.TryAdd("CausalLinksButtons", gameObjects["CausalLinksCanvas"]?.transform.Find("BodyTitle")?.Find("BodyCanvas")?.Find("Buttons")?.gameObject);
        gameObjects.TryAdd("CausalLinksDeleteButton", gameObjects["CausalLinksButtons"]?.transform.Find("Delete")?.gameObject);
        gameObjects.TryAdd("CausalLinksUpDownButtons", gameObjects["CausalLinksButtons"]?.transform.Find("UpDown")?.gameObject);
        if (gameObjects["CausalLinksCanvas"] == null || gameObjects["CausalLinksDescriptionTextCanvas"] == null
        || gameObjects["CausalLinksButtons"] == null || gameObjects["CausalLinksDeleteButton"] == null
        || gameObjects["CausalLinksUpDownButtons"] == null)
        {
            print("One of the CausalLinksCanvas, CausalLinksDescriptionTextCanvas, CausalLinksButtons, CausalLinksDeleteButton, CausalLinksUpDownButtons is null");
        }

        gameObjects.TryAdd("OrderingConstraintsCanvas", GameObject.Find("OrderingConstraints Menu"));
        gameObjects.TryAdd("OrderingConstraintsDescriptionTextCanvas", gameObjects["OrderingConstraintsCanvas"]?.transform.Find("BodyTitle")?.Find("DescriptionCanvas")?.Find("Text")?.gameObject);
        gameObjects.TryAdd("OrderingConstraintsButtons", gameObjects["OrderingConstraintsCanvas"]?.transform.Find("BodyTitle")?.Find("BodyCanvas")?.Find("Buttons")?.gameObject);
        gameObjects.TryAdd("OrderingConstraintsDeleteButton", gameObjects["OrderingConstraintsButtons"]?.transform.Find("Delete")?.gameObject);
        gameObjects.TryAdd("OrderingConstraintsUpDownButtons", gameObjects["OrderingConstraintsButtons"]?.transform.Find("UpDown")?.gameObject);
        if (gameObjects["OrderingConstraintsCanvas"] == null || gameObjects["OrderingConstraintsDescriptionTextCanvas"] == null
        || gameObjects["OrderingConstraintsButtons"] == null || gameObjects["OrderingConstraintsDeleteButton"] == null
        || gameObjects["OrderingConstraintsUpDownButtons"] == null)
        {
            print("One of the OrderingConstraintsCanvas, OrderingConstraintsDescriptionTextCanvas, OrderingConstraintsButtons, OrderingConstraintsDeleteButton, OrderingConstraintsUpDownButtons is null");
        }

        gameObjects.TryAdd("EmergencyAlarm", GameObject.Find("PC Setup").transform.Find("Emergency-Alarm").gameObject);
        if (gameObjects["EmergencyAlarm"] == null)
        {
            print("EmergencyAlarm is null");
        }

        // gameObjects["AchieversCanvas"].SetActive(false);
        gameObjects["AchieversRequestButton"].SetActive(false);
        gameObjects["AchieversAlertTextCanvas"].SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Initialization
    public static void InitOperatorsMenu()
    {
        // Set the operators menu
        GameObject operatorsMenu = GameObject.Find("Operators Menu");
        if (operatorsMenu is null)
        {
            Debug.LogError("Operators Menu is null");
            return;
        }

        // Set the operators menu description
        GameObject descriptionCanvas = operatorsMenu.transform.Find("BodyTitle").Find("DescriptionCanvas").Find("Text").gameObject;
        descriptionCanvas.GetComponent<UnityEngine.UI.Text>().text = "Choose the operators to use in the planning problem";

        // create the operators buttons by cloning B1
        GameObject buttons = operatorsMenu.transform.Find("BodyTitle").Find("BodyCanvas").Find("Buttons").gameObject;
        GameObject B0 = buttons.transform.Find("B0").gameObject;
        foreach (POP.Operator oper in Problem.Operators)
        {
            Operators.Add(oper);
            GameObject button = Instantiate(B0, buttons.transform);
            button.name = "B" + (buttons.transform.childCount - 3);
            button.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = oper.Name + "(" + string.Join(",", oper.Variables.Select(v => "_")) + ")";
            button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y + 0.1f * (buttons.transform.childCount - 4), button.transform.localPosition.z);
            button.transform.localScale = new Vector3(1, 1, 1);
            button.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);
        }

        // initialize the operator description text
        OpMoveDown();

    }

    public static void InitNextVariableMenu()
    {
        if (currentParameterIndex > 0)
        {
            // save the variables to the current action
            if (currentVariableIdxInVariables - 1 - (1 * (currentVariableJdxInVariables % 2)) >= 0)
                currentAction.BoundVariables[currentAction.Variables[currentParameterIndex - 1]] = variables[currentVariableJdxInVariables - 1][currentVariableIdxInVariables - 1 - (1 * (currentVariableJdxInVariables % 2))];
        }

        if (currentAction.Variables.Length == 0 || currentParameterIndex == currentAction.Variables.Length)
        {
            DoneChoosingVariables();
            print("No more variables to choose");
            return;
        }


        currentParameterIndex++;

        // intitialize variables
        ResetVariables();

        // Set the variables menu description
        VariableDescriptionTextCanvas.GetComponent<UnityEngine.UI.Text>().text =
            $"Choose the value for the {currentParameterIndex}{currentParameterIndex switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" }} parameter \"{currentAction.Variables[currentParameterIndex - 1]}\""
            + $"\nNew Action: {currentAction}";

        // clear the previous variables buttons
        foreach (Transform child in VariableOptionButtons.transform)
        {
            if (child.name.StartsWith("B") && child.name != "B11")
            {
                Destroy(child.gameObject);
            }
        }

        // create the variables buttons by cloning B1
        GameObject buttonTemplate = VariableButtons.transform.Find("B").gameObject;
        GameObject B11 = VariableOptionButtons.transform.Find("B11")?.gameObject ?? Instantiate(buttonTemplate, VariableOptionButtons.transform);
        B11.name = "B11";
        B11.transform.localScale = new(1, 1, 1);
        for (int i = 0; i < Math.Min(2, variables.Count); i++)
        {
            for (int j = 0; j < variables[i].Count; j++)
            {
                string variable = variables[i][j];

                GameObject button = Instantiate(B11, VariableOptionButtons.transform);
                button.name = $"B{j + 1 + (1 * ((i + 1) % 2))}{i + 1}"; // e.g. "B11", "B12", "B21", "B22", etc.
                button.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = variable;
                button.transform.localPosition = new Vector3(button.transform.localPosition.x + i * 0.5f, button.transform.localPosition.y + 0.1f * (j + (1 * ((i + 1) % 2))), button.transform.localPosition.z);
                button.transform.localScale = new Vector3(1, 1, 1);
                button.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);
            }
        }

        // initialize highlights
        currentVariableIdxInVariables = 1;
        currentVariableJdxInVariables = 1;
        VarMoveDown();
    }

    private static void InitAgenda(bool show = true)
    {
        tempAgendaList = new();

        // Set Visibility
        if (show)
        {
            gameObjects["AgendaCanvas"].SetActive(true);
            gameObjects["AgendaCanvas"].transform.Find("BodyTitle").gameObject.SetActive(true);
            gameObjects["AchieversCanvas"].SetActive(false);
        }

        // Set the Agenda menu
        GameObject AgendaCanvas = gameObjects["AgendaCanvas"];
        if (AgendaCanvas == null) { Debug.LogError("Agenda Menu is null"); return; }

        // Set the Agenda menu description
        GameObject descriptionCanvas = gameObjects["AgendaDescriptionTextCanvas"];
        descriptionCanvas.GetComponent<UnityEngine.UI.Text>().text = "Choose any of the Pairs of Actions/Preconditions to try to achieve.";

        // clear the previous Agenda buttons
        foreach (Transform child in gameObjects["AgendaButtons"].transform)
        {
            if (child.name.StartsWith("B") && child.name != "B0")
            {
                Destroy(child.gameObject);
            }
        }

        // create the Agenda buttons by cloning B0
        GameObject buttons = gameObjects["AgendaButtons"];
        GameObject B0 = buttons.transform.Find("B0").gameObject;
        foreach (Tuple<Action, Literal> pair in popController.Planner.Agenda)
        {
            tempAgendaList.Add(pair);
        }
        for (int i = 0; i < tempAgendaList.Count; i++)
        {
            Tuple<Action, Literal> pair = tempAgendaList[i];

            GameObject button = Instantiate(B0, buttons.transform);
            button.name = "B" + (i + 1);
            button.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = $"A:{popController.ActionToString(pair.Item1)} / PreC:{popController.LiteralToString(pair.Item2)}";
            button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y + 0.125f * i, button.transform.localPosition.z);
            button.transform.localScale = new Vector3(1.8f, 1, 1);
            button.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = (i == 0) ? new Color(1, 0, 0) : new Color(1, 1, 1);
        }

        if (tempAgendaList.Count == 0)
        {
            // change the Title text
            gameObjects["AgendaCanvas"].transform.Find("BodyTitle").Find("TitleCanvas").Find("Text").GetComponent<UnityEngine.UI.Text>().text = "No More Items in Your Agenda!";
            // change the description text
            descriptionCanvas.GetComponent<UnityEngine.UI.Text>().text = "Goal Achieved! Go Celebrate!";
            // hide the Done button
            gameObjects["AgendaDoneButton"].SetActive(false);
            // hide the UpDown buttons
            gameObjects["AgendaUpDownButtons"].SetActive(false);

        }


        // initialize the operator description text
        currentAgendaIndex = 1;
        AgendaMoveDown();
    }

    public static void InitAchieversMenu()
    {
        // show the next, navigation buttons, cancel, done and the achievers options AND hide the request button & alert text
        gameObjects["AchieversCanvas"].SetActive(true);
        gameObjects["AchieversNavigation"].SetActive(true);
        gameObjects["AchieversDoneButton"].SetActive(true);
        gameObjects["AchieversOptionButtons"].SetActive(true);
        gameObjects["AchieversRequestButton"].SetActive(false);
        gameObjects["AchieversAlertTextCanvas"].SetActive(false);
        gameObjects["AchieversCancelButton"].SetActive(true);


        // Set the Achievers List
        SetAchievers();

        // Set the Achievers menu
        GameObject AchieversCanvas = gameObjects["AchieversCanvas"];
        if (AchieversCanvas == null) { Debug.LogError("Achievers Menu is null"); return; }

        // Set the Achievers menu description
        GameObject descriptionCanvas = gameObjects["AchieversDescriptionTextCanvas"];
        descriptionCanvas.GetComponent<UnityEngine.UI.Text>().text = $"Choose any of the Achievers for the selected pair of Actions/Preconditions:\n {popController.ActionToString(tempAgendaList[currentAgendaIndex - 1].Item1)} / {popController.LiteralToString(tempAgendaList[currentAgendaIndex - 1].Item2)}";

        // clear the previous Achievers buttons
        foreach (Transform child in gameObjects["AchieversOptionButtons"].transform)
        {
            if (child.name.StartsWith("B"))
            {
                Destroy(child.gameObject);
            }
        }

        // create the Achievers buttons by cloning B1
        GameObject buttonTemplate = gameObjects["AchieversButtons"].transform.Find("B").gameObject;
        for (int i = 0; i < Math.Min(2, achievers.Count); i++)
        {
            for (int j = 0; j < achievers[i].Count; j++)
            {
                Action actionAchiever = (achievers[i][j] is Action action) ? action : null;
                string achiever = (actionAchiever is not null) ? popController.ActionToString(actionAchiever) : achievers[i][j].ToString();
                GameObject button = Instantiate(buttonTemplate, gameObjects["AchieversOptionButtons"].transform);
                button.name = $"B{j + 1}{i + 1}"; // e.g. "B11", "B12", "B21", "B22", etc.
                button.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = achiever;
                button.transform.localPosition = new Vector3(button.transform.localPosition.x + i * 0.75f, button.transform.localPosition.y + 0.1f * j, button.transform.localPosition.z);
                button.transform.localScale = new Vector3(1.8f, 1, 1);
                button.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = (i == 0 && j == 0) ? new Color(1, 0, 0) : new Color(1, 1, 1);
            }
        }

        // initialize highlights
        currentAchieverIdx = 1;
        currentAchieverJdx = 1;
        AchieversMoveDown();
    }

    public static void InitCausalLinksMenu()
    {
        // reset the CausalLinks List
        CausalLinks = new();

        // Set the CausalLinksCanvas
        GameObject CausalLinksMenu = gameObjects["CausalLinksCanvas"];
        if (CausalLinksMenu == null) { Debug.LogError("CausalLinksCanvas is null"); return; }

        // Set the CausalLinks menu description
        GameObject descriptionCanvas = gameObjects["CausalLinksDescriptionTextCanvas"];
        descriptionCanvas.GetComponent<UnityEngine.UI.Text>().text = "Choose any of the Causal Links to delete.";

        // clear the previous CausalLinks buttons
        foreach (Transform child in gameObjects["CausalLinksButtons"].transform)
        {
            if (child.name.StartsWith("B") && child.name != "B0")
            {
                Destroy(child.gameObject);
            }
        }

        // create the CausalLinks buttons by cloning B0
        GameObject buttons = gameObjects["CausalLinksButtons"];
        GameObject B0 = buttons.transform.Find("B0").gameObject;
        foreach (CausalLink link in popController.Planner.PartialPlan.CausalLinks)
        {
            CausalLinks.Add(link);
        }

        for (int i = 0; i < CausalLinks.Count; i++)
        {
            CausalLink link = CausalLinks[i];

            GameObject button = Instantiate(B0, buttons.transform);
            button.name = "B" + (i + 1);
            button.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = popController.Planner.PartialPlan.CausalLinkToString(link);
            button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y + 0.125f * i, button.transform.localPosition.z);
            button.transform.localScale = new Vector3(2.9f, 1, 1);
            button.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = i == 0 ? new Color(1, 0, 0) : new Color(1, 1, 1);
        }

        // initialize the CausalLinks description text
        currentCausalLinkIndex = 1;
        CausalLinksMoveDown();
    }

    public static void InitOrderingConstraintsMenu()
    {
        // reset the OrderingConstraints List
        SetOrderingConstraints();

        // Set the OrderingConstraintsCanvas
        GameObject OrderingConstraintsMenu = gameObjects["OrderingConstraintsCanvas"];
        if (OrderingConstraintsMenu == null) { Debug.LogError("OrderingConstraintsCanvas is null"); return; }

        // Set the OrderingConstraints menu description
        GameObject descriptionCanvas = gameObjects["OrderingConstraintsDescriptionTextCanvas"];
        descriptionCanvas.GetComponent<UnityEngine.UI.Text>().text = "Note: Any Ordering Constraint with Start or Finish will not be shown, And any one implied by a causal link will not be shown, as it cannot be deleted.";

        // clear the previous OrderingConstraints buttons
        foreach (Transform child in gameObjects["OrderingConstraintsButtons"].transform)
        {
            if (child.name.StartsWith("B") && child.name != "B0")
            {
                Destroy(child.gameObject);
            }
        }

        // create the OrderingConstraints buttons by cloning B0
        GameObject buttons = gameObjects["OrderingConstraintsButtons"];
        GameObject B0 = buttons.transform.Find("B0").gameObject;
        for (int i = 0; i < OrderingConstraints.Count; i++)
        {
            Tuple<Action, Action> pair = OrderingConstraints[i];

            GameObject button = Instantiate(B0, buttons.transform);
            button.name = "B" + (i + 1);
            button.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = OrderingConstraintToString(pair);
            button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y + 0.125f * i, button.transform.localPosition.z);
            button.transform.localScale = new Vector3(2.9f, 1, 1);
            button.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = i == 0 ? new Color(1, 0, 0) : new Color(1, 1, 1);
        }

        // initialize the OrderingConstraints description text
        OrderingConstraintsMoveDown();
    }

    #endregion

    #region Flow Control(Done, ...etc)
    public static void DoneChoosingOperator()
    {
        // reset the current parameter index
        currentParameterIndex = 0;

        // hide the operators menu
        OperatorCanvasBody.SetActive(false);

        // show the variables menu
        VariableCanvas.SetActive(true);

        // Initialize the current action
        print("currentOperatorIndex: " + currentOperatorIndex);
        print("Operators.Count: " + Operators.Count);
        currentAction = planner.createAction(Operators[currentOperatorIndex - 1], planner.PartialPlan.BindingConstraints);

        // Initialize the variables menu
        InitNextVariableMenu();
    }

    public static void CancelChoosingVariables()
    {
        // hide the request button
        RequestButton.SetActive(false);
        // show the navigation & next buttons
        VariablesNavigation.SetActive(true);
        VariablesNextButton.SetActive(true);

        // hide the variables menu
        VariableCanvas.SetActive(false);

        // show the operators menu
        OperatorCanvasBody.SetActive(true);

        foreach (var item in currentAction.BoundVariables)
        {
            print(item.Key + " : " + item.Value);
        }

        // reset the current action
        currentAction = null;
    }

    private static void DoneChoosingVariables()
    {
        // update the description 
        VariableDescriptionTextCanvas.GetComponent<UnityEngine.UI.Text>().text = $"New Added Action: {currentAction}\n\nPlease Press Request to spawn and add the action to the plan. \n\n The action will get infront of you shortly.";
        // hide the next , navigation buttons and the variables options
        VariablesNextButton.SetActive(false);
        VariablesNavigation.SetActive(false);
        VariableOptionButtons.SetActive(false);

        // show the request button
        RequestButton.SetActive(true);
    }

    public static void RequestAction()
    {
        // hide the request button
        RequestButton.SetActive(false);

        // show the next, navigation buttons and the variables options
        VariablesNextButton.SetActive(true);
        VariablesNavigation.SetActive(true);
        VariableOptionButtons.SetActive(true);

        // hide the variables menu
        VariableCanvas.SetActive(false);



        // add the current action to the partial plan
        partialPlan.Actions.Add(currentAction);

        // add the current action's binding constraints to the partial plan
        foreach (var item in currentAction.BoundVariables)
        {
            partialPlan.BindingConstraints.setEqual(item.Key, item.Value);
        }

        Instance.StartCoroutine(InstantiateNewAction());

        // reset the current action
        currentAction = null;

        // reset the current parameter index
        currentParameterIndex = 0;

        // reset the current operator index
        currentOperatorIndex = 1;

        // reset the variables
        ResetVariables();

        // reset the variables menu
        VariableDescriptionTextCanvas.GetComponent<UnityEngine.UI.Text>().text = "";

        // initialize the operators menu
        OpMoveDown();
    }

    private static IEnumerator InstantiateNewAction()
    {
        // create the operators blocks by using the prefab "NF-Operator-Green" in the Prefabs folder
        GameObject operatorBlock = Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Prefabs/NF-Operator-Green.prefab", typeof(GameObject)) as GameObject, OperatorCanvas.transform.parent.Find("Actions").transform);
        operatorBlock.name = "Action" + (++numOfOperators);
        Vector3 offset = new Vector3(OperatorCanvas.transform.localPosition.x, OperatorCanvas.transform.localPosition.y + 10.0f, OperatorCanvas.transform.localPosition.z - 0.5f);
        operatorBlock.transform.localPosition = new Vector3(0.0f, 20.0f, 1.1f) + offset;

        //get position of the last action
        Vector3 lastActionOriginal = operatorBlock.transform.localPosition;
        Vector3 lastActionPosition = operatorBlock.transform.localPosition;

        while (Math.Sqrt(Math.Pow(lastActionPosition.x - lastActionOriginal.x, 2) + Math.Pow(lastActionPosition.z - lastActionOriginal.z, 2)) < 1.0f)
        {
            lastActionPosition = operatorBlock.transform.localPosition;
            yield return new WaitForSeconds(3.0f);
        }

        // show the operators menu
        OperatorCanvasBody.SetActive(true);
    }
    //================================================================================================================================================================
    public static void DoneChoosingAgenda()
    {
        // show the achievers menu
        if (gameObjects.ContainsKey("AchieversCanvas") && gameObjects["AchieversCanvas"] != null)
        {
            gameObjects["AchieversCanvas"].SetActive(true);
        }
        else
        {
            Debug.Log("AchieversCanvas is not available or not initialized yet.");
            return;
        }
        // hide the agenda menu body
        if (gameObjects["AchieversCanvas"].activeSelf)
        {
            gameObjects["AgendaCanvas"].transform.Find("BodyTitle").gameObject.SetActive(false);
        }

        // initialize the achievers menu
        InitAchieversMenu();
    }

    public static void CancelChoosingAchievers()
    {
        // hide the request button
        gameObjects["AchieversRequestButton"].SetActive(false);
        // show the navigation & next buttons
        gameObjects["AchieversNavigation"].SetActive(true);
        gameObjects["AchieversDoneButton"].SetActive(true);

        // hide the achievers menu
        gameObjects["AchieversCanvas"].SetActive(false);

        // show the operators menu
        gameObjects["AgendaCanvas"].transform.Find("BodyTitle").gameObject.SetActive(true);
    }

    public static void DoneChoosingAchievers()
    {
        // hide the request button
        gameObjects["AchieversRequestButton"].SetActive(currentAchieverJdx == 1);

        // hide the next, navigation buttons and the variables options
        gameObjects["AchieversNavigation"].SetActive(false);
        gameObjects["AchieversDoneButton"].SetActive(false);
        gameObjects["AchieversOptionButtons"].SetActive(false);

        if (currentAchieverJdx > 1) // using existing action
        {
            // // hide achievers menu
            // gameObjects["AchieversCanvas"].SetActive(false);

            // // show the agenda menu
            // gameObjects["AgendaCanvas"].transform.Find("BodyTitle").gameObject.SetActive(true);

            // // initialize the agenda menu
            // AgendaMoveDown();
            Instance.StartCoroutine(ApplyAchiever(false, currentAgendaIndex));
        }
    }

    public static void RequestNewAchiever()
    {
        // hide the request button
        gameObjects["AchieversRequestButton"].SetActive(false);

        // show the next, navigation buttons and the variables options
        gameObjects["AchieversNavigation"].SetActive(true);
        gameObjects["AchieversDoneButton"].SetActive(true);
        gameObjects["AchieversOptionButtons"].SetActive(true);

        // hide the achievers menu
        gameObjects["AchieversCanvas"].SetActive(false);

        // show the agenda menu
        gameObjects["AgendaCanvas"].transform.Find("BodyTitle").gameObject.SetActive(true);

        gameObjects["AgendaCanvas"].SetActive(false);

        Instance.StartCoroutine(InstantiateNewAchiever(currentAgendaIndex));

        // reset the current agenda index
        currentAgendaIndex = 1;

        // initialize the agenda menu
        AgendaMoveDown();
    }

    public static IEnumerator InstantiateNewAchiever(int AgendaPairIndex)
    {
        // create the operators blocks by using the prefab "NF-Operator-Green" in the Prefabs folder
        GameObject Actions = gameObjects["AgendaCanvas"].transform.parent.Find("Actions").gameObject;
        GameObject operatorBlock = Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Prefabs/NF-Operator-Green.prefab", typeof(GameObject)) as GameObject, Actions.transform);
        operatorBlock.name = "Action" + (++numOfOperators);
        GameObject RoundSpawnPlace = GameObject.Find("Mid_PlayArea_RoundSpawnObject")?.transform.Find("HD_Rounded_Wall_4B").gameObject;
        Vector3 offset = new Vector3(RoundSpawnPlace.transform.position.x, RoundSpawnPlace.transform.position.y + 10.0f, RoundSpawnPlace.transform.position.z - 0.5f);
        operatorBlock.transform.position = new Vector3(0.0f, 20.0f, -0.1f) + offset;

        // update name on the block
        operatorBlock.transform.GetComponent<GraphNode>().Node.UpdateName(achievers[currentAchieverJdx - 1][currentAchieverIdx - 1].Name + "(...)\nPut in graph area to link.");
        operatorBlock.transform.GetComponent<GraphNode>().UpdatePrecondions(string.Join(", ", achievers[currentAchieverJdx - 1][currentAchieverIdx - 1].Preconditions));
        operatorBlock.transform.GetComponent<GraphNode>().UpdateEffects(string.Join(", ", achievers[currentAchieverJdx - 1][currentAchieverIdx - 1].Effects));

        //get position of the last action
        Vector3 lastActionOriginal = operatorBlock.transform.localPosition;
        Vector3 lastActionPosition = operatorBlock.transform.localPosition;

        // set ArrowController.stage to ActionsSpawn
        ArrowsController.stage = ArrowsController.Stage.ActionsSpawn;
        while (Math.Sqrt(Math.Pow(lastActionPosition.x - lastActionOriginal.x, 2) + Math.Pow(lastActionPosition.z - lastActionOriginal.z, 2)) < 1.0f)
        {
            lastActionPosition = operatorBlock.transform.localPosition;
            yield return new WaitForSeconds(1.0f);
        }

        // show the agenda menu
        gameObjects["AgendaCanvas"].SetActive(true);

        // wait till it is near the graph nodes (start or finish) then link it
        Vector3 startNode = Actions.transform.Find("Start").transform.position;
        Vector3 finishNode = Actions.transform.Find("Finish").transform.position;
        lastActionPosition = operatorBlock.transform.position;

        print("waiting");
        // set ArrowController.stage to GraphArea
        ArrowsController.stage = ArrowsController.Stage.GraphArea;
        while (Math.Sqrt(Math.Pow(lastActionPosition.x - startNode.x, 2) + Math.Pow(lastActionPosition.z - startNode.z, 2)) > 7.0f &&
               Math.Sqrt(Math.Pow(lastActionPosition.x - finishNode.x, 2) + Math.Pow(lastActionPosition.z - finishNode.z, 2)) > 7.0f)
        {
            lastActionPosition = operatorBlock.transform.position;
            print($"lastActionPosition: {lastActionPosition}, startNode: {startNode}, difference: {Math.Sqrt(Math.Pow(lastActionPosition.x - startNode.x, 2) + Math.Pow(lastActionPosition.z - startNode.z, 2))}");
            yield return new WaitForSeconds(3.0f);
        }

        // apply the achiever
        yield return Instance.StartCoroutine(ApplyAchiever(true, AgendaPairIndex, operatorBlock));

    }

    //================================================================================================================================================================
    public static void ActionPromotion()
    {
        Instance.StartCoroutine(PromoteAction());
    }
    private static IEnumerator PromoteAction()
    {
        if (ThreatAction is null || ThreatenedLink is null) yield break;

        GameObject ThreatsText = gameObjects["ThreatsDescriptionTextCanvas"];
        ThreatsText.GetComponent<UnityEngine.UI.Text>().text += " P\n";

        if (ThreatenedLink.Consumerj == new Action("Finish", new(), new(), new string[] { }))
        {
            yield return Instance.StartCoroutine(UpdateThreatsText($"###ERROR! Cannot promote action after the Finish() action.\n"));
            yield break;
        }

        bool successful = popController.Planner.UserPromoteActionOnLink(ThreatAction, ThreatenedLink);

        var tempAction = ThreatAction;
        var tempLink = ThreatenedLink;
        if (successful)
        {
            ThreatAction = null;
            ThreatenedLink = null;
            // link the ordering constraint visually in the graph
            ForceDirectedGraph.DataStructure.Link link = new ForceDirectedGraph.DataStructure.Link(POPEngineDriverController.GetNodeByAction(tempLink.Consumerj), POPEngineDriverController.GetNodeByAction(tempAction),
                         0.001f, isOrderingConstraint: true);
            POPEngineDriverController.Network.Links.Add(link);
            POPEngineDriverController.Graph.AddDisplayLink(link);
            // update the nodes text and links text
            POPEngineDriverController.UpdateNodesText(popController.Planner.PartialPlan);
            POPEngineDriverController.UpdateLinksText(popController.Planner.PartialPlan);

            yield return Instance.StartCoroutine(UpdateThreatsText($"Action {popController.ActionToString(tempAction)} has been promoted successfully.\n"));
        }
        else
        {
            yield return Instance.StartCoroutine(UpdateThreatsText($"###ERROR! Action {popController.ActionToString(tempAction)} could not be promoted. Cycle Detected! Rolling Back!\n"));
        }
        yield return Instance.StartCoroutine(UpdateThreatsText("Clearing and restarting...\n"));
        Instance.StartCoroutine(CheckAndUpdateThreats(tempAction, tempLink));
    }

    public static void ActionDemotion()
    {
        Instance.StartCoroutine(DemoteAction());
    }
    private static IEnumerator DemoteAction()
    {
        if (ThreatAction is null || ThreatenedLink is null) yield break;

        GameObject ThreatsText = gameObjects["ThreatsDescriptionTextCanvas"];
        ThreatsText.GetComponent<UnityEngine.UI.Text>().text += " D\n";

        if (ThreatenedLink.Consumerj == new Action("Start", new(), new(), new string[] { }))
        {
            yield return Instance.StartCoroutine(UpdateThreatsText($"###ERROR! Cannot demote action before the Start() action.\n"));
            yield break;
        }

        bool successful = popController.Planner.UserDemoteActionOnLink(ThreatAction, ThreatenedLink);

        var tempAction = ThreatAction;
        var tempLink = ThreatenedLink;
        if (successful)
        {
            print(popController.Planner.PartialPlan);
            ThreatAction = null;
            ThreatenedLink = null;
            // link the ordering constraint visually in the graph
            ForceDirectedGraph.DataStructure.Link link = new ForceDirectedGraph.DataStructure.Link(POPEngineDriverController.GetNodeByAction(tempAction), POPEngineDriverController.GetNodeByAction(tempLink.Produceri),
                         0.001f, isOrderingConstraint: true);
            POPEngineDriverController.Network.Links.Add(link);
            POPEngineDriverController.Graph.AddDisplayLink(link);
            // update the nodes text and links text
            POPEngineDriverController.UpdateNodesText(popController.Planner.PartialPlan);
            POPEngineDriverController.UpdateLinksText(popController.Planner.PartialPlan);


            yield return Instance.StartCoroutine(UpdateThreatsText($"Action {popController.ActionToString(tempAction)} has been demoted successfully.\n"));
        }
        else
        {
            yield return Instance.StartCoroutine(UpdateThreatsText($"###ERROR! Action {popController.ActionToString(tempAction)} could not be demoted. Cycle Detected! Rolling Back!\n"));
        }
        print("restarting");
        yield return Instance.StartCoroutine(UpdateThreatsText("Clearing and restarting...\n"));
        Instance.StartCoroutine(CheckAndUpdateThreats(tempAction, tempLink));
    }
    //================================================================================================================================================================
    public static void DeleteCausalLink()
    {
        Instance.StartCoroutine(DeleteCausalLinkCoroutine());
    }

    private static IEnumerator DeleteCausalLinkCoroutine()
    {
        if (currentCausalLinkIndex < 1 || currentCausalLinkIndex > CausalLinks.Count) yield break;

        GameObject CausalLinksText = gameObjects["CausalLinksDescriptionTextCanvas"];

        CausalLink link = CausalLinks[currentCausalLinkIndex - 1];

        // remove the causal link from the partial plan
        popController.Planner.PartialPlan.CausalLinks.Remove(link);
        // re-add the link condition to the agenda
        popController.Planner.Agenda.Add(link.Consumerj, link.LinkCondition);

        CausalLinksText.GetComponent<UnityEngine.UI.Text>().text = "Causal Link Deleted Successfully!\n";
        // destroy the causal link button
        Destroy(gameObjects["CausalLinksButtons"].transform.Find($"B{currentCausalLinkIndex}").gameObject);
        // set delete button disabled
        gameObjects["CausalLinksDeleteButton"].GetComponent<UnityEngine.UI.Button>().interactable = false;

        // remove the causal link visually from the graph
        ForceDirectedGraph.DataStructure.Link graphLink = POPEngineDriverController.GetLinkByActions(link.Produceri, link.Consumerj, popController.Planner.PartialPlan);
        // Check first if there is no other graphLink with the same actions
        bool isThereAnotherLink = popController.Planner.PartialPlan.OrderingConstraints.Contains(new(link.Produceri, link.Consumerj));
        if (POPEngineDriverController.Graph.RemoveLink(graphLink, isThereAnotherLink, popController.Planner.PartialPlan.LiteralToString(link.LinkCondition)))
            if (isThereAnotherLink) graphLink.IsOrderingConstraint = true;
            else POPEngineDriverController.Network.Links.Remove(graphLink);
        // update the nodes text and links text
        POPEngineDriverController.UpdateNodesText(popController.Planner.PartialPlan);
        POPEngineDriverController.UpdateLinksText(popController.Planner.PartialPlan);


        yield return new WaitForSeconds(3.0f);
        gameObjects["CausalLinksDeleteButton"].GetComponent<UnityEngine.UI.Button>().interactable = true;
        CausalLinksText.GetComponent<UnityEngine.UI.Text>().text = "Choose any of the Causal Links & press delete.\n";

        InitCausalLinksMenu();
        // reset agenda
        InitAgenda();
        // check for threats
        if (ThreatAction is not null && ThreatenedLink is not null)
            Instance.StartCoroutine(CheckAndUpdateThreats(ThreatAction, ThreatenedLink));
    }

    public static void DeleteOrderingConstraint()
    {
        Instance.StartCoroutine(DeleteOrderingConstraintCoroutine());
    }

    private static IEnumerator DeleteOrderingConstraintCoroutine()
    {
        if (currentOrderingConstraintIndex < 1 || currentOrderingConstraintIndex > OrderingConstraints.Count) yield break;

        GameObject OrderingConstraintsText = gameObjects["OrderingConstraintsDescriptionTextCanvas"];

        Tuple<Action, Action> pair = OrderingConstraints[currentOrderingConstraintIndex - 1];

        popController.Planner.PartialPlan.OrderingConstraints.Remove(pair);

        OrderingConstraintsText.GetComponent<UnityEngine.UI.Text>().text = "Ordering Constraint Deleted Successfully!\n";
        // destroy the ordering constraint button
        Destroy(gameObjects["OrderingConstraintsButtons"].transform.Find($"B{currentOrderingConstraintIndex}").gameObject);
        // set delete button disabled
        gameObjects["OrderingConstraintsDeleteButton"].GetComponent<UnityEngine.UI.Button>().interactable = false;

        // remove the ordering constraint visually from the graph
        ForceDirectedGraph.DataStructure.Link link = POPEngineDriverController.GetLinkByActions(pair.Item1, pair.Item2, popController.Planner.PartialPlan);
        POPEngineDriverController.Network.Links.Remove(link);
        POPEngineDriverController.Graph.RemoveLink(link, false);
        // update the nodes text and links text
        POPEngineDriverController.UpdateNodesText(popController.Planner.PartialPlan);
        POPEngineDriverController.UpdateLinksText(popController.Planner.PartialPlan);


        yield return new WaitForSeconds(3.0f);
        gameObjects["OrderingConstraintsDeleteButton"].GetComponent<UnityEngine.UI.Button>().interactable = true;
        OrderingConstraintsText.GetComponent<UnityEngine.UI.Text>().text = "Note: Any Ordering Constraint with Start or Finish will not be shown, And any one implied by a causal link will not be shown, as it cannot be deleted.\n";

        InitOrderingConstraintsMenu();
        // reset agenda
        InitAgenda();
        // check for threats
        if (ThreatAction is not null && ThreatenedLink is not null)
            Instance.StartCoroutine(CheckAndUpdateThreats(ThreatAction, ThreatenedLink));
    }
    #endregion

    #region Navigation
    public static void AgendaMoveDown()
    {
        if (currentAgendaIndex < 1) return;

        int oldidx = currentAgendaIndex;
        currentAgendaIndex = Math.Max(currentAgendaIndex - 1, 1);

        // set the next operator button to "#FFFFFF" color
        GameObject nextAgendaButton = gameObjects["AgendaButtons"].transform.Find($"B{oldidx}").gameObject;
        nextAgendaButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current operator button to "#FF0000" color
        GameObject currentAgendaButton = gameObjects["AgendaButtons"].transform.Find($"B{currentAgendaIndex}").gameObject;
        currentAgendaButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);

        // set the description of the current operator in the description canvas
        // gameObjects["AgendaDescriptionTextCanvas"].GetComponent<UnityEngine.UI.Text>().text = popController.ActionToString(tempAgendaList[currentAgendaIndex - 1].Item1) + " / " + popController.LiteralToString(tempAgendaList[currentAgendaIndex - 1].Item2);
    }

    public static void AgendaMoveUp()
    {
        if (currentAgendaIndex > tempAgendaList.Count) return;

        int oldidx = currentAgendaIndex;
        currentAgendaIndex = Math.Min(currentAgendaIndex + 1, tempAgendaList.Count);

        // set the previous operator button to "#FFFFFF" color
        GameObject previousAgendaButton = gameObjects["AgendaButtons"].transform.Find($"B{oldidx}").gameObject;
        previousAgendaButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current operator button to "#FF0000" color
        GameObject currentAgendaButton = gameObjects["AgendaButtons"].transform.Find($"B{currentAgendaIndex}").gameObject;
        currentAgendaButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);

        // set the description of the current operator in the description canvas
        // gameObjects["AgendaDescriptionTextCanvas"].GetComponent<UnityEngine.UI.Text>().text = popController.ActionToString(tempAgendaList[currentAgendaIndex - 1].Item1) + " / " + popController.LiteralToString(tempAgendaList[currentAgendaIndex - 1].Item2);
    }

    //================================================================================================================================================================
    public static void AchieversMoveLeft()
    {
        if (currentAchieverJdx <= 1) return;

        int oldidx = currentAchieverIdx, oldjdx = currentAchieverJdx;

        currentAchieverJdx = 1;
        currentAchieverIdx = Math.Min(currentAchieverIdx, achievers[currentAchieverJdx - 1].Count);

        // set the previous variable button to "#FFFFFF" color
        GameObject previousAchieverButton = gameObjects["AchieversOptionButtons"].transform.Find($"B{oldidx}{oldjdx}").gameObject;
        previousAchieverButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current variable button to "#FF0000" color
        GameObject currentAchieverButton = gameObjects["AchieversOptionButtons"].transform.Find($"B{currentAchieverIdx}{currentAchieverJdx}").gameObject;
        currentAchieverButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);
    }

    public static void AchieversMoveRight()
    {
        if (currentAchieverJdx > 2) return;

        int oldidx = currentAchieverIdx, oldjdx = currentAchieverJdx;

        currentAchieverJdx = 2;
        currentAchieverIdx = Math.Min(currentAchieverIdx, achievers[currentAchieverJdx - 1].Count);
        if (currentAchieverIdx == 0)
        {
            currentAchieverIdx = oldidx;
            currentAchieverJdx = 1;
            return;
        }

        // set the previous variable button to "#FFFFFF" color
        GameObject previousAchieverButton = gameObjects["AchieversOptionButtons"].transform.Find($"B{oldidx}{oldjdx}").gameObject;
        previousAchieverButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current variable button to "#FF0000" color
        GameObject currentAchieverButton = gameObjects["AchieversOptionButtons"].transform.Find($"B{currentAchieverIdx}{currentAchieverJdx}").gameObject;
        currentAchieverButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);
    }
    public static void AchieversMoveDown()
    {
        if (currentAchieverIdx < 1) return;

        int oldidx = currentAchieverIdx;
        currentAchieverIdx = Math.Max(currentAchieverIdx - 1, 1);

        // set the previous variable button to "#FFFFFF" color
        GameObject previousAchieverButton = gameObjects["AchieversOptionButtons"].transform.Find($"B{oldidx}{currentAchieverJdx}").gameObject;
        previousAchieverButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current variable button to "#FF0000" color
        GameObject currentAchieverButton = gameObjects["AchieversOptionButtons"].transform.Find($"B{currentAchieverIdx}{currentAchieverJdx}").gameObject;
        currentAchieverButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);
    }
    public static void AchieversMoveUp()
    {
        if (currentAchieverIdx > achievers[currentAchieverJdx - 1].Count) return;

        int oldidx = currentAchieverIdx;
        currentAchieverIdx = Math.Min(currentAchieverIdx + 1, achievers[currentAchieverJdx - 1].Count);

        // set the previous variable button to "#FFFFFF" color
        GameObject previousAchieverButton = gameObjects["AchieversOptionButtons"].transform.Find($"B{oldidx}{currentAchieverJdx}").gameObject;
        previousAchieverButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current variable button to "#FF0000" color
        GameObject currentAchieverButton = gameObjects["AchieversOptionButtons"].transform.Find($"B{currentAchieverIdx}{currentAchieverJdx}").gameObject;
        currentAchieverButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);
    }
    //================================================================================================================================================================
    public static void OpMoveUp()
    {
        if (currentOperatorIndex > operators.Count) return;

        currentOperatorIndex = Math.Min(currentOperatorIndex + 1, operators.Count);

        // set the previous operator button to "#FFFFFF" color
        GameObject previousOperatorButton = OperatorButtons.transform.Find($"B{currentOperatorIndex - 1}").gameObject;
        previousOperatorButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current operator button to "#FF0000" color
        GameObject currentOperatorButton = OperatorButtons.transform.Find($"B{currentOperatorIndex}").gameObject;
        currentOperatorButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);

        // set the description of the current operator in the description canvas
        OperatorDescriptionTextCanvas.GetComponent<UnityEngine.UI.Text>().text = operators[currentOperatorIndex - 1].GetFullStringDetails();
    }

    public static void OpMoveDown()
    {
        if (currentOperatorIndex < 1) return;

        currentOperatorIndex = Math.Max(currentOperatorIndex - 1, 1);

        // set the next operator button to "#FFFFFF" color
        GameObject nextOperatorButton = OperatorButtons.transform.Find($"B{currentOperatorIndex + 1}").gameObject;
        nextOperatorButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current operator button to "#FF0000" color
        GameObject currentOperatorButton = OperatorButtons.transform.Find($"B{currentOperatorIndex}").gameObject;
        currentOperatorButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);

        // set the description of the current operator in the description canvas
        OperatorDescriptionTextCanvas.GetComponent<UnityEngine.UI.Text>().text = operators[currentOperatorIndex - 1].GetFullStringDetails();
    }
    //================================================================================================================================================================
    public static void VarMoveUp()
    {
        if (currentVariableIdxInVariables > variables[currentVariableJdxInVariables - 1].Count + 1 - (1 * ((currentVariableJdxInVariables + 1) % 2))) return;

        int oldidx = currentVariableIdxInVariables;
        currentVariableIdxInVariables = Math.Min(currentVariableIdxInVariables + 1, variables[currentVariableJdxInVariables - 1].Count + 1 - (1 * ((currentVariableJdxInVariables + 1) % 2)));

        // set the previous variable button to "#FFFFFF" color
        GameObject previousVariableButton = VariableOptionButtons.transform.Find($"B{oldidx}{currentVariableJdxInVariables}").gameObject;
        previousVariableButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current variable button to "#FF0000" color
        GameObject currentVariableButton = VariableOptionButtons.transform.Find($"B{currentVariableIdxInVariables}{currentVariableJdxInVariables}").gameObject;
        currentVariableButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);
    }

    public static void VarMoveDown()
    {
        if (currentVariableIdxInVariables < 1) return;

        int oldidx = currentVariableIdxInVariables;
        currentVariableIdxInVariables = Math.Max(currentVariableIdxInVariables - 1, 1);

        // set the previous variable button to "#FFFFFF" color
        GameObject previousVariableButton = VariableOptionButtons.transform.Find($"B{oldidx}{currentVariableJdxInVariables}").gameObject;
        previousVariableButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current variable button to "#FF0000" color
        GameObject currentVariableButton = VariableOptionButtons.transform.Find($"B{currentVariableIdxInVariables}{currentVariableJdxInVariables}").gameObject;
        currentVariableButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);
    }

    public static void VarMoveRight()
    {
        if (currentVariableJdxInVariables > 2) return;

        int oldidx = currentVariableIdxInVariables;

        currentVariableJdxInVariables = 2;
        currentVariableIdxInVariables = Math.Min(currentVariableIdxInVariables, variables[currentVariableJdxInVariables - 1].Count);
        if (currentVariableIdxInVariables == 0)
        {
            currentVariableIdxInVariables = oldidx;
            currentVariableJdxInVariables = 1;
            return;
        }

        // set the previous variable button to "#FFFFFF" color
        GameObject previousVariableButton = VariableOptionButtons.transform.Find($"B{oldidx}{currentVariableJdxInVariables - 1}").gameObject;
        previousVariableButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current variable button to "#FF0000" color
        GameObject currentVariableButton = VariableOptionButtons.transform.Find($"B{currentVariableIdxInVariables}{currentVariableJdxInVariables}").gameObject;
        currentVariableButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);
    }

    public static void VarMoveLeft()
    {
        if (currentVariableJdxInVariables <= 1) return;

        int oldidx = currentVariableIdxInVariables;

        currentVariableJdxInVariables = 1;
        currentVariableIdxInVariables = Math.Min(currentVariableIdxInVariables, variables[currentVariableJdxInVariables - 1].Count + 1);

        // set the previous variable button to "#FFFFFF" color
        GameObject previousVariableButton = VariableOptionButtons.transform.Find($"B{oldidx}{currentVariableJdxInVariables + 1}").gameObject;
        previousVariableButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current variable button to "#FF0000" color
        GameObject currentVariableButton = VariableOptionButtons.transform.Find($"B{currentVariableIdxInVariables}{currentVariableJdxInVariables}").gameObject;
        currentVariableButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);
    }
    //================================================================================================================================================================
    public static void CausalLinksMoveDown()
    {
        if (currentCausalLinkIndex < 1 || CausalLinks.Count < 1) return;

        int oldidx = currentCausalLinkIndex;
        currentCausalLinkIndex = Math.Max(currentCausalLinkIndex - 1, 1);

        // set the next operator button to "#FFFFFF" color
        GameObject nextCausalLinkButton = gameObjects["CausalLinksButtons"].transform.Find($"B{oldidx}").gameObject;
        nextCausalLinkButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current operator button to "#FF0000" color
        GameObject currentCausalLinkButton = gameObjects["CausalLinksButtons"].transform.Find($"B{currentCausalLinkIndex}").gameObject;
        currentCausalLinkButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);

        // set the description of the current operator in the description canvas
        // gameObjects["CausalLinksDescriptionTextCanvas"].GetComponent<UnityEngine.UI.Text>().text = popController.Planner.PartialPlan.CausalLinkToString(CausalLinks[currentCausalLinkIndex - 1]);
    }

    public static void CausalLinksMoveUp()
    {
        if (currentCausalLinkIndex > CausalLinks.Count) return;

        int oldidx = currentCausalLinkIndex;
        currentCausalLinkIndex = Math.Min(currentCausalLinkIndex + 1, CausalLinks.Count);

        // set the previous operator button to "#FFFFFF" color
        GameObject previousCausalLinkButton = gameObjects["CausalLinksButtons"].transform.Find($"B{oldidx}").gameObject;
        previousCausalLinkButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current operator button to "#FF0000" color
        GameObject currentCausalLinkButton = gameObjects["CausalLinksButtons"].transform.Find($"B{currentCausalLinkIndex}").gameObject;
        currentCausalLinkButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);

        // set the description of the current operator in the description canvas
        // gameObjects["CausalLinksDescriptionTextCanvas"].GetComponent<UnityEngine.UI.Text>().text = popController.Planner.PartialPlan.CausalLinkToString(CausalLinks[currentCausalLinkIndex - 1]);
    }

    public static void OrderingConstraintsMoveDown()
    {
        if (currentOrderingConstraintIndex < 1 || OrderingConstraints.Count < 1) return;

        int oldidx = currentOrderingConstraintIndex;
        currentOrderingConstraintIndex = Math.Max(currentOrderingConstraintIndex - 1, 1);

        // set the next operator button to "#FFFFFF" color
        GameObject nextOrderingConstraintButton = gameObjects["OrderingConstraintsButtons"].transform.Find($"B{oldidx}").gameObject;
        nextOrderingConstraintButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current operator button to "#FF0000" color
        GameObject currentOrderingConstraintButton = gameObjects["OrderingConstraintsButtons"].transform.Find($"B{currentOrderingConstraintIndex}").gameObject;
        currentOrderingConstraintButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);

        // set the description of the current operator in the description canvas
        // gameObjects["OrderingConstraintsDescriptionTextCanvas"].GetComponent<UnityEngine.UI.Text>().text = OrderingConstraintToString(OrderingConstraints[currentOrderingConstraintIndex - 1]);
    }

    public static void OrderingConstraintsMoveUp()
    {
        if (currentOrderingConstraintIndex > OrderingConstraints.Count) return;

        int oldidx = currentOrderingConstraintIndex;
        currentOrderingConstraintIndex = Math.Min(currentOrderingConstraintIndex + 1, OrderingConstraints.Count);

        // set the previous operator button to "#FFFFFF" color
        GameObject previousOrderingConstraintButton = gameObjects["OrderingConstraintsButtons"].transform.Find($"B{oldidx}").gameObject;
        previousOrderingConstraintButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);

        // set the color of the current operator button to "#FF0000" color
        GameObject currentOrderingConstraintButton = gameObjects["OrderingConstraintsButtons"].transform.Find($"B{currentOrderingConstraintIndex}").gameObject;
        currentOrderingConstraintButton.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);

        // set the description of the current operator in the description canvas
        // gameObjects["OrderingConstraintsDescriptionTextCanvas"].GetComponent<UnityEngine.UI.Text>().text = OrderingConstraintToString(OrderingConstraints[currentOrderingConstraintIndex - 1]);
    }

    #endregion

    #region Setting Variables

    public static void SetAchievers()
    {
        achievers = new();
        List<Operator> existingActions = new();
        existingActions.AddRange(popController.Planner.PartialPlan.getListOfActionsAchievers(tempAgendaList[currentAgendaIndex - 1].Item2, tempAgendaList[currentAgendaIndex - 1].Item1));

        List<Operator> newActions = popController.Planner.Problem.GetListOfAchievers(tempAgendaList[currentAgendaIndex - 1].Item2);

        achievers.Add(newActions);
        achievers.Add(existingActions);
    }

    private static void ResetVariables()
    {
        variables = new List<List<string>>();

        List<string> myConstants = POPEngineDriverController.ProblemConstants.ToList();
        List<string> myVariables = new List<string>();

        HashSet<string> myUniqueVariables = new HashSet<string>();

        foreach (string variable in partialPlan.Actions.SelectMany(a => a.Variables))
        {
            string b = partialPlan.GetBindingConstraintsBounds(variable);
            if (!Helpers.IsUpper(b[0]))
                myUniqueVariables.Add(b);
        }

        foreach (string variable in partialPlan.Actions.SelectMany(a => a.Effects.SelectMany(e => e.Variables))
                .Concat(partialPlan.Actions.SelectMany(a => a.Preconditions.SelectMany(p => p.Variables))))
        {
            string b = partialPlan.GetBindingConstraintsBounds(variable);
            if (!Helpers.IsUpper(b[0]))
                myUniqueVariables.Add(b);
        }

        if (currentAction != null)
        {
            foreach (string variable in currentAction.Variables)
            {
                string b = currentAction.BoundVariables?.GetValueOrDefault(variable, null);
                if (b is null)
                {
                    if (currentAction.Variables[currentParameterIndex - 1] != variable)
                        myUniqueVariables.Add(variable);
                }
                else if (!Helpers.IsUpper(b[0]))
                    myUniqueVariables.Add(b);
            }
        }

        foreach (string variable in myUniqueVariables)
        {
            myVariables.Add(partialPlan.GetBindingConstraintsBounds(variable));
        }

        variables.Add(myConstants);
        variables.Add(myVariables);
    }

    private static void SetOrderingConstraints()
    {
        OrderingConstraints = new List<Tuple<Action, Action>>();
        foreach (var tuple in popController.Planner.PartialPlan.OrderingConstraints)
        {
            if (tuple.Item1 == null || tuple.Item2 == null) continue;
            if (tuple.Item1 == new Action("Start", new(), new(), new string[] { })
                    || tuple.Item2 == new Action("Finish", new(), new(), new string[] { }))
                continue;

            bool foundLink = false;
            foreach (CausalLink link in popController.Planner.PartialPlan.CausalLinks)
            {
                // Ordering Constraints implied by the Causal Links will not be shown and cannot be removed
                if (link.Produceri == tuple.Item1 && link.Consumerj == tuple.Item2) { foundLink = true; break; }
            }
            if (foundLink) continue;

            OrderingConstraints.Add(new Tuple<Action, Action>(tuple.Item1, tuple.Item2));
        }
    }

    #endregion

    #region Helpers

    public static IEnumerator ApplyAchiever(bool isNew, int AgendaPairIndex, GameObject operatorBlock = null)
    {
        // link the action to the start node
        print("Linking the action");
        Action newAction = null;
        CausalLink newLink = null;
        bool applied = popController.Planner.UserApplyAchiever(achievers[currentAchieverJdx - 1][currentAchieverIdx - 1], tempAgendaList[AgendaPairIndex - 1], ref newAction, ref newLink);
        // set ArrowsController.stage to Agenda
        ArrowsController.stage = ArrowsController.Stage.Agenda;

        if (applied)
        {
            print("Achiever Applied Successfully");
            print(popController.Planner.Agenda.Remove(tempAgendaList[AgendaPairIndex - 1]));

            // update the alert text
            gameObjects["AchieversCancelButton"].SetActive(false);
            gameObjects["AchieversAlertTextCanvas"].SetActive(true);
            if (isNew)
            {
                gameObjects["AchieversAlertTextCanvas"].GetComponent<UnityEngine.UI.Text>().text = "New Achiever Added and Applied Successfully";
            }
            else
            {
                gameObjects["AchieversAlertTextCanvas"].GetComponent<UnityEngine.UI.Text>().text = "Achiever Applied and Linked Successfully!\n Please Go check the Graph";
            }
            // check for threats
            Instance.StartCoroutine(CheckAndUpdateThreats(newAction, newLink));
            // init the agenda but don't show yet
            InitAgenda(false);
            // link the action visually
            if (isNew)
            {
                // Add the action to the network
                ForceDirectedGraph.DataStructure.Node node = new ForceDirectedGraph.DataStructure.Node(newAction, popController.Planner.PartialPlan);
                POPEngineDriverController.Network.Nodes.Add(node);
                POPEngineDriverController.Graph.AddDisplayNode(node, Color.green, existingNode: operatorBlock);
            }
            // Add the link to the network
            ForceDirectedGraph.DataStructure.Link linkCheck = POPEngineDriverController.GetLinkByActions(newLink.Produceri, newLink.Consumerj, popController.Planner.PartialPlan);
            if (linkCheck is not null && !linkCheck.IsOrderingConstraint)
            {
                linkCheck.Condition += "," + popController.Planner.PartialPlan.LiteralToString(newLink.LinkCondition);
            }
            else
            {
                if (linkCheck is not null) POPEngineDriverController.Graph.RemoveLink(linkCheck, false);
                ForceDirectedGraph.DataStructure.Link link = new ForceDirectedGraph.DataStructure.Link(POPEngineDriverController.GetNodeByAction(newLink.Produceri), POPEngineDriverController.GetNodeByAction(newLink.Consumerj),
                             0.01f, CausalLinkCondition: popController.Planner.PartialPlan.LiteralToString(newLink.LinkCondition));
                POPEngineDriverController.Network.Links.Add(link);
                POPEngineDriverController.Graph.AddDisplayLink(link);
            }
            // update the nodes text and links text
            POPEngineDriverController.UpdateNodesText(popController.Planner.PartialPlan);
            POPEngineDriverController.UpdateLinksText(popController.Planner.PartialPlan);

            yield return new WaitForSeconds(3.0f);
            gameObjects["AchieversAlertTextCanvas"].SetActive(false);
            gameObjects["AchieversCancelButton"].SetActive(true);

            // print current plan
            print(popController.Planner.PartialPlan);

            // now show the agenda menu
            gameObjects["AgendaCanvas"].SetActive(true);
            gameObjects["AgendaCanvas"].transform.Find("BodyTitle").gameObject.SetActive(true);
            gameObjects["AchieversCanvas"].SetActive(false);
        }
        else
        {
            print("Achiever Not Applied Successfully");
            gameObjects["AchieversAlertTextCanvas"].SetActive(true);
            gameObjects["AchieversAlertTextCanvas"].GetComponent<UnityEngine.UI.Text>().text = "Conflict: Achiever Couldn't Bind Variables!";
            yield return new WaitForSeconds(3.0f);

            // show the next, navigation buttons, cancel, done and the achievers options AND hide the request button & alert text
            gameObjects["AchieversNavigation"].SetActive(true);
            gameObjects["AchieversDoneButton"].SetActive(true);
            gameObjects["AchieversOptionButtons"].SetActive(true);
            gameObjects["AchieversRequestButton"].SetActive(false);
            gameObjects["AchieversAlertTextCanvas"].SetActive(false);
            gameObjects["AchieversCancelButton"].SetActive(true);
        }
    }

    public static IEnumerator CheckAndUpdateThreats(Action newAction, CausalLink newCausalLink)
    {
        // initialize the Causal Links & Ordering Constraints
        InitCausalLinksMenu();
        InitOrderingConstraintsMenu();

        GameObject ThreatsText = gameObjects["ThreatsDescriptionTextCanvas"];
        /*
            ### Initiating Threat detection protocol...
            ### Do you want to abort? (Y/N) N
            $: sudo threat -a action -l link
            Executing Threat Search Algorithm...
            ### ERROR! THREAT DETECTED:
            > Action(p1,p2,p3) is threatening the link:
            Action2(p1,p2) --Precond()--> Action3(a1,a2)
            ### PRESS 'P' TO INITIATE PROMOTION PROTOCOL OR 'D' TO INITIATE DEMOTION PROTOCOL...
        */
        ThreatsText.GetComponent<UnityEngine.UI.Text>().text = "### Initiating Threat detection protocol...\n### Do you want to abort? (Y/N) N\n$: sudo threat -a action -l link\nExecuting Threat Search Algorithm...";

        yield return new WaitForSeconds(1.0f);

        // check for threats
        var threat = popController.Planner.UserCheckForThreats(newAction, newCausalLink);

        // update the alert text
        if (threat is null)
        {
            ThreatAction = null;
            ThreatenedLink = null;
            ThreatsText.GetComponent<UnityEngine.UI.Text>().text += "\n### No Threats Detected!\n$: ";
            NotebookController.TURNED_ON = true;
            ThereIsThreat = false;
            // set ArrowController.stage to Agenda
            ArrowsController.stage = ArrowsController.Stage.Agenda;
            yield break;
        }

        ThreatAction = threat.Value.Threat;
        ThreatenedLink = threat.Value.ThreatenedLink;

        ThreatsText.GetComponent<UnityEngine.UI.Text>().text += $"\n### ERROR! THREAT DETECTED:\n> Action {popController.Planner.PartialPlan.ActionToString(ThreatAction)} is threatening the link:\n{popController.Planner.PartialPlan.CausalLinkToString(ThreatenedLink)} \n### PRESS 'P' TO INITIATE PROMOTION PROTOCOL OR 'D' TO INITIATE DEMOTION PROTOCOL... ";
        Instance.StartCoroutine(EmergencySoundAndLightOn());

        NotebookController.TURNED_ON = false;
        // set ArrowController.stage to Threats
        ArrowsController.stage = ArrowsController.Stage.Threats;
    }

    private static IEnumerator UpdateThreatsText(string text)
    {
        GameObject ThreatsText = gameObjects["ThreatsDescriptionTextCanvas"];

        string[] lines = ThreatsText.GetComponent<UnityEngine.UI.Text>().text.Split('\n');
        StringBuilder newText = new();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue;
            newText.Append(lines[i] + "\n");
        }
        newText.Append(text + "\n");
        ThreatsText.GetComponent<UnityEngine.UI.Text>().text = newText.ToString();

        yield return new WaitForSeconds(2.0f);
    }

    private static string OrderingConstraintToString(Tuple<Action, Action> tuple)
    {
        return $"{popController.ActionToString(tuple.Item1)} ≺ {popController.ActionToString(tuple.Item2)}";
    }

    private static IEnumerator EmergencySoundAndLightOn()
    {
        if (ThereIsThreat) yield break;
        ThereIsThreat = true;

        // turn on the emergency sound 
        gameObjects["EmergencyAlarm"].GetComponent<AudioSource>().Play();

        // turn on the emergency light
        while (ThereIsThreat)
        {
            gameObjects["EmergencyAlarm"].transform.Find("Light").GetComponent<Light>().enabled = true;
            yield return new WaitForSeconds(0.55f);
            gameObjects["EmergencyAlarm"].transform.Find("Light").GetComponent<Light>().enabled = false;
            yield return new WaitForSeconds(0.5f);

            gameObjects["EmergencyAlarm"].transform.Find("Light").GetComponent<Light>().enabled = true;
            yield return new WaitForSeconds(0.55f);
            gameObjects["EmergencyAlarm"].transform.Find("Light").GetComponent<Light>().enabled = false;
            yield return new WaitForSeconds(0.5f);

            gameObjects["EmergencyAlarm"].transform.Find("Light").GetComponent<Light>().enabled = true;
            yield return new WaitForSeconds(0.55f);
            gameObjects["EmergencyAlarm"].transform.Find("Light").GetComponent<Light>().enabled = false;
            yield return new WaitForSeconds(1.19f);
        }

        // turn off the emergency sound
        gameObjects["EmergencyAlarm"].GetComponent<AudioSource>().Stop();
    }

    #endregion


}
