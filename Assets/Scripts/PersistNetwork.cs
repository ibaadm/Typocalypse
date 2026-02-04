using UnityEngine;

public class PersistNetwork : MonoBehaviour {
    void Awake() {
        DontDestroyOnLoad(gameObject);
    }
}