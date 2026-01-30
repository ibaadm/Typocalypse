using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;
using TMPro;

public class TextManager : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI textField;
    public Player player;
    [SerializeField] private int lineMaxCharacterCount = 20;
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private AudioManager audioManager;
    [HideInInspector] public HUDManager HUDManager;
    private List<string> wordBank;
    private List<char> typedText = new List<char>();
    private List<char> currentText = new List<char>();
    private string nextWord;
    private string topLine;
    private string currentLine;
    private string bottomLine;

    void Start() {

        LoadWordBank();
        InitializeText();
    }

    void Update() {

        CheckForKeyPresses();
    }

    void LoadWordBank() {
        TextAsset wordFile = Resources.Load<TextAsset>("words");

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

        foreach (char c in topLine + '\n' + currentLine + '\n' + bottomLine) {
            currentText.Add(c);
        }
        UpdateTextField();
    }

    void UpdateTextField() {
        textField.text = "<color=#808080>" + string.Join("", typedText) + "</color>" +
            "|" + string.Join("", currentText);
    }

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

    void CheckForKeyPresses() { if (player.isDead || !menuManager.hasGameStarted) { return; }
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

        foreach (KeyControl key in Keyboard.current.allKeys) {
            if (key != null && key.displayName.Length == 1 && key.wasPressedThisFrame) {
                if (Keyboard.current.shiftKey.isPressed) {
                    ProcessKeyPress(key.displayName.ToUpper());
                }
                else {
                    ProcessKeyPress(key.displayName.ToLower());
                }
            }
        }
    }

    void ProcessKeyPress(string keyPress) {

        if (keyPress == "backspace") {
            /*if (typedText.Count <= 0) { return; }
            currentText.Insert(0, typedText[^1]);
            typedText.RemoveAt(typedText.Count - 1);
            if (currentText[0] == '\n') {
                // At the beginning of a line, move back twice to ignore the \n character
                currentText.Insert(0, typedText[^1]);
                typedText.RemoveAt(typedText.Count - 1);
            }
            player.MoveBackward();
            UpdateTextField();*/
        }

        else if (keyPress == "up") {
            player.MoveUp();
        }

        else if (keyPress == "down") {
            player.MoveDown();
        }

        else {
            // Move the first character from currentText to typedText
            if (keyPress[0] == currentText[0]) {
                typedText.Add(currentText[0]);
                currentText.RemoveAt(0);

                // If the middle line is complete, move the text up
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
                // If the top line is complete, skip the \n character and move to the middle line
                else if (currentText[0] == '\n') {
                    typedText.Add(currentText[0]);
                    currentText.RemoveAt(0);
                }
                player.MoveForward();
                HUDManager.UpdateScore();
                audioManager.PlayKeyboardSFX();
                UpdateTextField();
            }
        }
    }
}
