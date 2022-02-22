using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    // GAME VARIABLES
    int gameTime;
    int gameScore;

    List<string> wordsData = new List<string>();
    List<string> availableWords = new List<string>();
    List<string> usedWords = new List<string>();

    // UI GAMEOBJECTS
    [SerializeField]
    private TMP_Text _wordText;
    [SerializeField]
    private TMP_Text _previewWordText;

    void Start()
    {
        // Make instance, so that GameManager will be accesible outside this class.
        if (instance == null) instance = this;

        // Format text file to array.
        LoadWords();
    }
    void Update()
    {
        
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
        Debug.Log("Words Count: " + wordsData.Count);


        availableWords = wordsData;
    }
}
