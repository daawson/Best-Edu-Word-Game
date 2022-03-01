using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public float scrollSpeed;           // Defines the speed of paralax
    public GameObject[] children;       // To hold all the children.
    public float xOffset = 20;          // Offset at which the background will loop
    void Start()
    {
        // Stores all the children in one array
        children = new GameObject[3];
        children[0] = gameObject.transform.GetChild(0).gameObject;
        children[1] = gameObject.transform.GetChild(1).gameObject;
        children[2] = gameObject.transform.GetChild(2).gameObject;
    }
    void Update()
    {
        foreach(GameObject g in children)   // Loops through each.
        {
            g.transform.position -= Vector3.right * scrollSpeed * Time.deltaTime;      // Moves them to the left
            if (g.transform.position.x <= -xOffset) g.transform.position = new Vector3(xOffset*2, 0, 0);        //If at the offset limit, move them at the start of the background loop.
        }
    }
}
