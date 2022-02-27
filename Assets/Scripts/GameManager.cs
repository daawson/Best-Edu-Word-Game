using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Text.RegularExpressions;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;                 // Only one GameManager per game

    // GAME VARIABLES
    string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    float gameTime = 10000;
    float gameMaxTime = 10000;
    int gameScore = 0;
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

        // Setting up camera variables
        cameraMain = Camera.main;
        cameraHeight = 2f * cameraMain.orthographicSize;
        cameraWidth = cameraHeight * cameraMain.aspect;

        // Format text file to array.
        LoadWords();

        lettersHolder = GameObject.Find("Letters");
        SetupWord("best");

    }
    void Update()
    {
        gameTime--;
        if(gameTime < 0)
        {
            gameLives--;
            if (gameLives < 0) Lost();
        }

        // Update the time slider
        _timeSlider.value = gameTime / gameMaxTime;
        // Update the score
        _scoreText.text = "Score: " + gameScore;
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
            //string a = word.Replace("\"", string.Empty);
            //a = Regex.Replace(a, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);

            string a = Regex.Replace(word, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);

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
            l.GetComponent<Letter>().SetupLetter(availableAlphabet[Random.Range(0, availableAlphabet.Length)].ToString(), false);
            lettersOnScreen++;
        }


        //Spawn Best Education Logo    = Extra Points
        float chance = Random.Range(0f, 1f);
        Debug.Log(chance);
        if (chance > 0.30f)
        {
            GameObject l = Instantiate(letterPrefab, lettersHolder.transform);
            l.transform.position = new Vector3(Random.Range(-cameraWidth / 2, cameraWidth / 2), Random.Range(-cameraHeight / 2, cameraHeight / 2), 0);
            l.GetComponent<Letter>().SetupLetter("*", false);
        }

        //Spawn a timer
        chance = Random.Range(0f, 1f);
        Debug.Log(chance);
        if (chance > 0.60f)
        {
            GameObject l = Instantiate(letterPrefab, lettersHolder.transform);
            l.transform.position = new Vector3(Random.Range(-cameraWidth / 2, cameraWidth / 2), Random.Range(-cameraHeight / 2, cameraHeight / 2), 0);
            l.GetComponent<Letter>().SetupLetter("#", false);
        }

        //Spawn a heart
        chance = Random.Range(0f, 1f);
        Debug.Log(chance);
        if (chance > 0.90f)
        {
            GameObject l = Instantiate(letterPrefab, lettersHolder.transform);
            l.transform.position = new Vector3(Random.Range(-cameraWidth / 2, cameraWidth / 2), Random.Range(-cameraHeight / 2, cameraHeight / 2), 0);
            l.GetComponent<Letter>().SetupLetter("$", false);
        }
    }


    void WonTheRound()
    {
        // Update the arrays of available words
        usedWords.Add(currentWord.ToLower());
        int index = availableWords.IndexOf(currentWord.ToLower());
        Debug.Log(index);
        availableWords.RemoveAt(index);        

        gameScore += 1000;
        gameTime = 10000;

        // Randomize next word, and setup it on the screen
        string nextWord = availableWords[Random.Range(0, availableWords.Count)];
        //nextWord = nextWord.Remove(nextWord.Length - 1, 1);
        SetupWord(nextWord);
    }

    void Lost()
    {

    }

    void WrongLetterClicked()
    {
        gameLives--;
    }

    void BonusClicked(string letter)
    {
        if (letter == "*") { gameScore += 250; }        // BEST EDU LOGO     - Extra points
        else if (letter == "#") { gameTime += 1000; }   // TIMER             - Extra time
        else if (letter == "$") { gameLives += 1; }    // HEART             - Extra lives
    }

    // Called by a Letter's GameObject on click
    public void ClickedOnLetter(GameObject caller, string letter)
    {
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
        }
        else
        {
            if (caller.GetComponent<Letter>().letter == "$" || caller.GetComponent<Letter>().letter == "#" || caller.GetComponent<Letter>().letter == "*")
                BonusClicked(caller.GetComponent<Letter>().letter);                                                                             // Player clicked on the bonus!       
            else 
                WrongLetterClicked();                                                                                                           // Wrong letter is clicked

            Destroy(caller);
        }
    }
}
