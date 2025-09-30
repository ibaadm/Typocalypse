using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using TMPro;

public class DuelManager : NetworkBehaviour {

    [SerializeField] TextMeshProUGUI blueReadyText;
    [SerializeField] TextMeshProUGUI redReadyText;
    private NetworkVariable<int> playersReady = new NetworkVariable<int>(0);
    private bool ready = false;


    void Start() {
        if (LobbyManager.instance.shouldBeHost == true) {
            NetworkManager.Singleton.StartHost();
        }
        else {
            NetworkManager.Singleton.StartClient();
        }
    }

    public override void OnNetworkSpawn() {
        if (IsHost) {
            Debug.Log("Host");
        }
        else {
            Debug.Log("Client");
        }
        StartCoroutine(PlayerReadyUp());
    }

    IEnumerator PlayerReadyUp() {

        while (playersReady.Value < 2) {
            if (Keyboard.current.spaceKey.wasPressedThisFrame && !ready) {
                ready = true;
                IncreasePlayersReadyServerRpc();
                if (IsHost) {
                    blueReadyText.text = "Ready";
                }
                else {
                    redReadyText.text = "Ready";
                }
            }
            if (playersReady.Value == 1 && !ready) {
                if (IsHost) {
                    redReadyText.text = "Ready";
                }
                else {
                    blueReadyText.text = "Ready";
                }
            }
            yield return null;
        }

        FindAnyObjectByType<MenuManager>().hasGameStarted = true;
    }

    [ServerRpc (RequireOwnership = false)]
    void IncreasePlayersReadyServerRpc() {
        playersReady.Value++;
    }
}
