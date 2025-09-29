using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class Player : NetworkBehaviour {

    private Player otherPlayer;
    [SerializeField] float maxDistanceBetweenPlayers = 2f;

    [SerializeField] private float moveSpeed = 0.1f;
    [SerializeField] private float horizontalMoveDistance = 0.5f;
    [SerializeField] private float verticalMoveDistance = 0.5f;
    private Vector2 targetPosition;
    private float maxHeight;
    private float minHeight;

    [SerializeField] private Sprite[] playerSprites = new Sprite[4];
    [SerializeField] private Sprite deathSprite;
    [SerializeField] private GameObject blood;
    [SerializeField] private GameObject deadBody;
    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool isEaten = false;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private int currentSpriteIndex = 0;

    [SerializeField] HUDManager HUDManager;
    [SerializeField] TextManager textManager;

    void Start() {

        targetPosition = transform.position;
        maxHeight = transform.position.y + verticalMoveDistance;
        minHeight = transform.position.y - verticalMoveDistance;
        
        blood.SetActive(false);
        isDead = false;

        StartCoroutine(MoveCoroutine());
    }

    // Disable the blue player on the host and disable the red player on the client
    public override void OnNetworkSpawn() {

        bool isBlue = gameObject.tag == "Player Blue";
        bool isRed = gameObject.tag == "Player Red";
        if (!IsHost && isBlue) {
            otherPlayer = GameObject.FindWithTag("Player Red").GetComponent<Player>();
            enabled = false;
        }
        if (IsHost && isRed) {
            maxHeight = transform.position.y + 2 * verticalMoveDistance;
            minHeight = transform.position.y;
            otherPlayer = GameObject.FindWithTag("Player Blue").GetComponent<Player>();
            enabled = false;
        }
        if (!IsHost && isRed) {
            textManager.player = this;
            HUDManager.player = this;
            RequestOwenershipServerRPC();
            maxHeight = transform.position.y + 2 * verticalMoveDistance;
            minHeight = transform.position.y;
            otherPlayer = GameObject.FindWithTag("Player Blue").GetComponent<Player>();
        }
        if (IsHost && isBlue) {
            otherPlayer = GameObject.FindWithTag("Player Red").GetComponent<Player>();
        }
    }

    // Moves the player to the target position, controlled by key presses
    IEnumerator MoveCoroutine() {

        while (true) {

            if (isEaten && otherPlayer != null && !otherPlayer.isDead) {
                transform.position = otherPlayer.transform.position;
            }

            else {
                transform.position = Vector2.MoveTowards
                    (transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }

            yield return null;            
        }
    }

    // Let the client take ownership of the red player
    [ServerRpc(RequireOwnership = false)]
    void RequestOwenershipServerRPC(ServerRpcParams rpcParams = default) {
        GetComponent<NetworkObject>().ChangeOwnership(rpcParams.Receive.SenderClientId);
    }

    public void MoveForward(bool rpc = false) {

        if (isDead || (IsClient &&
            transform.position.x - otherPlayer.transform.position.x > maxDistanceBetweenPlayers)) {
            return;
        }

        if (IsClient && !rpc) {
            MoveForwardServerRpc();
            return;
        }
        targetPosition += new Vector2(horizontalMoveDistance, 0f);
        spriteRenderer.flipX = false;
        CyclePlayerSprite();
    }

    public void MoveUp(bool rpc = false) {
        if (isDead) { return; }

        if (IsClient && !rpc) {
            MoveUpServerRpc();
            return;
        }

        if (Mathf.Abs(targetPosition.y - maxHeight) > 0.01f) {
            targetPosition += new Vector2(0f, verticalMoveDistance);
            CyclePlayerSprite();
        }
    }

    public void MoveDown(bool rpc = false) {
        if (isDead) { return; }

        if (IsClient && !rpc) {
            MoveDownServerRpc();
            return;
        }

        if (Mathf.Abs(targetPosition.y - minHeight) > 0.01f) {
            targetPosition -= new Vector2(0f, verticalMoveDistance);
            CyclePlayerSprite();
        }                                    
    }
    
    private void CyclePlayerSprite(bool rpc = false) { if (isDead) { return; }

        currentSpriteIndex++;
        if (currentSpriteIndex >= playerSprites.Length) {
            currentSpriteIndex = 0;
        }
        spriteRenderer.sprite = playerSprites[currentSpriteIndex];
    }

    // Run the function on all clients when on a network
    [ServerRpc(RequireOwnership = false)]
    void MoveForwardServerRpc() { MoveForwardClientRpc(); }
    [ClientRpc]
    void MoveForwardClientRpc() { MoveForward(true); }
    
    [ServerRpc(RequireOwnership = false)]
    void MoveUpServerRpc() { MoveUpClientRpc(); }
    [ClientRpc]
    void MoveUpClientRpc() { MoveUp(true); }

    [ServerRpc(RequireOwnership = false)]
    void MoveDownServerRpc() { MoveDownClientRpc(); }
    [ClientRpc]
    void MoveDownClientRpc() { MoveDown(true); }

    // When dead, fall down, splatter blood, and stop other functions with isDead

    void OnTriggerEnter2D(Collider2D other){
        if (IsClient){
            HandleDeathServerRpc(other.gameObject.tag);
            return;
        }
        if (!isDead){
            AudioManager.instance.PlayFallOverSFX();
            Vector3 deathPos = transform.position;
            Instantiate(deadBody, deathPos + new Vector3(0.05f, 0f, 0f), Quaternion.identity);
            spriteRenderer.enabled = false;
            FindAnyObjectByType<HUDManager>().DisableTypingText();
        }
        isDead = true;
        if (other.gameObject.CompareTag("Zombie Horde")) {
            GetComponent<BoxCollider2D>().enabled = false;
            blood.transform.SetParent(null);
            blood.SetActive(true);
            AudioManager.instance.PlayEatingSFX();
        }
    }

    // Handle death on a network by implementing changes on all clients
    [ServerRpc(RequireOwnership = false)]
    private void HandleDeathServerRpc(string tag) {
        HandleDeathClientRpc(tag);
    }
    [ClientRpc]
    private void HandleDeathClientRpc(string tag) {
        if (!isDead) {
            AudioManager.instance.PlayFallOverSFX();
            Vector3 deathPos = transform.position;
            Instantiate(deadBody, deathPos + new Vector3(0.05f, 0f, 0f), Quaternion.identity);
            spriteRenderer.enabled = false;
            if (IsHost && gameObject.tag == "Player Blue" || !IsHost && gameObject.tag == "Player Red") {
                FindAnyObjectByType<HUDManager>().DisableTypingText();
            }
            if (otherPlayer.isEaten) {
                FindAnyObjectByType<CameraFollower>().StopCamera();
            }
        }
        isDead = true;
        if (tag == "Zombie Horde") {
            isEaten = true;
            GetComponent<BoxCollider2D>().enabled = false;
            blood.transform.SetParent(null);
            blood.SetActive(true);
            AudioManager.instance.PlayEatingSFX();
            if (otherPlayer.isEaten) {
                FindAnyObjectByType<ZombieHorde>().HandleCollision();
            }
        }
    }

    public float GetHorizontalMoveDistance() { return horizontalMoveDistance; }
}
