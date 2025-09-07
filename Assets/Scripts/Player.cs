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
    private SpriteRenderer spriteRenderer;
    private int currentSpriteIndex = 0;

    void Start(){

        targetPosition = transform.position;
        maxHeight = transform.position.y + verticalMoveDistance;
        minHeight = transform.position.y - verticalMoveDistance;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void MoveForward() {

        targetPosition += new Vector2(horizontalMoveDistance, 0f);
        spriteRenderer.flipX = false;
        CyclePlayerSprite();
    }

    public void MoveBackward() {

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
    
    private void CyclePlayerSprite() {

        currentSpriteIndex++;
        if (currentSpriteIndex >= playerSprites.Length) {
            currentSpriteIndex = 0;
        }
        spriteRenderer.sprite = playerSprites[currentSpriteIndex];
    }

    // Player constantly moves towards the target position
    // Target position is updated with valid key presses
    void Update() {

        transform.position = Vector2.MoveTowards
            (transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other) {

        Debug.Log("Die");
    }
}
