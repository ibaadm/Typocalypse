using UnityEngine;

public class IndividualZombieHand : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D other) {

        other.GetComponent<Player>().Die();
    }

}
