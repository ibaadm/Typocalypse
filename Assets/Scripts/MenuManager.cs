using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour {
    
    [SerializeField] private GameObject helpText;
    private bool showHelpText = false;
    public bool hasGameStarted = false;
    private GameObject HUDPanel;

    void Awake() {

        Time.timeScale = 0;
        helpText.SetActive(false);
        HUDPanel = GameObject.FindWithTag("HUD");
        HUDPanel.SetActive(false);
        GameObject.FindWithTag("Death").SetActive(false);
        AudioManager.instance.EnableLowPassFilterCutoff();
        if (hasGameStarted) {
            StartGame();
        }
    }

    public void StartGame() {

        if (!hasGameStarted) {
            AudioManager.instance.PlayButtonPressSFX();
        }
        hasGameStarted = true;
        Time.timeScale = 1;
        HUDPanel.SetActive(true);
        AudioManager.instance.gameplay = true;
        gameObject.SetActive(false);
    }

    public void Duel() {
        
        AudioManager.instance.PlayButtonPressSFX();
    }

    public void Help() {
        
        AudioManager.instance.PlayButtonPressSFX();
        if (showHelpText) {
            helpText.SetActive(false);
            showHelpText = false;
        }
        else {
            helpText.SetActive(true);
            showHelpText = true;
        }
    }

    public void Quit() {
        
        AudioManager.instance.PlayButtonPressSFX();
        Application.Quit();
    }
}
