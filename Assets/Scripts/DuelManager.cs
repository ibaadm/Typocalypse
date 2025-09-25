using Unity.Netcode;
using UnityEngine;

public class DuelManager : NetworkBehaviour {


    [SerializeField] private GameObject playerRedPrefab;

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
        FindAnyObjectByType<MenuManager>().hasGameStarted = true;
    }

}
