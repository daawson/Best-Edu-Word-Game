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
    float gameTime = 10;                                        // Time remaining in seconds.
    float gameMaxTime = 10;                                     // Max seconds per word.
    int gameScore = 0;                                          // Total score.
    int gameLives = 3;                                          // Player lives

    // Difficulty variables / more time on long words
    float basicTime = 10;
    float difficultyHardMultiplier = 2f;
    float difficultyMediumMultiplier = 1.5f;

    // Words variables
    public TextAsset wordList;                                  // Text file holding all the words.
    public string currentWord;                                  // Currently chosen random word.
    public string previewCurrentWord;                           // Holds already clicked letters
    int clickedLetterCount = 0;                                 // Helper var to count clicks.

    string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";             // To generate filler letters.

    // Chance Modifiers for bonuses.
    float pointsChanceModifier = 0.3f;
    float timersChanceModifier = 0.05f;
    float heartsChanceModifier = 0.5f;

    public List<string> wordsData = new List<string>();        // RAW WORDS
    public List<string> availableWords = new List<string>();   // SORTED FOR WORDS >= 4 LETTERS ~ 870 WORDS.
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
    [SerializeField]
    private TMP_Text _livesText;


    // Game objects
    Camera cameraMain;                                  // Camera object
    public float cameraHeight, cameraWidth;             
    public float edgeOffset = 4f;                       // Offset from the game window edge.

    [SerializeField]
    private GameObject letterPrefab;                    // Letter GameObject prefab

    GameObject lettersHolder;                           // Empty GameObject that holds all the letters


    // Pause menu variables
    bool isPaused = false;
    [SerializeField]
    private GameObject gameUI;                          // Game UI's canvas
    [SerializeField]
    private GameObject pauseMenu;                       // Pause canvas


    // End menu variables
    bool hasEndedTheGame = false;
    [SerializeField]
    private GameObject endMenu;
    [SerializeField]
    private TMP_Text endMenuScore;
    [SerializeField]
    private TMP_Text endMenuStatistics;
    
    int completedWords, failedWords, extraPointsGained, extraLivesGained, extraTimeGained;   // End result extra data

    void Start()
    {
        // Make instance, so that GameManager will be accesible outside this class and to not accidentaly make another instance of GameManager.
        // Accesing GameManager only by GamaManager gm = GamaManager.instance;
        if (instance == null) instance = this;

        // Setting up camera variables.
        cameraMain = Camera.main;
        cameraHeight = 2f * cameraMain.orthographicSize;
        cameraWidth = cameraHeight * cameraMain.aspect;

        // Format text file to array.
        LoadWords();

        // Find gameObject to hold all the letters.
        lettersHolder = GameObject.Find("Letters");

        // Start the game with a random word with length of 4 letters.
        string startWord = availableWords[Random.Range(0, availableWords.Count)];
        while(startWord.Length != 4)
        {
            startWord = availableWords[Random.Range(0, availableWords.Count)];
        }
        SetupWord(startWord);
    }
    void Update()
    {
        if (!isPaused)
        {
            gameTime -= Time.deltaTime;                             //Time remaining
                                                                    // No time?
            if (gameTime < 0)
            {
                // Subtract lives, and check in to next round.
                gameLives--;
                LostTheRound();
            }

            // Update the time slider
            _timeSlider.value = gameTime / gameMaxTime;
            // Update the score
            _scoreText.text = "Score: " + gameScore;
            // Update the lives text
            _livesText.text = "x" + gameLives;
        }


        // Pause/UnPause if player pressed escape. 
        // Also check if player doesn't see statistics screen at this moment
        if (Input.GetKeyDown(KeyCode.Escape) && !hasEndedTheGame) SwitchPauseState();
    }

    #region #Loading words, setting up the game
    void LoadWords()
    {
        // Load and prepare words array
        string words_preformat = wordList.text;
        
        // Formating the words, and adding it to Words List
        string[] words = words_preformat.Split('\n');
        foreach (var word in words)
        {
            string a = Regex.Replace(word, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);

            // If it has 4 or more letters add to Words List
            if (a.Length >= 4)
            {
                wordsData.Add(a);
            }
        }
        availableWords = new List<string>(wordsData);
    }

    // Invokes the generation of the new word
    void SetupWord(string word) 
    {
        clickedLetterCount = 0;                         // Resets clicked letters count
        if(lettersHolder.transform.childCount > 0)      // Removes all letters from the screen if there are any left.
        {
            foreach (Transform child in lettersHolder.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        currentWord = word.ToUpper();                   // Uppercasing the letters
        _wordText.text = currentWord;                   // Updating info text

        // Updating preview of clicked letters " _ _ _ _ _ _ " ... etc. 
        string a = "";
        for (int i = 0; i < currentWord.Length; i++) a += "_";
        previewCurrentWord = a;
        _previewWordText.text = a;


        // Dificulty checkup, the longer the word the more time you get.
        if(currentWord.Length < 6) 
        { 
            gameTime = basicTime;
            gameMaxTime = basicTime;
        }
        else if(currentWord.Length >= 6)
        {
            gameTime = basicTime * difficultyMediumMultiplier;           
            gameMaxTime = basicTime * difficultyMediumMultiplier;
        }
        else if(currentWord.Length >= 8)
        {
            gameTime = basicTime * difficultyHardMultiplier;
            gameMaxTime = basicTime * difficultyHardMultiplier;
        }

        GenerateLetters(currentWord);                   // Starts the letter generations.
    }

    // Generates letters on the screen
    void GenerateLetters(string word)
    {
        int lettersOnScreen = 0;
        string availableAlphabet = alphabet;            // SEtup alphabet to later remove letters used in the choosen by game word.

        // Generate initial word letter's
        for (int i = 0; i < word.Length; i++)
        {
            GameObject l = Instantiate(letterPrefab, lettersHolder.transform);              // Initiate the letters
            l.transform.position = new Vector3(Random.Range(-cameraWidth / 2, cameraWidth / 2), Random.Range(-cameraHeight / 2, cameraHeight / 2), 0);  // Positon the letter
            l.GetComponent<Letter>().SetupLetter(word[i].ToString().ToUpper(), true);       // Setup the letter
            lettersOnScreen++;                                                              // Count the letters on the screen
            availableAlphabet = availableAlphabet.Replace(""+word[i], string.Empty);        // Remove this letter from available letters
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
        if (chance > pointsChanceModifier)
        {
            GameObject l = Instantiate(letterPrefab, lettersHolder.transform);
            l.transform.position = new Vector3(Random.Range(-cameraWidth / 2, cameraWidth / 2), Random.Range(-cameraHeight / 2, cameraHeight / 2), 0);
            l.GetComponent<Letter>().SetupLetter("*", false);
        }

        //Spawn a timer
        chance = Random.Range(0f, 1f);
        if (chance > timersChanceModifier)
        {
            GameObject l = Instantiate(letterPrefab, lettersHolder.transform);
            l.transform.position = new Vector3(Random.Range(-cameraWidth / 2, cameraWidth / 2), Random.Range(-cameraHeight / 2, cameraHeight / 2), 0);
            l.GetComponent<Letter>().SetupLetter("#", false);
        }

        //Spawn a heart
        chance = Random.Range(0f, 1f);
        if (chance > heartsChanceModifier && gameLives != 3)
        {
            GameObject l = Instantiate(letterPrefab, lettersHolder.transform);
            l.transform.position = new Vector3(Random.Range(-cameraWidth / 2, cameraWidth / 2), Random.Range(-cameraHeight / 2, cameraHeight / 2), 0);
            l.GetComponent<Letter>().SetupLetter("$", false);
        }
    }
	#endregion

	#region #Rounds Management Behaviours
	void WonTheRound()
    {
        // Update the arrays of available words, adds word to an array that holds all completed words. Next, removes it from the available pool.
        usedWords.Add(currentWord.ToLower());
        int index = availableWords.IndexOf(currentWord.ToLower());
        availableWords.RemoveAt(index);

        completedWords++;                                   // End game statistic, completed words++

        gameScore += (int)(1000 + (gameTime/2));            // Gives the player score, currently based on static 1000 and time left/2.
        //gameTime = 10;                                      // Resets the word timer.

        // Randomize next word, and setup it on the screen
        string nextWord = availableWords[Random.Range(0, availableWords.Count)];
        SetupWord(nextWord);
    }

    void LostTheRound()
    {
        // If player has even or more than 0 lives.
        if (gameLives >= 0)
        {
            // Update the arrays of available words, adds word to an array that holds all completed words. Next, removes it from the available pool.
            usedWords.Add(currentWord.ToLower());
            int index = availableWords.IndexOf(currentWord.ToLower());
            availableWords.RemoveAt(index);

            failedWords++;                           // End game statistic, failed words++
            //gameTime = 10;                           // Resets the word timer.

            // Randomize next word, and setup it on the screen
            string nextWord = availableWords[Random.Range(0, availableWords.Count)];
            SetupWord(nextWord);
        }
        else
        {

            failedWords++;                          // End game statistic, failed words++
            usedWords.Add(currentWord.ToLower());
            LostTheGame();
        }
    }

    void LostTheGame()
    {
        hasEndedTheGame = true;
        ShowEndScreen();
    }
    #endregion

    #region #On Letters Click
    void WrongLetterClicked()
    {
        gameLives--;
        // If player has less than 0 lives.
        if (gameLives < 0)
        {
            // You lost :(
            LostTheRound();
        }
    }

    void BonusClicked(string letter)
    {
        if (letter == "*") { gameScore += 250; extraPointsGained += 250; }        // When clicked on Best Edu Logo, give the player extra points
        else if (letter == "#") { gameTime += 2; extraTimeGained += 2; }   // When clicked on a timer, give the player 2 seconds extra time.
        else if (letter == "$") { gameLives += 1; extraLivesGained += 1; }     // When clicked on a heart, give the player extra lives.
    }

    // Called by a Letter's GameObject on click
    public void ClickedOnLetter(GameObject caller, string letter)
    {
        // If the letter clicked is one of the needed letters -- never remove it if its in the wrong order, keep it on the screen
        if(caller.GetComponent<Letter>().isNeeded)
        {
            // If its right letter
            if (currentWord[clickedLetterCount] == letter[0])
            {
                previewCurrentWord = previewCurrentWord.Remove(clickedLetterCount, 1).Insert(clickedLetterCount, letter[0].ToString());         // Update preview text
                _previewWordText.text = previewCurrentWord;
                Destroy(caller);                                                                                                                // Destroy clicked letter

                gameScore += 2;

                if (currentWord.Equals(previewCurrentWord)) WonTheRound();                                                                      // Win the round if word is complete
                else clickedLetterCount++;  // keep going with next letters
            }
            else WrongLetterClicked();
        }
        // Letters clicked is one of the randomized letters.
        else
        {
            // Bonus?
            if (caller.GetComponent<Letter>().letter == "$" || caller.GetComponent<Letter>().letter == "#" || caller.GetComponent<Letter>().letter == "*")
                BonusClicked(caller.GetComponent<Letter>().letter);                                                                             // Player clicked on the bonus!       
            else 
                WrongLetterClicked();                                                                                                           // Wrong letter is clicked

            Destroy(caller); // Destroy the letter.
        }
    }

    #endregion

    #region #Pause Behaviours

    // Switches between paused and unpaused
    public void SwitchPauseState()
    {
        if (!isPaused)
        {
            isPaused = true;
            gameUI.SetActive(false);                                // Disable inGame UI's
            lettersHolder.SetActive(false);                         // Hide letters so player cant see them when paused.
            pauseMenu.SetActive(true);                              // Show the pause menu.
        }
        else
        {
            isPaused = false;
            pauseMenu.SetActive(false);
            gameUI.SetActive(true);
            lettersHolder.SetActive(true);
        }

    }


    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion

    #region #Endgame Behaviours

    // Show the player the end game screen
    public void ShowEndScreen()
    {
        // Pause and disable all UI's
        isPaused = true;
        gameUI.SetActive(false);
        lettersHolder.SetActive(false);
        pauseMenu.SetActive(false);

        // Format the end game statistics.
        endMenuScore.text = "Your score was: " + gameScore + "!";
        string endStatisticsFormat = "Completed words:\t\t\t" + completedWords + "\n" +
            "Failed words:\t\t\t" + failedWords + "\n" +
            "Extra points gained:\t\t" + extraPointsGained + "\n" +
            "Extra time gained:\t\t" + extraTimeGained + "s\n" +
            "Extra lives gained:\t\t" + extraLivesGained + "x\n" +
            "Words:\n";

        // If player got at least one word, add the words to the end of the string
        if (usedWords.Count > 0)
        {
            // Loop the used words and add to the string
            foreach (string word in usedWords)
            {
                endStatisticsFormat += word + ", ";
            }
        }
        endMenuStatistics.text = endStatisticsFormat;

        // Show end game stats;
        endMenu.SetActive(true);
    }

    public void RestartTheGame()
    {
        // Reversing ending check
        hasEndedTheGame = false;

        // Restart the words array
        availableWords = new List<string>(wordsData);

        // Clear used words array
        usedWords.Clear();

        // Hide End Game UI's
        endMenu.SetActive(false);
        // Show UI's and unpause the game.
        isPaused = false;
        gameUI.SetActive(true);
        lettersHolder.SetActive(true);
        pauseMenu.SetActive(false);

        // Restart the statistics
        completedWords = 0;
        failedWords = 0;
        extraLivesGained = 0;
        extraPointsGained = 0;
        extraTimeGained = 0;
        
        // Restart the timer, health and score
        gameTime = 10;
        gameLives = 3;
        gameScore = 0;

        // Setup the screen
        SetupWord("best");
    }

    #endregion
}
