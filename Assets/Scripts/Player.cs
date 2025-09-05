using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    [SerializeField] private float moveSpeed = 0.1f;
    [SerializeField] private float horizontalMoveDistance = 0.5f;
    [SerializeField] private float verticalMoveDistance = 0.5f;
    private Vector3 targetPosition;
    private float maxHeight;
    private float minHeight;

    void Start(){

        targetPosition = transform.position;
        maxHeight = transform.position.y + verticalMoveDistance;
        minHeight = transform.position.y - verticalMoveDistance;
    }

    public void MoveForward() {

        targetPosition += new Vector3(horizontalMoveDistance, 0f, 0f);
    }

    public void MoveBackward() {

        targetPosition -= new Vector3(horizontalMoveDistance, 0f, 0f);
    }

    public void MoveUp() {

        if (targetPosition.y < maxHeight) {
            targetPosition += new Vector3(0f, verticalMoveDistance, 0f);
        }
    }

    public void MoveDown() {

        if (targetPosition.y > minHeight) {
            targetPosition -= new Vector3(0f, verticalMoveDistance, 0f);
        }
    }
    
    // Player constantly moves towards the target position
    // Target position is updated with valid key presses
    void Update() {

        transform.position = Vector3.MoveTowards
            (transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
}
