using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;
using TMPro;

public class TextManager : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private Player player;
    [SerializeField] private int lineMaxCharacterCount = 20;
    private List<string> wordBank;
    private List<char> typedText = new List<char>();
    private List<char> currentText = new List<char>();
    private string nextWord;
    private string topLine;
    private string currentLine;
    private string bottomLine;

    void Start()
    {

        LoadWordBank();
        InitializeText();
    }

    void Update() {

        CheckForKeyPresses();
    }

    void LoadWordBank() {
        // Load words.txt from Resources
        TextAsset wordFile = Resources.Load<TextAsset>("words");
        
        // Split words by lines and store in wordBank list if file found
        if (wordFile != null) {
            wordBank = new List<string>(wordFile.text.Split('\n'));
        }
        else {
            Debug.LogError("word file not found");
        }
    }


    void InitializeText() {
        nextWord = wordBank[Random.Range(0, wordBank.Count)] + " ";
        topLine = FillLine();
        currentLine = FillLine();
        bottomLine = FillLine();

        //add the characters of the text to currentText
        foreach (char c in topLine + '\n' + currentLine + '\n' + bottomLine)
        {
            currentText.Add(c);
        }
        UpdateTextField();
    }

    // joins the grey typed text and white current text around a vertical bar
    void UpdateTextField() {
        textField.text = "<color=#808080>" + string.Join("", typedText) + "</color>" +
            "|" + string.Join("", currentText);
        Debug.Log(currentText[0]);
    }

    // fills a line with words until the line is full
    string FillLine() {
        string line = "";
        while (true) {  
            if ((line + nextWord).Length <= lineMaxCharacterCount) {
                line += nextWord;
                nextWord = wordBank[Random.Range(0, wordBank.Count)] + " ";
            }
            else {
                return line;
            }
        }
    }

    void CheckForKeyPresses() {
        //handle backspace, space and arrow keys first
        if (Keyboard.current.backspaceKey.wasPressedThisFrame) {
            ProcessKeyPress("backspace");
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            ProcessKeyPress(" ");
        }
        if (Keyboard.current.upArrowKey.wasPressedThisFrame) {
            ProcessKeyPress("up");
        }
        if (Keyboard.current.downArrowKey.wasPressedThisFrame) {
            ProcessKeyPress("down");
        }

        // check if any printable key was pressed, case sensitive, caps lock ignored
        foreach (KeyControl key in Keyboard.current.allKeys) {
            if (key != null && key.displayName.Length == 1 && key.wasPressedThisFrame) {
                if (Keyboard.current.shiftKey.isPressed) {
                    ProcessKeyPress(key.displayName.ToUpper());
                }
                else {
                    ProcessKeyPress(key.displayName.ToLower());
                    Debug.Log(key.displayName.ToLower());
                }
            }
        }
    }

    // proccess any key presses, progress if right, regress if wrong HANDLE APOSTROPHE
    void ProcessKeyPress(string keyPress) {
        if (keyPress == "backspace" && typedText.Count > 0) {
            currentText.Insert(0, typedText[^1]);
            typedText.RemoveAt(typedText.Count - 1);
            if (currentText[0] == '\n') {
                currentText.Insert(0, typedText[^1]);
                typedText.RemoveAt(typedText.Count - 1);
            }
            player.MoveBackward();
            UpdateTextField();
        }
        else if (keyPress == "up") {
            player.MoveUp();
        }
        else if (keyPress == "down") {
            player.MoveDown();
        }
        else {
            if (keyPress[0] == currentText[0]) {
                typedText.Add(currentText[0]);
                currentText.RemoveAt(0);

                if (string.Join("", typedText) == topLine + '\n' + currentLine) {
                    topLine = currentLine;
                    currentLine = bottomLine;
                    bottomLine = FillLine();
                    currentText.Clear();
                    typedText.Clear();
                    foreach (char c in currentLine + '\n' + bottomLine) {
                        currentText.Add(c);
                    }
                    foreach (char c in topLine + '\n') {
                        typedText.Add(c);
                    }
                }
                else if (currentText[0] == '\n') {
                    typedText.Add(currentText[0]);
                    currentText.RemoveAt(0);
                }
                player.MoveForward();
                UpdateTextField();
            }
        }
    }
}
