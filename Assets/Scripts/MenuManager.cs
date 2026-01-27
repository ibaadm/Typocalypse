using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour {
    
    [SerializeField] private GameObject helpText;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject duelMenu;
    [SerializeField] private TextMeshProUGUI createCode;
    [SerializeField] private GameObject joinCode;
    [SerializeField] private TMP_InputField joinCodeText;
    [SerializeField] private GameObject joinSymbol;
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
        if (showHelpText) {
            Help();
        }
        Join();
        mainMenu.SetActive(false);
        duelMenu.SetActive(true);

    }

    public void Create() {
        AudioManager.instance.PlayButtonPressSFX();
        joinSymbol.SetActive(false);
        joinCode.SetActive(false);
        createCode.text = LobbyManager.instance.GetLobbyCode();
    }

    public void Join() {
        AudioManager.instance.PlayButtonPressSFX();
        joinSymbol.SetActive(true);
        joinCode.SetActive(true);
        createCode.text = "";
    }

    public async void JoinWithCode() {
        AudioManager.instance.PlayButtonPressSFX();
        try {
            await LobbyManager.instance.JoinLobby(joinCodeText.text);
            joinCodeText.text = "...";
        }
        catch (System.Exception e) {
            joinCodeText.text = "ERROR";
            Debug.Log("Join error: " + e.Message);
        }
    }

    public void Back() {
        AudioManager.instance.PlayButtonPressSFX();
        mainMenu.SetActive(true);
        duelMenu.SetActive(false);
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
