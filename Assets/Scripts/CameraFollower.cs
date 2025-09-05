using UnityEngine;

public class CameraFollower : MonoBehaviour {

    [SerializeField] Transform player;
    [SerializeField] float offset = 1f;
    private float greatestX;         

    void Start() {
        if (player == null)
            Debug.LogError("Player not assigned in FollowPlayerForward!");
        
        greatestX = player.position.x;
    }

    // Camera follows the player only when they move forward
    void Update() {

        float playerX = player.position.x;

        if (playerX > greatestX) {
            transform.position = new Vector3
                (playerX + offset, transform.position.y, transform.position.z);
            greatestX = playerX;
        }
    }
}