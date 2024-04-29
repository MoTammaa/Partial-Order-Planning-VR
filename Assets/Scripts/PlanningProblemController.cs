using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using POP;
using System.Linq;

public class PlanningProblemController : MonoBehaviour
{
    private string problemName;
    private PlanningProblem problem;
    private int maxDepthForDFS;

    /*
        SS: Socks and Shoes Problem
        MBCD: Milk, Bananas, Cordless Drill Problem
        GB: Groceries Buying Problem
        SP: Spare Tires Problem
    */

    // Start is called before the first frame update
    void Start()
    {
        SetProblem("SS"); // socks and shoes problem
    }

    public void SetProblem(string problemName)
    {
        this.problemName = problemName;

        switch (problemName)
        {
            case "SS":
                this.problem = PlanningProblem.SocksShoesProblem(out int recommendedMaxDepthForDFS);
                maxDepthForDFS = recommendedMaxDepthForDFS;
                break;
            case "MBCD":
                this.problem = PlanningProblem.MilkBananasCordlessDrillProblem(out recommendedMaxDepthForDFS);
                maxDepthForDFS = recommendedMaxDepthForDFS;
                break;
            case "GB":
                this.problem = PlanningProblem.GroceriesBuyProblem(out recommendedMaxDepthForDFS);
                maxDepthForDFS = recommendedMaxDepthForDFS;
                break;
            case "SP":
                this.problem = PlanningProblem.SpareTiresProblem(out recommendedMaxDepthForDFS);
                maxDepthForDFS = recommendedMaxDepthForDFS;
                break;
            default:
                break;
        }
        // save the chosen problem and the recommended max depth to the playerprefs
        PlayerPrefs.SetString("ProblemName", problemName);
        PlayerPrefs.SetInt("RecommendedDepthForDFS", maxDepthForDFS);

        SetInitialAndGoalStatesCanvas();
        SetOperatorsCanvas();

        // set the chosen problem name in the canvas
        GameObject.Find("ChosenCanvas").transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = GetOriginalName(problemName);

        // Set All unchosen buttons Normal color to "#FFFFFF" color and the chosen button to "#6B6B6B" color in the "Buttons" parent object
        GameObject buttons = GameObject.Find("Planning Problem Menu").transform.Find("BodyTitle").Find("BodyCanvas").Find("Buttons").gameObject;
        foreach (Transform button in buttons.transform)
        {
            if (button.name == problemName)
            {
                button.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(0.4196078f, 0.4196078f, 0.4196078f);
            }
            else
            {
                button.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);
            }
        }

        // set depth limit canvas to the recommended max depth for DFS if the search strategy is DFS
        if (PreferencesController.SearchStrategy == "DFS")
        {
            PreferencesController.updateSuggestedDepth();
            // add the suggested depth to the ui
            PreferencesController.DepthLimitCanvasObject.transform.Find("BodyTitle").Find("BodyCanvas").Find("OnscreenNumberpadCanvas").Find("NumberCanvas").Find("Text").GetComponent<UnityEngine.UI.Text>().text = PlayerPrefs.GetInt("RecommendedDepthForDFS").ToString();
            // add the suggested depth to the playerprefs
            PlayerPrefs.SetInt("DepthLimit", PlayerPrefs.GetInt("RecommendedDepthForDFS"));
        }
    }

    private void SetOperatorsCanvas()
    {
        GameObject operatorsCanvas = GameObject.Find("OperatorsMenu");
        if (operatorsCanvas == null)
        {
            Debug.LogError("OperatorsCanvas not found");
            return;
        }
        // clear the canvas text in OperatorsMenu > BodyTitle > BodyCanvas > OperatorsCanvas > Text
        GameObject operatorsText = operatorsCanvas.transform.Find("BodyTitle").Find("BodyCanvas").Find("OperatorsCanvas").Find("Text").gameObject;
        operatorsText.GetComponent<UnityEngine.UI.Text>().text = "";

        // add the operators to the canvas
        operatorsText.GetComponent<UnityEngine.UI.Text>().text = string.Join("\n\n", problem.Operators.Select(op => op.GetFullStringDetails()).ToArray());
    }

    private void SetInitialAndGoalStatesCanvas()
    {
        GameObject StatesCanvas = GameObject.Find("StatesMenu");
        if (StatesCanvas == null)
        {
            Debug.LogError("Initial And Goal \"StatesCanvas\" not found");
            return;
        }
        // clear the canvas text in StatesMenu > BodyTitle > BodyCanvas > StatesCanvas > Text
        GameObject StatesText = StatesCanvas.transform.Find("BodyTitle").Find("BodyCanvas").Find("StatesCanvas").Find("Text").gameObject;
        StatesText.GetComponent<UnityEngine.UI.Text>().text = "";

        // add the initial and goal states to the canvas
        StatesText.GetComponent<UnityEngine.UI.Text>().text = $"Initial State:\n{string.Join(", ", problem.InitialState.Select(l => l.ToString()).ToArray())}"
                + $"\n\nGoal State:\n{string.Join(", ", problem.GoalState.Select(l => l.ToString()).ToArray())}";
    }

    private string GetOriginalName(string name)
    {
        return name switch
        {
            "SS" => "Socks and Shoes Problem",
            "MBCD" => "Milk, Bananas, Cordless Drill Problem",
            "GB" => "Groceries Buying Problem",
            "SP" => "Spare Tires Problem",
            _ => name,
        };
    }
}
