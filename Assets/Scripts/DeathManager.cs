using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;
using System.Collections;

public class DeathManager : NetworkBehaviour {

    [SerializeField] private HUDManager HUDManager;
    [SerializeField] private TextMeshProUGUI playerBlueTime;
    [SerializeField] private TextMeshProUGUI playerBlueScore;
    [SerializeField] private TextMeshProUGUI playerRedTime;
    [SerializeField] private TextMeshProUGUI playerRedScore;
    private NetworkVariable<int> replayingPlayers = new NetworkVariable<int>(0);

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

        AudioManager.instance.PlayButtonPressSFX();

        if (IsClient) {

            //NetworkManager.SceneManager.LoadScene("DuelScene", LoadSceneMode.Single);
            //SceneManager.LoadScene("DuelScene");
            IncreaseReplayingPlayersServerRpc();
            return;
        }

        SceneManager.LoadScene("SingleScene");
    }

    [ServerRpc(RequireOwnership = false)]
    void IncreaseReplayingPlayersServerRpc() {
        /*replayingPlayers.Value++;
        if (replayingPlayers.Value >= 2) {
            NetworkManager.SceneManager.LoadScene("DuelScene", LoadSceneMode.Single);
        }*/
        LoadDuelSceneClientRpc();
    }
    [ClientRpc]
    void LoadDuelSceneClientRpc() {
        /*if (!IsHost) {
            TempServerRpc();
        }
        SceneManager.LoadScene("MenuScene");
        SceneManager.LoadScene("DuelScene");
        */
        StartCoroutine(Temp());
    }
    [ServerRpc(RequireOwnership = false)]
    void TempServerRpc() {
        NetworkManager.Singleton.Shutdown();
    }

    IEnumerator Temp() {
        NetworkManager.Singleton.Shutdown();
        yield return new WaitForSecondsRealtime(5f);        
        //SceneManager.LoadScene("DuelScene");
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
