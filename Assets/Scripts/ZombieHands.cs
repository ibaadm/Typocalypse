using UnityEngine;
using System.Linq;
using Unity.Netcode;

public class ZombieHands : NetworkBehaviour {

    [SerializeField] private GameObject[] zombieHands = new GameObject[3];
    [SerializeField] private float activationDistance = 1.5f;
    private Transform playerBlue;
    private Transform playerRed;
    private Vector3 lastPos = Vector3.zero;
    bool activated = false;

    void Start() {

        playerBlue = GameObject.FindWithTag("Player Blue").transform;
        playerRed = GameObject.FindWithTag("Player Red")?.transform;
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

        float redX = playerRed != null ? playerRed.position.x : float.MinValue;
        if ((transform.position.x - Mathf.Max(playerBlue.position.x, redX)) <= activationDistance && !activated) {
            activated = true;
            int[] hands = ChooseRandomHands();
            if (IsClient) { ActivateHandsClientRpc(hands); return;}
            foreach (int hand in hands) {
                zombieHands[hand].SetActive(true);
            }
        }
    }

    [ClientRpc]
    void ActivateHandsClientRpc(int[] hands) {
        foreach (int hand in hands) {
            zombieHands[hand].SetActive(true);
        }
    }

    void ShuffleZombieHands() {

        for (int i = 0; i < zombieHands.Length; i++) {
            int r = Random.Range(i, zombieHands.Length);
            (zombieHands[i], zombieHands[r]) = (zombieHands[r], zombieHands[i]);
        }
    }

    int[] ChooseRandomHands() {

        int r = Random.Range(0, 6);

        if (r == 0) {
            return new int[] { 0 };
        }
        else if (r == 1) {
            return new int[] { 1 };
        }
        else if (r == 2) {
            return new int[] { 2 };
        }
        else if (r == 3) {
            return new int[] { 0, 1 };
        }
        else if (r == 4) {
            return new int[] { 0, 2 };
        }
        else {
            return new int[] { 1, 2 };
        }
    }
}
