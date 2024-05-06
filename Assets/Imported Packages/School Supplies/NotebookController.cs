using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NotebookController : MonoBehaviour
{
    private bool Opening = false, Opened = true;
    private bool Closing = false, Closed = false;
    private bool checking = false;
    public static bool TURNED_ON = true;
    private Vector3 originalAgendaScale;

    // Start is called before the first frame update
    void Start()
    {
        // Open();
        // Close();
        originalAgendaScale = this.transform.parent.localScale;

    }

    // Update is called once per frame
    void Update()
    {

        StartCoroutine(StartCheck());
    }

    IEnumerator StartCheck()
    {
        this.transform.parent.localScale = TURNED_ON ? originalAgendaScale : new Vector3(0.0f, 0.0f, 0.0f);
        if (checking || !TURNED_ON) yield break;
        checking = true;
        yield return new WaitForSeconds(0.5f);
        // TODO: needs testing first with VR

        // get the position of the player
        GameObject player = GameObject.Find("Player");
        GameObject VRCamera = player.transform.Find("NoSteamVRFallbackObjects").Find("FallbackObjects").gameObject;
        // if the player is not using VR, then use the fallback camera ... check for the object being disabled
        if (VRCamera == null || !VRCamera.activeSelf)
        {
            VRCamera = player.transform.Find("SteamVRObjects").Find("VRCamera").gameObject;
        }
        Vector3 playerPosition = VRCamera.transform.position;

        // get the position of the notebook
        Vector3 notebookPosition = this.transform.position;
        // get the distance between the player and the notebook
        float distance = Vector3.Distance(playerPosition, notebookPosition);
        // if the player is close to the notebook and the notebook is not opening or closing then open the notebook
        if (distance < 3.5 && !Opening && !Opened)
        {
            print("Opening");
            //if closing, stop closing
            if (Closing)
            {
                StopCoroutine(CloseNotebook());
                Closing = false;
                Closed = false;
            }
            Opening = true;
            Open();
        }
        // if the player is far from the notebook and the notebook is not opening or closing then close the notebook
        else if (distance > 3.5 && !Closing && !Closed)
        {
            print("Closing");
            // if opening, stop opening
            if (Opening)
            {
                StopCoroutine(OpenNotebook());
                Opening = false;
                Opened = false;
            }
            Closing = true;
            Close();
        }
        checking = false;
    }

    public void Open()
    {
        StartCoroutine(OpenNotebook());
    }

    public void Close()
    {
        StartCoroutine(CloseNotebook());
    }

    public IEnumerator OpenNotebook()
    {
        if (this.transform?.Find("Body")?.Find("notebook_left") == null) yield break;

        float zRotation = this.transform.Find("Body").Find("notebook_left").localEulerAngles.z;
        if (zRotation > 180) zRotation -= 360; // Convert to negative angle if necessary

        float zPosition = this.transform.localPosition.z;

        while (zRotation < 45 || zPosition > -1.54f)
        {
            if (zPosition > -1.54f)
            {
                this.transform.localPosition -= new Vector3(0, 0, 0.015f * Time.deltaTime * 100);
                GameObject bodyTitle = this.transform.parent.Find("BodyTitle").gameObject, achieversMenu = this.transform.parent.Find("Achievers Menu").gameObject;
                bodyTitle.transform.localPosition = new Vector3(bodyTitle.transform.localPosition.x, bodyTitle.transform.localPosition.y, this.transform.localPosition.z - 0.11f);
                achieversMenu.transform.localPosition = bodyTitle.transform.localPosition;
            }
            if (zRotation < 45) this.transform.Find("Body").Find("notebook_left").Rotate(0, 0, 1 * Time.deltaTime * 100);
            yield return new WaitForSeconds(0.01f);
            zRotation = this.transform.Find("Body").Find("notebook_left").localEulerAngles.z;
            zPosition = this.transform.localPosition.z;
            if (zRotation > 180) zRotation -= 360;
        }
        Opened = true;
        Opening = Closing = Closed = false;
    }

    public IEnumerator CloseNotebook()
    {
        if (this.transform?.Find("Body")?.Find("notebook_left") == null) yield break;

        float zRotation = this.transform.Find("Body").Find("notebook_left").localEulerAngles.z;
        if (zRotation > 180) zRotation -= 360; // Convert to negative angle if necessary

        float zPosition = this.transform.localPosition.z;

        while (zRotation > -83 || zPosition < 0.11)
        {
            if (zPosition < 0.11)
            {
                this.transform.localPosition += new Vector3(0, 0, 0.015f * Time.deltaTime * 100);
                GameObject bodyTitle = this.transform.parent.Find("BodyTitle").gameObject, achieversMenu = this.transform.parent.Find("Achievers Menu").gameObject;
                bodyTitle.transform.localPosition = new Vector3(bodyTitle.transform.localPosition.x, bodyTitle.transform.localPosition.y, this.transform.localPosition.z - 0.11f);
                achieversMenu.transform.localPosition = bodyTitle.transform.localPosition;
            }

            if (zRotation > -83) this.transform.Find("Body").Find("notebook_left").Rotate(0, 0, -1 * Time.deltaTime * 100);
            yield return new WaitForSeconds(0.01f);
            zRotation = this.transform.Find("Body").Find("notebook_left").localEulerAngles.z;
            zPosition = this.transform.localPosition.z;
            if (zRotation > 180) zRotation -= 360;
        }
        Closed = true;
        Closing = Opening = Opened = false;
    }

    // public IEnumerator FlipPage()
    // {
    //     float zfrontRotation = this.transform.Find("Body").Find("Page").Find("notebook_front").localEulerAngles.z;
    //     float zbackRotation = this.transform.Find("Body").Find("Page").Find("notebook_back").localEulerAngles.z;
    //     if (zfrontRotation > 180) zfrontRotation -= 360; // Convert to negative angle if necessary
    //     if (zbackRotation > 180) zbackRotation -= 360; // Convert to negative angle if necessary

    //     while (zfrontRotation < -45 && zbackRotation < 135)
    //     {
    //         this.transform.Find("Body").Find("Page").Find("notebook_front").Rotate(0, 0, 1 * Time.deltaTime * 100);
    //         this.transform.Find("Body").Find("Page").Find("notebook_back").Rotate(0, 0, 1 * Time.deltaTime * 100);
    //         yield return new WaitForSeconds(0.01f);
    //         zfrontRotation = this.transform.Find("Body").Find("Page").Find("notebook_front").localEulerAngles.z;
    //         zbackRotation = this.transform.Find("Body").Find("Page").Find("notebook_back").localEulerAngles.z;
    //         if (zfrontRotation > 180) zfrontRotation -= 360;
    //         if (zbackRotation > 180) zbackRotation -= 360;
    //     }


    // }

}
