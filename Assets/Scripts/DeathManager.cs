using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;

public class DeathManager : NetworkBehaviour {

    [SerializeField] private HUDManager HUDManager;
    [SerializeField] private TextMeshProUGUI playerBlueTime;
    [SerializeField] private TextMeshProUGUI playerBlueScore;
    [SerializeField] private TextMeshProUGUI playerRedTime;
    [SerializeField] private TextMeshProUGUI playerRedScore;

    void Start() {
        playerBlueTime.text = playerBlueScore.text = playerRedScore.text = playerRedTime.text = "";
    }

    public void OnPanelEnable() {
        if (HUDManager.timeText.text != "0:00") {
            if (IsHost) {
                DisplayDuelScoresClientRpc(HUDManager.timeText.text, HUDManager.score);
            }
            else if (IsClient) {
                DisplayDuelScoresServerRpc(HUDManager.timeText.text, HUDManager.score);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DisplayDuelScoresServerRpc(string redTime, int redScore) {

        DisplayPlayerStats(HUDManager.timeText.text, HUDManager.score, redTime, redScore);
    }
    [ClientRpc]
    void DisplayDuelScoresClientRpc(string blueTime, int blueScore) {
        if (IsHost) { return; }

        DisplayPlayerStats(blueTime, blueScore, HUDManager.timeText.text, HUDManager.score);
    }
    void DisplayPlayerStats(string blueTime, int blueScore, string redTime, int redScore) {
        playerBlueTime.text = $"Time {blueTime} |";
        playerBlueScore.text = " Score " + blueScore.ToString();
        playerRedTime.text = $"Time {redTime} |";
        playerRedScore.text = " Score " + redScore.ToString();
    }

    public void Replay() {

        if (IsClient) {

            return;
        }

        AudioManager.instance.PlayButtonPressSFX();
        SceneManager.LoadScene("SingleScene");
    }

    public void Menu() {

        if (IsClient) {
            LobbyManager.instance.DisconnectServerRpc();
            return;
        }

        AudioManager.instance.PlayButtonPressSFX();
        SceneManager.LoadScene("MenuScene");
    }
}
