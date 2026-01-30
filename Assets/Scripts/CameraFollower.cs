using UnityEngine;
using Unity.Netcode;

public class CameraFollower : NetworkBehaviour {

    Transform playerBlue;
    Transform playerRed;
    [SerializeField] float offset = 1f;
    private bool stopCamera = false;

    void Start() {

        playerBlue = GameObject.FindWithTag("Player Blue").transform;
    }

    public override void OnNetworkSpawn() {
        playerRed = GameObject.FindWithTag("Player Red").transform;
    }

    void Update() { if (stopCamera) { return; }

        if (playerRed != null && playerRed.position.x < playerBlue.position.x) {
            Vector3 targetPos = new Vector3(playerRed.position.x + offset, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 15f * Time.deltaTime);
        }
        else {
            Vector3 targetPos = new Vector3(playerBlue.position.x + offset, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 15f * Time.deltaTime);
        }
    }

    public void StopCamera() { stopCamera = true; }
}