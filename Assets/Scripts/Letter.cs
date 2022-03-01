using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Letter : MonoBehaviour
{
    GameManager gm;                     // GameManager variable

    public string letter;               // Objects letter
    public bool isNeeded = false;       // Is this a part of currently chosen word?

    [SerializeField]
    private TMP_Text letterText;        // Text object


    // Bonus object variables and UI elements
    [SerializeField]
    private GameObject bonusSprite;

    [SerializeField]
    private Sprite pointsPrefab;
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

        letter = _letter;

        if (_letter == "*") { bonusSprite.GetComponent<SpriteRenderer>().sprite = pointsPrefab; bonusSprite.SetActive(true); }        // Set the sprite to Best Education Logo
        else if (_letter == "#") { bonusSprite.GetComponent<SpriteRenderer>().sprite = timerPrefab; bonusSprite.SetActive(true); }    // Set the sprite to timer
        else if (_letter == "$") { bonusSprite.GetComponent<SpriteRenderer>().sprite = heartPrefab; bonusSprite.SetActive(true); }    // Set the sprite to heart
        else letterText.text = letter;

        
        isNeeded = _isNeeded;
    }

    private void OnMouseDown()          // Route the click on the letter to GameManager instance
    {
        gm.ClickedOnLetter(gameObject, letter);
    }


    /// <summary>
    /// Update, and OnTrigger functions reposition the letter when it collides with other letter or is under the UI elements.
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
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Letter"))
        {
            transform.position = new Vector3(Random.Range(-(gm.cameraWidth - gm.edgeOffset + 1) / 2, (gm.cameraWidth - gm.edgeOffset) / 2), Random.Range((-gm.cameraHeight - gm.edgeOffset) / 2, (gm.cameraHeight - gm.edgeOffset) / 2), 0);
        }
    }
}
