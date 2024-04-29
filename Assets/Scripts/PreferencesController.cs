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

    private string mode;


    // Start is called before the first frame update
    void Start()
    {
        SetMode("Creative");
    }

    public void SetMode(string mode)
    {
        if (mode != "Creative" && mode != "Survival" && mode != "Spectator")
        {
            Debug.LogError("Invalid mode");
            return;
        }

        this.mode = mode;
        // save the chosen mode to the playerprefs
        PlayerPrefs.SetString("Mode", mode);

        // set the chosen mode description in the canvas
        GameObject BodyCanvas = GameObject.Find("Mode Menu").transform.Find("BodyTitle").gameObject;
        BodyCanvas.transform.Find("DescriptionCanvas").Find("Text").GetComponent<UnityEngine.UI.Text>().text = GetModeDescription(mode);

        // Set All unchosen buttons Normal color to "#FFFFFF" color and the chosen button to "#6B6B6B" color in the "Buttons" parent object
        GameObject buttons = GameObject.Find("Mode Menu").transform.Find("BodyTitle").Find("BodyCanvas").Find("Buttons").gameObject;
        foreach (Transform button in buttons.transform)
        {
            if (button.name == mode)
            {
                button.GetComponent<UnityEngine.UI.Button>().GetComponent<UnityEngine.UI.Image>().color = new Color(0.4196078f, 0.4196078f, 0.4196078f);
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
            if (button.name != mode)
                button.gameObject.SetActive(false);
        }

    }
}
