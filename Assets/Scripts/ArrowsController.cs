using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ArrowsController : MonoBehaviour
{
    public static Stage stage = Stage.Name;
    public static Dictionary<string, GameObject> gameObjects = new();
    public static bool IsTeleportPressed { get { return Teleport.instance.TeleportVisible; } }
    // Start is called before the first frame update
    void Start()
    {
        gameObjects.TryAdd("Arrows", GameObject.Find("UserInput Preferences").transform.Find("Navigation").Find("Arrows").gameObject);
        gameObjects.TryAdd("ToName", gameObjects["Arrows"]?.transform.Find("ToName").gameObject);
        gameObjects.TryAdd("ToMode", gameObjects["Arrows"]?.transform.Find("ToMode").gameObject);
        gameObjects.TryAdd("ToProblem", gameObjects["Arrows"]?.transform.Find("ToProblem").gameObject);
        gameObjects.TryAdd("ToStrategy", gameObjects["Arrows"]?.transform.Find("ToStrategy").gameObject);
        gameObjects.TryAdd("ToReady", gameObjects["Arrows"]?.transform.Find("ToReady").gameObject);
        gameObjects.TryAdd("ToPlayArea", gameObjects["Arrows"]?.transform.Find("ToPlayArea").gameObject);
        gameObjects.TryAdd("ToAgenda", gameObjects["Arrows"]?.transform.Find("ToAgenda").gameObject);
        gameObjects.TryAdd("ToActionSpawn", gameObjects["Arrows"]?.transform.Find("ToActionSpawn").gameObject);
        gameObjects.TryAdd("ToCausalLinksOrOrderingConstraints", gameObjects["Arrows"]?.transform.Find("ToCausalLinksOrOrderingConstraints").gameObject);
        gameObjects.TryAdd("ToGraphArea", gameObjects["Arrows"]?.transform.Find("ToGraphArea").gameObject);
        gameObjects.TryAdd("ToThreats", gameObjects["Arrows"]?.transform.Find("ToThreats").gameObject);

        if (gameObjects["Arrows"] == null || gameObjects["ToName"] == null || gameObjects["ToMode"] == null
            || gameObjects["ToProblem"] == null || gameObjects["ToStrategy"] == null
            || gameObjects["ToReady"] == null || gameObjects["ToPlayArea"] == null
            || gameObjects["ToAgenda"] == null || gameObjects["ToActionSpawn"] == null
            || gameObjects["ToCausalLinksOrOrderingConstraints"] == null
            || gameObjects["ToGraphArea"] == null || gameObjects["ToThreats"] == null)
        {
            Debug.LogError("ArrowsController: One or more GameObjects not found");
        }

        gameObjects.TryAdd("PlanningProblem", GameObject.Find("Planning Problem Menu"));
        if (gameObjects["PlanningProblem"] == null)
        {
            Debug.LogError("ArrowsController: Planning Problem Menu not found");
        }
    }

    // Update is called once per frame
    void Update()
    {

        // get the position of the player
        GameObject player = GameObject.Find("Player");
        GameObject VRCamera = player.transform.Find("NoSteamVRFallbackObjects").Find("FallbackObjects").gameObject;
        // if the player is not using VR, then use the fallback camera ... check for the object being disabled
        if (VRCamera == null || !VRCamera.activeSelf)
        {
            VRCamera = player.transform.Find("SteamVRObjects").Find("VRCamera").gameObject;
        }
        Vector3 playerPosition = VRCamera.transform.position;

        Vector3 PlannigProblemPosition = gameObjects["PlanningProblem"].transform.position;
        if (Vector3.Distance(playerPosition, PlannigProblemPosition) < 2.0f)
        {
            // check mode
            if (PlayerPrefs.HasKey("Mode"))
            {
                if (PlayerPrefs.GetString("Mode") == "Spectator")
                {
                    stage = Stage.Strategy;
                }
                else
                {
                    stage = Stage.Ready;
                }
            }
        }

        // hide all
        gameObjects["ToName"].SetActive(false);
        gameObjects["ToMode"].SetActive(false);
        gameObjects["ToProblem"].SetActive(false);
        gameObjects["ToStrategy"].SetActive(false);
        gameObjects["ToReady"].SetActive(false);
        gameObjects["ToPlayArea"].SetActive(false);
        gameObjects["ToAgenda"].SetActive(false);
        gameObjects["ToActionSpawn"].SetActive(false);
        gameObjects["ToCausalLinksOrOrderingConstraints"].SetActive(false);
        gameObjects["ToGraphArea"].SetActive(false);
        gameObjects["ToThreats"].SetActive(false);

        if (IsTeleportPressed)
        {
            if (stage != Stage.Problem && stage != Stage.Strategy)
            {
                gameObjects["PlanningProblem"].SetActive(false);
            }
            else
            {
                gameObjects["PlanningProblem"].SetActive(true);
            }
            switch (stage)
            {
                case Stage.Name:
                    gameObjects["ToName"].SetActive(true);
                    break;
                case Stage.Mode:
                    gameObjects["ToMode"].SetActive(true);
                    break;
                case Stage.Problem:
                    gameObjects["ToProblem"].SetActive(true);
                    break;
                case Stage.Strategy:
                    gameObjects["ToStrategy"].SetActive(true);
                    break;
                case Stage.Ready:
                    gameObjects["ToReady"].SetActive(true);
                    break;
                case Stage.PlayArea:
                    gameObjects["ToPlayArea"].SetActive(true);
                    if (PlayerPrefs.HasKey("Mode")) if (PlayerPrefs.GetString("Mode") != "Spectator")
                            gameObjects["ToAgenda"].SetActive(true);
                        else gameObjects["ToGraphArea"].SetActive(true);
                    break;
                case Stage.Agenda:
                    gameObjects["ToAgenda"].SetActive(true);
                    break;
                case Stage.ActionsSpawn:
                    gameObjects["ToActionSpawn"].SetActive(true);
                    gameObjects["ToGraphArea"].SetActive(true);
                    break;
                case Stage.CausalLinksOrOrderingConstraints:
                    gameObjects["ToCausalLinksOrOrderingConstraints"].SetActive(true);
                    break;
                case Stage.GraphArea:
                    gameObjects["ToGraphArea"].SetActive(true);
                    break;
                case Stage.Threats:
                    gameObjects["ToThreats"].SetActive(true);
                    break;
                default:
                    break;
            }
        }
        else
        {
            gameObjects["PlanningProblem"].SetActive(true);
        }
    }
    public enum Stage
    {
        Name,
        Mode,
        Problem,
        Strategy,
        Ready,
        PlayArea,
        Agenda,
        ActionsSpawn,
        CausalLinksOrOrderingConstraints,
        GraphArea,
        Threats

    }
}
