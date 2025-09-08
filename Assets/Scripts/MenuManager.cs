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
    }

    void Update() {

        if (hasGameStarted) {
            Time.timeScale = 1;
            HUDPanel.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    public void StartGame() { hasGameStarted = true; }

    public void Duel() { Debug.Log("Duel"); }

    public void Help() { Debug.Log("Help"); }

    public void Quit() { Application.Quit(); }
}
