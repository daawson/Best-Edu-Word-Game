using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;                 // Only one GameManager per game

    // GAME VARIABLES
    string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    int gameTime;
    int gameScore;
    int gameLives = 3;
    public string currentWord;
    public string previewCurrentWord;
    int clickedLetterCount = 0;

    public List<string> wordsData = new List<string>();        // RAW WORDS
    public List<string> availableWords = new List<string>();   // SORTED FOR WORDS >= 4 LETTERS ~ 970 WORDS.
    public List<string> usedWords = new List<string>();        // WORDS THAT YOU COMPLETED

    // UI GAMEOBJECTS
    [SerializeField]
    private TMP_Text _wordText;
    [SerializeField]
    private TMP_Text _previewWordText;
    [SerializeField]
    private TMP_Text _scoreText;
    [SerializeField]
    private Slider _timeSlider;


    // Game objects
    Camera cameraMain;                                  // Camera object
    public float cameraHeight, cameraWidth;
    public float edgeOffset = 4f;

    [SerializeField]
    private GameObject letterPrefab;                    // Letter GameObject prefab

    GameObject lettersHolder;                           // Empty GameObject that holds all the letters

    void Start()
    {
        // Make instance, so that GameManager will be accesible outside this class and to not accidentaly make another instance of GameManager.
        // Accesing GameManager only by GamaManager gm = GamaManager.instance;
        if (instance == null) instance = this;

        cameraMain = Camera.main;
        cameraHeight = 2f * cameraMain.orthographicSize;
        cameraWidth = cameraHeight * cameraMain.aspect;

        // Format text file to array.
        LoadWords();

        lettersHolder = GameObject.Find("Letters");
        string check = "best";
        SetupWord(check);

    }
    void Update()
    {
        _scoreText.text = "Score: " + gameScore;

        if (Input.GetKeyDown(KeyCode.Space))
            WonTheRound();
    }

    void LoadWords()
    {
        // Load and prepare words array
        string path = "Assets/words.txt";
        StreamReader reader = new StreamReader(path);
        string words_preformat = reader.ReadToEnd();
        reader.Close();

        // Formating the words, and adding it to Words List
        string[] words = words_preformat.Split('\n');
        foreach (var word in words)
        {
            // Removing Quotes ' "" '
            string a = word.Replace("\"", string.Empty);

            // If it has 4 or more letters add to Words List
            if (a.Length >= 4)
            {
                wordsData.Add(a);
            }
        }
        availableWords = wordsData;
    }

    void SetupWord(string word) 
    {
        clickedLetterCount = 0;
        if(lettersHolder.transform.childCount > 0)
        {
            foreach (Transform child in lettersHolder.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        currentWord = word.ToUpper();                   // Uppercasing the letters
        _wordText.text = currentWord;                   // Updating info text

        // Updating preview of clicked letters
        string a = "";
        for (int i = 0; i < currentWord.Length; i++) a += "_";
        previewCurrentWord = a;
        _previewWordText.text = a;

        GenerateLetters(currentWord);
    }

    // Generates letters on the screen
    void GenerateLetters(string word)
    {
        int lettersOnScreen = 0;
        string availableAlphabet = alphabet;

        // Generate initial word letter's
        for (int i = 0; i < word.Length; i++)
        {
            GameObject l = Instantiate(letterPrefab, lettersHolder.transform);              // Initiate the letters
            l.transform.position = new Vector3(Random.Range(-cameraWidth / 2, cameraWidth / 2), Random.Range(-cameraHeight / 2, cameraHeight / 2), 0);  // Positon the letter
            l.GetComponent<Letter>().SetupLetter(word[i].ToString().ToUpper(), true);       // Setup the letter
            lettersOnScreen++;                                                              // Count the letters on the screen
            availableAlphabet = availableAlphabet.Replace(""+word[i], string.Empty);        // Remove this letter from available random letters
        }

        // Generate random filling letters
        for (int i = lettersOnScreen; i < 20; i++)
        {
            GameObject l = Instantiate(letterPrefab, lettersHolder.transform);
            l.transform.position = new Vector3(Random.Range(-cameraWidth / 2, cameraWidth / 2), Random.Range(-cameraHeight / 2, cameraHeight / 2), 0);
            l.GetComponent<Letter>().SetupLetter(availableAlphabet[Random.Range(0, availableAlphabet.Length)].ToString(), true);
            lettersOnScreen++;
        }
    }


    void WonTheRound()
    {
        // Update the arrays of available words
        usedWords.Add(currentWord.ToLower());
        availableWords.Remove(currentWord.ToLower());

        gameScore += 1000;

        // Randomize next word, and setup it on the screen
        string nextWord = availableWords[Random.Range(0, availableWords.Count)];
        nextWord = nextWord.Remove(nextWord.Length - 1, 1);
        SetupWord(nextWord);
    }

    void LostTheRound()
    {

    }

    void WrongLetterClicked()
    {
        gameLives--;
    }

    void BonusClicked(string letter)
    {
        if (letter == "*") { }         // STAR             - Extra points
        else if (letter == "#") { }    // BEST EDU LOGO    - Extra time
        else if (letter == "$") { }    // HEART            - Extra lives
    }

    // Called by a Letter's GameObject on click
    public void ClickedOnLetter(GameObject caller, string letter)
    {
        Debug.Log(letter + " Clicked");

        // If the letter clicked is one of the needed letters -- never remove it if its in the wrong order, keep it on the screen
        if(caller.GetComponent<Letter>().isNeeded)
        {
            // if its right letter
            if (currentWord[clickedLetterCount] == letter[0])
            {
                previewCurrentWord = previewCurrentWord.Remove(clickedLetterCount, 1).Insert(clickedLetterCount, letter[0].ToString());         // Update preview text
                _previewWordText.text = previewCurrentWord;
                Destroy(caller);                                                                                                                // Destroy clicked letter


                if (currentWord.Equals(previewCurrentWord)) WonTheRound();                                                                      // Win the round if word is complete
                else clickedLetterCount++;  // keep going with next letters


            }
            else
            {
                WrongLetterClicked();                                                                                                           // Wrong letter is clicked
                Destroy(caller);
            }
        }
        else
        {
            if (caller.GetComponent<Letter>().letter == "$" || caller.GetComponent<Letter>().letter == "#" || caller.GetComponent<Letter>().letter == "*")
                BonusClicked(caller.GetComponent<Letter>().letter);                                                                             // Player clicked on the bonus!

            Destroy(caller);
        }
    }
}
