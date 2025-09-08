using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class Player : MonoBehaviour {

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

    void Start(){

        targetPosition = transform.position;
        maxHeight = transform.position.y + verticalMoveDistance;
        minHeight = transform.position.y - verticalMoveDistance;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        blood.SetActive(false);
        isDead = false;
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

        if (targetPosition.y < maxHeight) {
            targetPosition += new Vector2(0f, verticalMoveDistance);
            CyclePlayerSprite();
        }
    }

    public void MoveDown() {

        if (targetPosition.y > minHeight) {
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
        if (other.gameObject.CompareTag("Zombie Horde")) {
            blood.SetActive(true);
        }
    }

    public float GetHorizontalMoveDistance() { return horizontalMoveDistance; }
}
