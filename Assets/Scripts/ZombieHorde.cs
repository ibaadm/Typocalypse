using UnityEngine;

public class ZombieHorde : MonoBehaviour {

    [SerializeField] private Transform player;
    [SerializeField] private ZombieHordeAnimation[] zombies;

    [SerializeField] private float maxDistance = 2f;
    [SerializeField] private float moveCooldown = 0.5f;
    [SerializeField] private float moveDistance = 0.2f;
    private float moveTimer;

    void Start() {

        moveTimer = moveCooldown;
        // Spawn the zombies in the right place
        transform.position = new Vector2(player.position.x - maxDistance, player.position.y);
    }

    void Update() {
        
        TeleportCloser();
        MoveTowardsPlayer();

        moveTimer -= Time.deltaTime;
    }

    // When the zombies leave the camera, teleport them to the edge of the camera
    void TeleportCloser(){

        if (Vector2.Distance(transform.position, player.position) > maxDistance + 0.01f){
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
