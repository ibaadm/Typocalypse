using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class Player : NetworkBehaviour {

    [SerializeField] private float moveSpeed = 0.1f;
    [SerializeField] private float horizontalMoveDistance = 0.5f;
    [SerializeField] private float verticalMoveDistance = 0.5f;
    private Vector2 targetPosition;
    private float maxHeight;
    private float minHeight;

    [SerializeField] private Sprite[] playerSprites = new Sprite[4];
    [SerializeField] private Sprite deathSprite;
    [SerializeField] private GameObject blood;
    [HideInInspector] public bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private int currentSpriteIndex = 0;

    void Start() {

        targetPosition = transform.position;
        maxHeight = transform.position.y + verticalMoveDistance;
        minHeight = transform.position.y - verticalMoveDistance;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        blood.SetActive(false);
        isDead = false;
    }

    // Disable the blue player on the host and disable the red player on the client
    public override void OnNetworkSpawn() {
        
        if (!IsHost && gameObject.tag == "Player Blue") {
            enabled = false;
        }
        if (!IsHost && gameObject.tag == "Player Red") {
            FindAnyObjectByType<TextManager>().player = this;
            RequestOwenershipServerRPC();
            maxHeight = transform.position.y + 2 * verticalMoveDistance;
            minHeight = transform.position.y;
        }
        if (IsHost && gameObject.tag == "Player Red") {
            enabled = false;
        }
    }

    // Let the client take ownership of the red player
    [ServerRpc(RequireOwnership = false)]
    void RequestOwenershipServerRPC(ServerRpcParams rpcParams = default) {
        GetComponent<NetworkObject>().ChangeOwnership(rpcParams.Receive.SenderClientId);
    }

    public void MoveForward() { if (isDead) { return; }

        targetPosition += new Vector2(horizontalMoveDistance, 0f);
        spriteRenderer.flipX = false;
        CyclePlayerSprite();
    }

    public void MoveBackward() { if (isDead) { return; }

        targetPosition -= new Vector2(horizontalMoveDistance, 0f);
        spriteRenderer.flipX = true;
        CyclePlayerSprite();
    }

    public void MoveUp() {
        
        if (Mathf.Abs(targetPosition.y - maxHeight) > 0.01f) {
            targetPosition += new Vector2(0f, verticalMoveDistance);
            CyclePlayerSprite();
        }
    }

    public void MoveDown() {

        if (Mathf.Abs(targetPosition.y - minHeight) > 0.01f) {
            targetPosition -= new Vector2(0f, verticalMoveDistance);
            CyclePlayerSprite();
        }
    }
    
    private void CyclePlayerSprite() { if (isDead) { return; }

        currentSpriteIndex++;
        if (currentSpriteIndex >= playerSprites.Length) {
            currentSpriteIndex = 0;
        }
        spriteRenderer.sprite = playerSprites[currentSpriteIndex];
    }

    // Player constantly moves towards the target position
    // Target position is updated with valid key presses
    void Update() { if (isDead) { return; }

        transform.position = Vector2.MoveTowards
            (transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    // When dead, fall down, splatter blood, and stop other functions with isDead
    void OnTriggerEnter2D(Collider2D other) {

        spriteRenderer.sprite = deathSprite;
        isDead = true;
        AudioManager.instance.PlayFallOverSFX();
        if (other.gameObject.CompareTag("Zombie Horde")) {
            blood.SetActive(true);
            AudioManager.instance.PlayEatingSFX();
        }
    }

    public float GetHorizontalMoveDistance() { return horizontalMoveDistance; }
}
