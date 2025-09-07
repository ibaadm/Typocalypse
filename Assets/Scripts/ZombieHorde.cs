using UnityEngine;
using System.Collections;

public class ZombieHorde : MonoBehaviour {

    [SerializeField] private Transform player;
    [SerializeField] private ZombieHordeAnimation[] zombies;

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
    void MoveTowardsPlayer() {

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
}
