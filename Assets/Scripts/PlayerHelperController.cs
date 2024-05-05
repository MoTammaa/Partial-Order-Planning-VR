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
            planner = new Planner(problem, POPEngineDriverController.GetSearchStrategyFromPlayerPrefs(), POPEngineDriverController.GetMaxDepthFromPlayerPrefs());
            // partialPlan = planner.PartialPlan;
            partialPlan = planner.POP();
            // print("partialPlan: " + partialPlan);
            InitOperatorsMenu();
            ResetVariables();
            // DoneChoosingOperator();
        }
    }

    ////// new //////////////////////////////////////////

    private static POPController popController;
    public static POPController PopController { get { return popController; } set { popController = value; InitAgenda(); } }

    private static Dictionary<string, GameObject> gameObjects = new();
    private static int currentAgendaIndex = 1;
    private static List<Tuple<POP.Action, POP.Literal>> tempAgendaList = new();

    private static List<List<Operator>> achievers = new();

    private static int currentAchieverIdx = 1;
    private static int currentAchieverJdx = 1;


    //////////////////////////////////////////////////////
    private static List<Operator> operators = new(); // will be set by the POPEngineDriverController
    public static List<Operator> Operators { get { return operators; } }
    private static List<List<string>> variables = new(); // will be set by the POPEngineDriverController

    private static POP.Action currentAction;

    private static PartialPlan partialPlan; // will be set by the POPEngineDriverController
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

        gameObjects["AchieversCanvas"].SetActive(false);
        gameObjects["AchieversRequestButton"].SetActive(false);
        gameObjects["AchieversAlertTextCanvas"].SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {

    }

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


    private static void InitAgenda()
    {
        tempAgendaList = new();

        // Set Visibility
        gameObjects["AgendaCanvas"].SetActive(true);
        gameObjects["AgendaCanvas"].transform.Find("BodyTitle").gameObject.SetActive(true);
        gameObjects["AchieversCanvas"].SetActive(false);

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
        foreach (Tuple<POP.Action, POP.Literal> pair in popController.Planner.Agenda)
        {
            tempAgendaList.Add(pair);
        }
        for (int i = 0; i < tempAgendaList.Count; i++)
        {
            Tuple<POP.Action, POP.Literal> pair = tempAgendaList[i];

            GameObject button = Instantiate(B0, buttons.transform);
            button.name = "B" + (i + 1);
            button.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = $"A:{popController.ActionToString(pair.Item1)} / PreC:{popController.LiteralToString(pair.Item2)}";
            button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y + 0.125f * i, button.transform.localPosition.z);
            button.transform.localScale = new Vector3(1.8f, 1, 1);
            button.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);
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
        AgendaMoveDown();
    }

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

    public static void DoneChoosingAgenda()
    {
        // hide the agenda menu body
        gameObjects["AgendaCanvas"].transform.Find("BodyTitle").gameObject.SetActive(false);

        // show the achievers menu
        gameObjects["AchieversCanvas"].SetActive(true);

        // initialize the achievers menu
        InitAchieversMenu();
    }


    public static void InitAchieversMenu()
    {
        // show the next, navigation buttons, cancel, done and the achievers options AND hide the request button & alert text
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
                POP.Action actionAchiever = (achievers[i][j] is POP.Action action) ? action : null;
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

    public static void SetAchievers()
    {
        achievers = new();
        List<Operator> existingActions = new();
        existingActions.AddRange(popController.Planner.PartialPlan.getListOfActionsAchievers(tempAgendaList[currentAgendaIndex - 1].Item2, tempAgendaList[currentAgendaIndex - 1].Item1));

        List<Operator> newActions = popController.Planner.Problem.GetListOfAchievers(tempAgendaList[currentAgendaIndex - 1].Item2);

        achievers.Add(newActions);
        achievers.Add(existingActions);
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
            Instance.StartCoroutine(ApplyAchiever(false));
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

        Instance.StartCoroutine(InstantiateNewAchiever());

        // reset the current agenda index
        currentAgendaIndex = 1;

        // initialize the agenda menu
        AgendaMoveDown();
    }

    public static IEnumerator InstantiateNewAchiever()
    {
        // create the operators blocks by using the prefab "NF-Operator-Green" in the Prefabs folder
        GameObject Actions = gameObjects["AgendaCanvas"].transform.parent.Find("Actions").gameObject;
        GameObject operatorBlock = Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Prefabs/NF-Operator-Green.prefab", typeof(GameObject)) as GameObject, Actions.transform);
        operatorBlock.name = "Action" + (++numOfOperators);
        Vector3 offset = new Vector3(gameObjects["AgendaCanvas"].transform.localPosition.x, gameObjects["AgendaCanvas"].transform.localPosition.y + 10.0f, gameObjects["AgendaCanvas"].transform.localPosition.z - 0.5f);
        operatorBlock.transform.localPosition = new Vector3(0.0f, 20.0f, 1.1f) + offset;

        //get position of the last action
        Vector3 lastActionOriginal = operatorBlock.transform.localPosition;
        Vector3 lastActionPosition = operatorBlock.transform.localPosition;

        while (Math.Sqrt(Math.Pow(lastActionPosition.x - lastActionOriginal.x, 2) + Math.Pow(lastActionPosition.z - lastActionOriginal.z, 2)) < 1.0f)
        {
            lastActionPosition = operatorBlock.transform.localPosition;
            yield return new WaitForSeconds(3.0f);
        }

        // show the agenda menu
        gameObjects["AgendaCanvas"].SetActive(true);

        // wait till it is near the graph nodes (start or finish) then link it
        Vector3 startNode = Actions.transform.Find("Start").transform.position;
        Vector3 finishNode = Actions.transform.Find("Finish").transform.position;
        lastActionPosition = operatorBlock.transform.position;

        print("waiting");
        while (Math.Sqrt(Math.Pow(lastActionPosition.x - startNode.x, 2) + Math.Pow(lastActionPosition.z - startNode.z, 2)) > 4.0f &&
               Math.Sqrt(Math.Pow(lastActionPosition.x - finishNode.x, 2) + Math.Pow(lastActionPosition.z - finishNode.z, 2)) > 4.0f)
        {
            lastActionPosition = operatorBlock.transform.position;
            print($"lastActionPosition: {lastActionPosition}, startNode: {startNode}, difference: {Math.Sqrt(Math.Pow(lastActionPosition.x - startNode.x, 2) + Math.Pow(lastActionPosition.z - startNode.z, 2))}");
            yield return new WaitForSeconds(3.0f);
        }

        // apply the achiever
        yield return Instance.StartCoroutine(ApplyAchiever(true));

    }

    public static IEnumerator ApplyAchiever(bool isNew)
    {
        // link the action to the start node
        print("Linking the action");
        POP.Action newAction = null;
        CausalLink newLink = null;
        POP.Node node = popController.Planner.createNode(plan: popController.Planner.PartialPlan, agenda: popController.Planner.Agenda);
        bool applied = popController.Planner.ApplyAchiever(achievers[currentAchieverJdx - 1][currentAchieverIdx - 1], tempAgendaList[currentAgendaIndex - 1], node, ref newAction, ref newLink);

        if (applied)
        {
            print("Achiever Applied Successfully");
            print(popController.Planner.Agenda.Remove(tempAgendaList[currentAgendaIndex - 1]));

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
            yield return new WaitForSeconds(3.0f);
            gameObjects["AchieversAlertTextCanvas"].SetActive(false);
            gameObjects["AchieversCancelButton"].SetActive(true);

            // print current plan
            print(popController.Planner.PartialPlan);

            InitAgenda();
            AgendaMoveDown();
        }
        else
        {
            print("Achiever Not Applied Successfully");
        }
    }



}
