using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;
using TMPro;

public class TextManager : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private Player player;
    private List<string> wordBank;
    private List<char> typedText = new List<char>();
    private List<char> currentText = new List<char>();

    void Start() {

        LoadWordBank();

        // generate 10 words and add the characters to the currentText list
        for (int i = 0; i < 10; i++) {
            string wordToAdd = wordBank[Random.Range(0, wordBank.Count)] + " ";
            foreach (char c in wordToAdd) {
                currentText.Add(c);
            }
        }

        UpdateTextField();
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

    // joins the grey typed text and white current text around a vertical bar
    void UpdateTextField() {
        textField.text = "<color=#808080>" + string.Join("", typedText) + "</color>" +
            "|" + string.Join("", currentText);
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
                }
            }
        }
    }

    // proccess any key presses, progress if right, regress if wrong
    void ProcessKeyPress(string keyPress) {
        if (keyPress == "backspace") {
            currentText.Insert(0, typedText[^1]);
            typedText.RemoveAt(typedText.Count - 1);
            UpdateTextField();
            player.MoveBackward();
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
                UpdateTextField();
                player.MoveForward();
            }
        }
    }
}
