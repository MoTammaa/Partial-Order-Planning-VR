//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Demonstrates the use of the controller hint system
//
//=============================================================================

using UnityEngine;
using System.Collections;
using Valve.VR;
using Valve.VR.InteractionSystem;


//-------------------------------------------------------------------------
public class KeyBoardController : MonoBehaviour
{
	private GameObject NameKeyboardInputObject;

	void Start()
	{
		if (GameObject.Find("Name Menu") == null) print("Name Menu is null");
		if (GameObject.Find("Name Menu").transform.Find("BodyTitle") == null) print("BodyTitle is null");
		if (GameObject.Find("Name Menu").transform.Find("BodyTitle").Find("OnscreenKeyboardCanvas") == null) print("OnscreenKeyboardCanvas is null");
		if (GameObject.Find("Name Menu").transform.Find("BodyTitle").Find("OnscreenKeyboardCanvas").Find("KeyBoard") == null) print("Keyboard is null");
		if (GameObject.Find("Name Menu").transform.Find("BodyTitle").Find("OnscreenKeyboardCanvas").Find("KeyBoard").Find("KeyboardInput") == null) print("KeyboardInput is null");
		NameKeyboardInputObject = GameObject.Find("Name Menu").transform.Find("BodyTitle").Find("OnscreenKeyboardCanvas").Find("KeyBoard").Find("KeyboardInput").gameObject;
	}

	public void WriteText(string text)
	{
		// get the text of the "Text" object that called this function through the onClick event


		// get the object with name "TextCanvas" and the "Text" object that is a child of it 
		GameObject nameText = GameObject.Find("Name Menu").transform.Find("BodyTitle").Find("OnscreenKeyboardCanvas").Find("TextCanvas").Find("Text").gameObject;

		// append the text to the "Text" object that is a child of the "NameCanvas" object
		if (nameText != null)
		{
			string oldname = nameText.GetComponent<UnityEngine.UI.Text>().text;
			if (oldname == "Your Name" || oldname == "Please Enter Your Name")
			{
				nameText.GetComponent<UnityEngine.UI.Text>().text = "";
				oldname = "";
			}
			switch (text)
			{
				case "<<x":
					if (!string.IsNullOrEmpty(oldname))
						nameText.GetComponent<UnityEngine.UI.Text>().text = oldname[..^1];
					break;
				case "Edit":
					// show the keyboard "KeyboardInput"
					NameKeyboardInputObject.SetActive(true);

					// Remove the "Welcome" message
					nameText.GetComponent<UnityEngine.UI.Text>().text = PlayerPrefs.GetString("PlayerName");

					// change the text and listner of the Done_Edit button to "Done"
					GameObject Edit = GameObject.Find("OnscreenKeyboardCanvas").transform.Find("KeyBoard").Find("Done_Edit").gameObject;
					GameObject Edit_Text = Edit?.transform.Find("Text")?.gameObject;
					if (Edit_Text.GetComponent<UnityEngine.UI.Text>() == null) print("Done_Edit button is null");
					Edit_Text.GetComponent<UnityEngine.UI.Text>().text = "Done";
					if (Edit.GetComponent<UnityEngine.UI.Button>() == null) print("Done_Edit button is null");
					Edit.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
					Edit.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => WriteText("Done"));
					break;
				case "Done":
					if (oldname == "")
					{
						nameText.GetComponent<UnityEngine.UI.Text>().text = "Please Enter Your Name";
						break;
					}
					// save the name to the playerprefs
					PlayerPrefs.SetString("PlayerName", oldname);
					// hide the keyboard "KeyboardInput"
					NameKeyboardInputObject.SetActive(false);
					// change the text and listner of the Done_Edit button to "Edit Name"
					GameObject Done = GameObject.Find("OnscreenKeyboardCanvas").transform.Find("KeyBoard").Find("Done_Edit").gameObject;
					GameObject Done_Text = Done?.transform.Find("Text")?.gameObject;
					if (Done_Text.GetComponent<UnityEngine.UI.Text>() == null) print("Done_Edit button is null");
					Done_Text.GetComponent<UnityEngine.UI.Text>().text = "Edit Name";
					if (Done.GetComponent<UnityEngine.UI.Button>() == null) print("Done_Edit button is null");
					Done.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
					Done.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => WriteText("Edit"));

					// Welcome the player
					nameText.GetComponent<UnityEngine.UI.Text>().text = "Welcome " + oldname + "!";

					print("Player Name: " + PlayerPrefs.GetString("PlayerName"));
					break;
				case "Space":
					text = " ";
					if (oldname.Length < 14)
						nameText.GetComponent<UnityEngine.UI.Text>().text += text;
					break;
				default:
					if (oldname.Length < 14)
						nameText.GetComponent<UnityEngine.UI.Text>().text += text;
					break;
			}
		}
		else
		{
			Debug.Log("NameText is null");
		}


	}

	public void Hello()
	{
		Debug.Log("Hello");
	}








}
