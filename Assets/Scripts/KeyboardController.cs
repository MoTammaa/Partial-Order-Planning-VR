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

					// unlock "Level1 Locked" SteamVR teleport areas and points
					GameObject Teleports = GameObject.Find("Teleports");
					GameObject Level1Locked = Teleports.transform.Find("Level1 Locked").gameObject;
					foreach (Transform child in Level1Locked.transform)
					{
						child.gameObject.GetComponent<TeleportArea>()?.SetLocked(false);
						child.gameObject.GetComponent<TeleportPoint>()?.SetLocked(false);
					}
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

	public void WriteDepthNumber(string number)
	{
		// get the PreferencesController script instance variable in the Strategy Menu
		GameObject depthText = PreferencesController.DepthLimitCanvasObject.transform.Find("BodyTitle").Find("BodyCanvas").Find("OnscreenNumberpadCanvas").Find("NumberCanvas").Find("Text").gameObject;

		// append the text to the "Text" object that is a child of the "DepthLimit" object
		if (depthText != null)
		{
			string olddepth = depthText.GetComponent<UnityEngine.UI.Text>().text;
			if (olddepth == "Please Enter Number" || olddepth == "0" || olddepth == "00" || olddepth == "000")
			{
				depthText.GetComponent<UnityEngine.UI.Text>().text = "";
				olddepth = "";
			}
			switch (number)
			{
				case "<<x":
					if (!string.IsNullOrEmpty(olddepth))
						depthText.GetComponent<UnityEngine.UI.Text>().text = olddepth[..^1];
					break;
				case "Done":
					if (olddepth == "")
					{
						depthText.GetComponent<UnityEngine.UI.Text>().text = "Please Enter Number";
						break;
					}
					// save the depth limit to the playerprefs
					PlayerPrefs.SetInt("DepthLimit", int.Parse(olddepth));
					// get depth menu from the PreferencesController script in the Strategy Menu and hide the keyboard "KeyboardInput"
					PreferencesController.DepthLimitKeyboard.transform.Find("KeyboardInput").gameObject.SetActive(false);

					// change the text and listner of the Done_Edit button to "Edit"
					GameObject Done = PreferencesController.DepthLimitKeyboard.transform.Find("Done_Edit").gameObject;
					GameObject Done_Text = Done?.transform.Find("Text")?.gameObject;
					if (Done_Text.GetComponent<UnityEngine.UI.Text>() == null) print("Done_Edit button is null");
					Done_Text.GetComponent<UnityEngine.UI.Text>().text = "Edit";
					if (Done.GetComponent<UnityEngine.UI.Button>() == null) print("Done_Edit button is null");
					Done.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
					Done.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => WriteDepthNumber("Edit"));

					break;
				case "Edit":
					// show the keyboard "KeyboardInput"
					PreferencesController.DepthLimitKeyboard.transform.Find("KeyboardInput").gameObject.SetActive(true);

					// change the text and listner of the Done_Edit button to "Done"
					GameObject Edit = PreferencesController.DepthLimitKeyboard.transform.Find("Done_Edit").gameObject;
					GameObject Edit_Text = Edit?.transform.Find("Text")?.gameObject;
					if (Edit_Text.GetComponent<UnityEngine.UI.Text>() == null) print("Done_Edit button is null");
					Edit_Text.GetComponent<UnityEngine.UI.Text>().text = "Done";
					if (Edit.GetComponent<UnityEngine.UI.Button>() == null) print("Done_Edit button is null");
					Edit.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
					Edit.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => WriteDepthNumber("Done"));
					break;
				default:
					if (olddepth.Length < 3)
						depthText.GetComponent<UnityEngine.UI.Text>().text += number;
					break;
			}
		}
		else
		{
			Debug.Log("DepthText is null");
		}
	}








}
