using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using POP;
using System.Linq;

public class PreferencesController : MonoBehaviour
{

    private static string Mode;
    private static string searchStrategy;
    public static string SearchStrategy { get { return searchStrategy; } }
    private static GameObject DepthLimitCanvas;
    public static GameObject DepthLimitCanvasObject { get { return DepthLimitCanvas; } }
    public static GameObject DepthLimitKeyboard { get { return DepthLimitCanvas?.transform.Find("BodyTitle")?.Find("BodyCanvas")?.Find("OnscreenNumberpadCanvas")?.Find("KeyBoard")?.gameObject; } }
    public static bool GameStarted { get; set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        DepthLimitCanvas = GameObject.Find("Strategy Menu").transform.Find("Depth Menu").gameObject;
        if (PlayerPrefs.HasKey("Mode"))
        {
            SetMode(PlayerPrefs.GetString("Mode"));
        }
        else
        {
            SetMode("Creative");
        }
        if (DepthLimitCanvas == null)
        {
            print("DepthLimitCanvas is null");
        }
        else
        {
            SetSearchStrategy("A*");
        }
    }

    public void SetMode(string mode)
    {
        if (mode != "Creative" && mode != "Survival" && mode != "Spectator")
        {
            Debug.LogError("Invalid mode");
            return;
        }

        Mode = mode;
        // save the chosen mode to the playerprefs
        PlayerPrefs.SetString("Mode", mode);

        // set the chosen mode description in the canvas
        GameObject BodyCanvas = GameObject.Find("Mode Menu").transform.Find("BodyTitle").gameObject;
        BodyCanvas.transform.Find("DescriptionCanvas").Find("Text").GetComponent<UnityEngine.UI.Text>().text = GetModeDescription(mode);

        // Set All unchosen buttons Normal color to "#FFFFFF" color and the chosen button to "#FF0000" color in the "Buttons" parent object
        GameObject buttons = GameObject.Find("Mode Menu").transform.Find("BodyTitle").Find("BodyCanvas").Find("Buttons").gameObject;
        foreach (Transform button in buttons.transform)
        {
            if (button.name == mode)
            {
                button.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);
            }
            else
            {
                button.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);
            }
        }
    }

    private string GetModeDescription(string mode)
    {
        return mode switch
        {
            "Creative" => "In Creative mode, you have unlimited time & resources and can build and create to your heart's content (Of course with the limitation of Partial Order Planning).",
            "Survival" => "In Survival mode, you are limited by time and mistakes, and you will have to face the consequences of your \"actions\". Soon there will be real Threats.",
            "Spectator" => "In Spectator mode, you have your remote controller so you can sit on the couch or walk around and observe the beauty of the Algorithm in action. (but please make some effort & popcorn)",
            _ => "Invalid mode",
        };
    }

    public void ModeDone()
    {
        // hide the unrelevant buttons
        GameObject Buttons = GameObject.Find("Mode Menu").transform.Find("BodyTitle").Find("BodyCanvas").Find("Buttons").gameObject;
        foreach (Transform button in Buttons.transform)
        {
            if (button.name != Mode)
                button.gameObject.SetActive(false);
        }

        if (Mode == "Survival" || Mode == "Creative")
        {
            // Hide the strategy menu
            GameObject.Find("Strategy Menu").SetActive(false);
        }
        else
        {
            // Hide the Agenda Menu
            GameObject.Find("Agenda Menu").SetActive(false);
            // Hide the CausalLinks & OrderingConstraints Menu
            GameObject.Find("CausalLinks Menu").SetActive(false);
            GameObject.Find("OrderingConstraints Menu").SetActive(false);
            // Hide the PC Setup
            GameObject.Find("PC Setup").SetActive(false);
            // Hide the Actions
            GameObject.Find("Actions").SetActive(false);
            // Hide the Mid_PlayArea_RoundSpawnObject
            GameObject.Find("Temple Enviroment").transform.Find("Other Stone Temple objs").Find("Mid_PlayArea_RoundSpawnObject").gameObject.SetActive(false);

        }
        // unlock PreferencesArea TeleportArea
        GameObject Teleports = GameObject.Find("Teleports");
        GameObject PreferencesArea = Teleports.transform.Find("PreferencesArea").gameObject;
        PreferencesArea.GetComponent<TeleportArea>()?.SetLocked(false);

        // set ArrowController.stage to Problem
        ArrowsController.stage = ArrowsController.Stage.Problem;

        StartCoroutine(MoveModeBack());
    }

    public IEnumerator MoveModeBack()
    {
        GameObject ModeMenu = GameObject.Find("Mode Menu");
        Vector3 currentPosition = ModeMenu.transform.position;
        Vector3 targetPosition = currentPosition + new Vector3(0, 0, 3.545f);

        while (Vector3.Distance(ModeMenu.transform.position, targetPosition) > 0.01f)
        {
            ModeMenu.transform.position = Vector3.Lerp(ModeMenu.transform.position, targetPosition, 0.1f);
            yield return new WaitForSeconds(0.01f);
        }

        ModeMenu.transform.position = targetPosition;
    }

    public void SetSearchStrategy(string strategy)
    {
        if (strategy != "BFS" && strategy != "DFS" && strategy != "A*")
        {
            Debug.LogError("Invalid strategy");
            return;
        }
        searchStrategy = strategy;

        // save the chosen strategy to the playerprefs
        PlayerPrefs.SetString("SearchStrategy", strategy);

        // set the chosen strategy description in the canvas
        GameObject BodyCanvas = GameObject.Find("Strategy Menu").transform.Find("BodyTitle").gameObject;
        BodyCanvas.transform.Find("DescriptionCanvas").Find("Text").GetComponent<UnityEngine.UI.Text>().text = GetStrategyDescription(strategy);

        // Set All unchosen buttons Normal color to "#FFFFFF" color and the chosen button to "#FF0000" color in the "Buttons" parent object
        GameObject buttons = GameObject.Find("Strategy Menu").transform.Find("BodyTitle").Find("BodyCanvas").Find("Buttons").gameObject;
        foreach (Transform button in buttons.transform)
        {
            if (button.name == searchStrategy)
            {
                button.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.0f, 0.0f);
            }
            else
            {
                button.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);
            }
        }

        if (strategy == "DFS")
        {
            DepthLimitCanvas.SetActive(true);
            updateSuggestedDepth();
            // add the suggested depth to the ui
            DepthLimitCanvas.transform.Find("BodyTitle").Find("BodyCanvas").Find("OnscreenNumberpadCanvas").Find("NumberCanvas").Find("Text").GetComponent<UnityEngine.UI.Text>().text = PlayerPrefs.GetInt("RecommendedDepthForDFS").ToString();
            // add the suggested depth to the playerprefs
            PlayerPrefs.SetInt("DepthLimit", PlayerPrefs.GetInt("RecommendedDepthForDFS"));
        }
        else
        {
            DepthLimitCanvas.SetActive(false);
        }
    }

    private string GetStrategyDescription(string strategy)
    {
        return strategy switch
        {
            "BFS" => "Breadth-first search (BFS) is an algorithm for traversing or searching tree or graph level by level. It can be considered \"FIFO\". It starts at the tree root (or some arbitrary node of a graph) and explores the neighbor (direct children) nodes first, before moving to the next level neighbors.",
            "DFS" => "Depth-first search (DFS) is (in most cases) the slowest of the 3, but yet easier to understand as it shows how the planner decides to change its mind about bad decisions. It can be considered \"FIFO\". The algorithm starts at the root node and explores as far and deep as possible along each branch before going back and trying a different branch. Since we are going \"deep\" we may end up in a infinite branch, so we will make it depth-limited, and YOU will have to choose the depth (but don't worry, we will give you a suggestion).\n\t<==LOOK TO THE LEFT!",
            "A*" => "A* is an algorithm that is used in optimal pathfinding. It has a priority queue of nodes, and the next node to be pulled depends on its priority which is a combination of the cost of the path and a heuristic function.\n Here f(n) = h(n) + g(n)\n\t-h(n) = open preconditions of the partial plan (inside the agenda) \n\t-g(n) = path cost from the start node to the current node.",
            _ => "Invalid strategy",
        };
    }

    public static void updateSuggestedDepth()
    {
        string number = PlayerPrefs.GetInt("RecommendedDepthForDFS").ToString();
        DepthLimitCanvas.transform.Find("BodyTitle").Find("BodyCanvas").Find("OnscreenNumberpadCanvas").Find("SuggestionCanvas").Find("Text").GetComponent<UnityEngine.UI.Text>().text = "Suggestion: \n" + number + "\n DO NOT SET LESS THAN THIS NUMBER, OR IT MAY RUN FOREVER!";
    }

    public void StrategyDone()
    {
        // hide the unrelevant buttons
        GameObject Buttons = GameObject.Find("Strategy Menu").transform.Find("BodyTitle").Find("BodyCanvas").Find("Buttons").gameObject;
        // save the recommended depth to the playerprefs as DepthLimit if the depth is not set
        if (PlayerPrefs.HasKey("RecommendedDepthForDFS") && !PlayerPrefs.HasKey("DepthLimit"))
        {
            PlayerPrefs.SetInt("DepthLimit", PlayerPrefs.GetInt("RecommendedDepthForDFS"));
        }
        else if (PlayerPrefs.HasKey("RecommendedDepthForDFS"))
        {
            if (PlayerPrefs.GetInt("DepthLimit") < PlayerPrefs.GetInt("RecommendedDepthForDFS"))
            {
                PlayerPrefs.SetInt("DepthLimit", PlayerPrefs.GetInt("RecommendedDepthForDFS"));
            }
        }
        foreach (Transform button in Buttons.transform)
        {
            if (button.name != searchStrategy)
                button.gameObject.SetActive(false);
        }
        // set ArrowController.stage to Ready
        ArrowsController.stage = ArrowsController.Stage.Ready;
    }

    public void StartGame()
    {
        GameStarted = true;
        // hide the Strategy Menu
        GameObject.Find("Strategy Menu")?.SetActive(false);
        // hide the Planning Problem Menu
        GameObject.Find("Planning Problem Menu")?.SetActive(false);
        // hide the Ready Menu
        GameObject.Find("Ready Menu")?.SetActive(false);

        // Get EngineStart object and enable the disabled POPEngineDriverController script
        GameObject.Find("EngineStart").GetComponent<POPEngineDriverController>().enabled = true;

        //Unlock the "Level2 Locked" TeleportAreas
        GameObject Teleports = GameObject.Find("Teleports");
        GameObject Level2Locked = Teleports.transform.Find("Level2 Locked").gameObject;
        foreach (Transform child in Level2Locked.transform)
        {
            child.gameObject.GetComponent<TeleportArea>()?.SetLocked(false);
            child.gameObject.GetComponent<TeleportPoint>()?.SetLocked(false);
        }

        // set ArrowController.stage to PlayArea
        ArrowsController.stage = ArrowsController.Stage.PlayArea;

    }
}
