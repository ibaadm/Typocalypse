using UnityEngine;
using System.Collections;

public class ZombieHorde : MonoBehaviour {

    private Transform playerBlue;
    private Transform playerRed;
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

        playerBlue = GameObject.FindWithTag("Player Blue").transform;
        playerRed = GameObject.FindWithTag("Player Red")?.transform;
        audioManager = AudioManager.instance;
        stopMoving = false;
        moveTimer = moveCooldown;

        transform.position = new Vector2(playerBlue.position.x - maxDistance, playerBlue.position.y);
        StartCoroutine(SpeedUpHorde());
    }

    IEnumerator SpeedUpHorde() {

        while (true) {
            yield return new WaitForSeconds(speedIncreaseInterval);
            moveCooldown = Mathf.Max(moveCooldownMin, moveCooldown * moveCooldownDecreaseMultiplier);
            moveDistance += moveDistanceIncrease;
        }
    }

    void Update() {
        
        TeleportCloser();
        MoveTowardsPlayer();
        ManageVolume();

        moveTimer -= Time.deltaTime;
    }

    void TeleportCloser(){

        float redX = playerRed != null ? playerRed.position.x : float.MaxValue;
        float closestPlayerX = Mathf.Min(playerBlue.position.x, redX);
        if (closestPlayerX - transform.position.x > maxDistance + 0.01f){
            transform.position = new Vector2
                (closestPlayerX - maxDistance, transform.position.y);
                moveTimer = moveCooldown;
                CycleZombieSprites();
        }
    }

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

    void ManageVolume() {

        float distance = playerBlue.position.x - transform.position.x;
        audioManager.groanVolume = Mathf.Clamp01(1.3f - (distance / maxDistance));
    }

    void OnTriggerEnter2D(Collider2D other) {

        if (playerRed != null) {
            return;
        }
        HandleCollision();
    }

    public void HandleCollision() {
        stopMoving = true;
        FindAnyObjectByType<HUDManager>().StopTime();  
    }
}
