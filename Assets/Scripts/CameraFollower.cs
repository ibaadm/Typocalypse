using UnityEngine;
using Unity.Netcode;

public class CameraFollower : NetworkBehaviour {

    Transform playerBlue;
    Transform playerRed;
    [SerializeField] float offset = 1f;
    private float greatestX;
    private bool stopCamera = false;

    void Start() {

        playerBlue = GameObject.FindWithTag("Player Blue").transform;
    }

    // If dueling, follow the player that is behind
    public override void OnNetworkSpawn() {
        playerRed = GameObject.FindWithTag("Player Red").transform;
    }

    // Camera follows the player
    void Update() { if (stopCamera) { return; }

        if (playerRed != null && playerRed.position.x < playerBlue.position.x) {
            //transform.position = new Vector3
            //    (playerRed.position.x + offset, transform.position.y, transform.position.z);
            Vector3 targetPos = new Vector3(playerRed.position.x + offset, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 15f * Time.deltaTime);
        }
        else {
            //transform.position = new Vector3
            //    (playerBlue.position.x + offset, transform.position.y, transform.position.z);
            Vector3 targetPos = new Vector3(playerBlue.position.x + offset, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 15f * Time.deltaTime);
        }
    }

    public void StopCamera() { stopCamera = true; }
}