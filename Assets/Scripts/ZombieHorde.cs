using UnityEngine;
using System.Collections;

public class ZombieHorde : MonoBehaviour {

    [SerializeField] private Transform player;
    [SerializeField] private ZombieHordeAnimation[] zombies;
    private AudioManager audioManager;
    bool stopMoving;

    [Header("Base Movement")]
    [SerializeField] private float maxDistance = 2f;
    [SerializeField] private float moveCooldown = 0.5f;
    [SerializeField] private float moveDistance = 0.2f;
    private float moveTimer;

    [Header("Speed Increase")]
    [SerializeField] private float speedIncreaseInterval = 30f;
    [SerializeField] private float moveCooldownDecreaseMultiplier = 0.9f;
    [SerializeField] private float moveCooldownMin = 0.2f;
    [SerializeField] private float moveDistanceIncrease = 0.03f;

    void Start() {

        audioManager = GameObject.FindAnyObjectByType<AudioManager>();
        stopMoving = false;
        moveTimer = moveCooldown;
        // Spawn the zombies in the right place
        transform.position = new Vector2(player.position.x - maxDistance, player.position.y);

        StartCoroutine(SpeedUpHorde());
    }

    IEnumerator SpeedUpHorde() {

        while (true) {
            yield return new WaitForSeconds(speedIncreaseInterval);
            moveCooldown = Mathf.Max(moveCooldownMin, moveCooldown * moveCooldownDecreaseMultiplier);
            moveDistance += moveDistanceIncrease;
            //Debug.Log($"movecooldown: {moveCooldown}, moveDistance: {moveDistance}");
        }
    }

    void Update() {
        
        TeleportCloser();
        MoveTowardsPlayer();
        ManageVolume();

        moveTimer -= Time.deltaTime;
    }

    // When the zombies leave the camera, teleport them to the edge of the camera
    void TeleportCloser(){

        if (player.position.x - transform.position.x > maxDistance + 0.01f){
            transform.position = new Vector2
                (player.position.x - maxDistance, transform.position.y);
                moveTimer = moveCooldown;
                CycleZombieSprites();
        }
    }

    // Slowly advance the zombies
    void MoveTowardsPlayer() { if (stopMoving) { return; }

        if (moveTimer <= 0f){
            transform.position = new Vector2
                (transform.position.x + moveDistance, transform.position.y);
            moveTimer = moveCooldown;
            CycleZombieSprites();
        }
    }

    void CycleZombieSprites() {

        foreach (ZombieHordeAnimation zombie in zombies) {
            zombie.CycleZombieSprite();
        }
    }

    // Adjust the volume based on how close the zombies are to the player
    void ManageVolume() {

        float distance = (player.position.x - transform.position.x);
        audioManager.groanVolume = Mathf.Clamp01(1.3f - (distance / maxDistance));
    }

    void OnTriggerEnter2D(Collider2D other) {

        stopMoving = true;
        FindAnyObjectByType<HUDManager>().StopTime();
    }
}
