using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;
using TMPro;

public class TextManager : MonoBehaviour {

    [SerializeField] TextMeshProUGUI textField;
    private List<string> wordBank;
    private List<char> typedText = new List<char>();
    private List<char> currentText = new List<char>();
    private List<char> keyPressQueue = new List<char>();

    void Start() {
        // Load words.txt from Resources
        TextAsset wordFile = Resources.Load<TextAsset>("words");
        
        // Split words by lines and store in wordBank list if file found
        if (wordFile != null) {
            wordBank = new List<string>(wordFile.text.Split('\n'));
        }
        else {
            Debug.LogError("word file not found");
        }

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
        // Check for backspace first
        if (Keyboard.current.backspaceKey.wasPressedThisFrame) {
            //handle backspace
        }

        // Handle space separately
        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            keyPressQueue.Add(' ');
        }

        // check if any key was pressed, have to handle caps and non-letters
        foreach (KeyControl key in Keyboard.current.allKeys) {
            if (key != null && key.wasPressedThisFrame) {
                keyPressQueue.Add(key.displayName.ToLower()[0]);
            }
        }

        // proccess any key presses, progress if right, nothing if wrong
        if (keyPressQueue.Count > 0) {
            if (keyPressQueue[0] == currentText[0]) {
                keyPressQueue.RemoveAt(0);
                typedText.Add(currentText[0]);
                currentText.RemoveAt(0);
                UpdateTextField();
            }
            else {
                keyPressQueue.RemoveAt(0);
            }
        }
    }

    // helper function to update the text field
    void UpdateTextField() {
        textField.text = string.Join("", typedText) + "|" + string.Join("", currentText);
    }
}
