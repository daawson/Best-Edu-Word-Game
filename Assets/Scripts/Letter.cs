using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Letter : MonoBehaviour
{
    GameManager gm;                     // GameManager variable

    public string letter;               // Objects letter
    public bool isNeeded = false;       // Is this a part of currently chosen word?

    [SerializeField]
    private Text letterMesh;            // Text object


    // Bonus object variables and UI elements
    private Sprite bonusSprite;
    [SerializeField]
    private Sprite starPrefab;
    [SerializeField]
    private Sprite timerPrefab;
    [SerializeField]
    private Sprite heartPrefab;

    private void Start()
    {
        gm = GameManager.instance;
    }

    public void SetupLetter(string _letter, bool _isNeeded)    // Letter setup
    {

        if (_letter == "*") { }         // STAR             - Extra points
        else if (_letter == "#") { }    // BEST EDU LOGO    - Extra time
        else if (_letter == "$") { }    // HEART            - Extra lives
        else
        {
            letter = _letter;
            letterMesh.text = letter;
        }

        isNeeded = _isNeeded;
    }

    private void OnMouseDown()          // Route the click on the letter to GameManager instance
    {
        gm.ClickedOnLetter(gameObject, letter);
    }


    /// <summary>
    /// Update, and OnTrigger functions reposition the letter when it collides with other letter or is on the UI
    /// </summary>
    private void Update()
    {
        if(transform.position.y > 2f || transform.position.y < -3f || transform.position.x > 8f || transform.position.x < -7.9f)
        {
            transform.position = new Vector3(Random.Range(-(gm.cameraWidth - gm.edgeOffset + 1) / 2, (gm.cameraWidth - gm.edgeOffset) / 2), Random.Range((-gm.cameraHeight - gm.edgeOffset) / 2, (gm.cameraHeight - gm.edgeOffset) / 2), 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Letter")){
            transform.position = new Vector3(Random.Range(-(gm.cameraWidth - gm.edgeOffset + 1) / 2, (gm.cameraWidth - gm.edgeOffset) / 2), Random.Range((-gm.cameraHeight - gm.edgeOffset) / 2, (gm.cameraHeight - gm.edgeOffset) / 2), 0);
            Debug.Log("COLLISION!");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Letter"))
        {
            transform.position = new Vector3(Random.Range(-(gm.cameraWidth - gm.edgeOffset + 1) / 2, (gm.cameraWidth - gm.edgeOffset) / 2), Random.Range((-gm.cameraHeight - gm.edgeOffset) / 2, (gm.cameraHeight - gm.edgeOffset) / 2), 0);
            Debug.Log("COLLISION!");
        }
    }
}
