using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour {
    
    public bool hasGameStarted = false;
    private GameObject HUDPanel;

    void Awake() {

        Time.timeScale = 0;
        HUDPanel = GameObject.FindWithTag("HUD");
        HUDPanel.SetActive(false);
        GameObject.FindWithTag("Death").SetActive(false);
        AudioManager.instance.EnableLowPassFilterCutoff();
    }

    void Update() {

        if (hasGameStarted) {
            Time.timeScale = 1;
            HUDPanel.SetActive(true);
            AudioManager.instance.gameplay = true;
            gameObject.SetActive(false);
        }
    }

    public void StartGame() {

        AudioManager.instance.PlayButtonPressSFX();
        hasGameStarted = true;
    }

    public void Duel() {
        
        AudioManager.instance.PlayButtonPressSFX();
    }

    public void Help() {
        
        AudioManager.instance.PlayButtonPressSFX();
    }

    public void Quit() {
        
        AudioManager.instance.PlayButtonPressSFX();
        Application.Quit();
    }
}
