using UnityEngine;
using Unity.Netcode;

public class CameraFollower : NetworkBehaviour {

    Transform playerBlue;
    Transform playerRed;
    [SerializeField] float offset = 1f;
    private float greatestX;         

    void Start() {

        playerBlue = GameObject.FindWithTag("Player Blue").transform;
    }

    // If dueling, follow the player that is behind
    public override void OnNetworkSpawn() {
        playerRed = GameObject.FindWithTag("Player Red").transform;
    }

    // Camera follows the player
    void Update() {

        if (playerRed != null && playerRed.position.x < playerBlue.position.x) {
            transform.position = new Vector3
                (playerRed.position.x + offset, transform.position.y, transform.position.z);
        }
        else {
            transform.position = new Vector3
                (playerBlue.position.x + offset, transform.position.y, transform.position.z);
        }
    }
}