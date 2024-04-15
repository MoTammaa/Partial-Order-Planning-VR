using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using POP;

public class BlockController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UpdateName("Hello");

    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateName(string name)
    {
        // update the text written on the block in this object >> Cube >> Operator Canvas F/B >> Background Button >> Text (TMP)
        transform.Find("Cube").Find("Operator Canvas F").Find("Background Button").Find("Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>().text = name;
        transform.Find("Cube").Find("Operator Canvas B").Find("Background Button").Find("Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>().text = name;
    }



    void Move(Vector3 direction)
    {
        StartCoroutine(MoveBlock(direction));
    }

    IEnumerator MoveBlock(Vector3 direction)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + direction;
        float moveTime = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < moveTime)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / moveTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
    }
}
