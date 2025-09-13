using Unity.Netcode;
using UnityEngine;

public class DuelManager : NetworkBehaviour {


    [SerializeField] private GameObject playerRedPrefab;

    public override void OnNetworkSpawn() {
        if (IsHost){
            Debug.Log("Host");
        }
        else {
            Debug.Log("Client");
        }
    }

}
