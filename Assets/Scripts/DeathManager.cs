using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;

public class DeathManager : NetworkBehaviour {

    [SerializeField] private HUDManager HUDManager;
    [SerializeField] private TextMeshProUGUI playerBlueTime;
    [SerializeField] private TextMeshProUGUI playerBlueScore;
    [SerializeField] private TextMeshProUGUI playerRedTime;
    [SerializeField] private TextMeshProUGUI playerRedScore;
    [SerializeField] private TextMeshProUGUI playerBlueTick;
    [SerializeField] private TextMeshProUGUI playerRedTick;
    [SerializeField] private Color playerBlueTicked;
    [SerializeField] private Color playerBlueUnticked;
    [SerializeField] private Color playerRedTicked;
    [SerializeField] private Color playerRedUnticked;
    private bool wasHost = false;
    private bool wasClient = false;


    private NetworkVariable<int> replayingPlayers = new NetworkVariable<int>(0);

    void Start() {
        playerBlueTime.text = playerBlueScore.text = playerRedScore.text = playerRedTime.text = "";
        playerBlueTick.text = playerRedTick.text = "";
    }

    public void OnPanelEnable() {
        if (HUDManager.timeText.text != "0:00") {
            if (IsHost) {
                DisplayDuelScoresClientRpc(HUDManager.timeText.text, HUDManager.score);
                wasHost = true;
            }
            else if (IsClient) {
                DisplayDuelScoresServerRpc(HUDManager.timeText.text, HUDManager.score);
            }
        }
        playerBlueTick.text = playerRedTick.text = "<sprite index=0 tint=1>";
        wasClient = true;
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
        if (wasClient) {
            if (!LobbyManager.instance.isRetrying) {
                LobbyManager.instance.isRetrying = true;
                if (wasHost) {
                    IncreasePlayersRetryingServerRpc(true);
                    ChangeTickLocal(true, true);
                }
                else {
                    IncreasePlayersRetryingServerRpc(false);
                    ChangeTickLocal(false, true);
                }
            }
            else {
                LobbyManager.instance.isRetrying = false;
                if (wasHost) {
                    DecreasePlayersRetryingServerRpc(true);
                    ChangeTickLocal(true, false);
                }
                else {
                    DecreasePlayersRetryingServerRpc(false);
                    ChangeTickLocal(false, false);
                }
            }
            return;
        }

        SceneManager.LoadScene("SingleScene");
    }
    [ServerRpc (RequireOwnership = false)]
    void IncreasePlayersRetryingServerRpc(bool blue) {
        replayingPlayers.Value++;
        
        if (replayingPlayers.Value >= 2) {
            LobbyManager.instance.DisconnectServerRpc();
        }

        if (blue) {
            ChangeTickClientRpc(true, true);
        }
        else {
            ChangeTickClientRpc(false, true);
        }
    }

    public void Menu() {

        if (wasClient) {
            if (wasHost) {
                DecreasePlayersRetryingServerRpc(true);
                ChangeTickLocal(true, false);
            }
            else {
                DecreasePlayersRetryingServerRpc(false);
                ChangeTickLocal(false, false);
            }
            LobbyManager.instance.DisconnectServerRpc();
            LobbyManager.instance.MenuCleanup();
            return;
        }

        AudioManager.instance.PlayButtonPressSFX();
        SceneManager.LoadScene("MenuScene");
    }

    [ServerRpc (RequireOwnership = false)]
    void DecreasePlayersRetryingServerRpc(bool blue) {
        replayingPlayers.Value--;
        if (blue) {
            ChangeTickClientRpc(true, false);
        }
        else {
            ChangeTickClientRpc(false, false);
        }
    }

    [ClientRpc]
    void ChangeTickClientRpc(bool blue, bool ticked) {
        ChangeTickLocal(blue, ticked);
    }

        void ChangeTickLocal(bool blue, bool ticked) {
        if (blue) {
            if (ticked) {
                playerBlueTick.color = playerBlueTicked;
            }
            else {
                playerBlueTick.color = playerBlueUnticked;
            }
        }
        else {
            if (ticked) {
                playerRedTick.color = playerRedTicked;
            }
            else {
                playerRedTick.color = playerRedUnticked;
            }
        }
    }
}
