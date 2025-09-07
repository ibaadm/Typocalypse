using UnityEngine;
using System.Linq;

public class ZombieHands : MonoBehaviour {

    [SerializeField] private GameObject[] zombieHands = new GameObject[3];
    [SerializeField] private float activationDistance = 1.5f;
    private Transform player;
    private Vector3 lastPos = Vector3.zero;
    bool activated = false;

    void Start() {

        player = GameObject.FindAnyObjectByType<Player>().transform;
    }

    void Update() {

        HandleDeactivation();
        HandleActivation();
    }

    // When the zombie hands are moved to the front, deactivate them
    void HandleDeactivation() {

        if (Vector3.Distance(transform.position, lastPos) > 0.01f){
            foreach (GameObject zombieHand in zombieHands) {
                zombieHand.SetActive(false);
            }
            lastPos = transform.position;
            activated = false;
        }
    }

    // When the player moves in range, activate 1 or 2 random zombie hands
    void HandleActivation() {

        if ((transform.position.x - player.position.x) <= activationDistance && !activated) {
            ShuffleZombieHands();
            zombieHands[0].SetActive(true);
            if (Random.value > 0.5f) {
                zombieHands[1].SetActive(true);
            }
            activated = true;
        }
    }

    void ShuffleZombieHands() {

        for (int i = 0; i < zombieHands.Length; i++) {
            int r = Random.Range(i, zombieHands.Length);
            (zombieHands[i], zombieHands[r]) = (zombieHands[r], zombieHands[i]);
        }
    }
}
