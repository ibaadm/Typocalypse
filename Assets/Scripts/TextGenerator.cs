using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class TextGenerator : MonoBehaviour {

    [SerializeField] TextMeshProUGUI textField;
    private List<string> wordBank;

    void Start() {
        // Load words.txt from Resources
        TextAsset wordFile = Resources.Load<TextAsset>("words");
        
        if (wordFile != null) {
            // Split words by lines and store in list
            wordBank = new List<string>(wordFile.text.Split('\n'));
        }
        else {
            Debug.LogError("word file not found");
        }

        textField.text = "";
        for (int i = 0; i < 10; i++) {
            textField.text += wordBank[Random.Range(0, wordBank.Count)] + " ";
        }
    }

    void Update() {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            Debug.Log(wordBank[Random.Range(0, wordBank.Count)]);
        }
    }
}
