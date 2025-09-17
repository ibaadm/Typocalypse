using UnityEngine;

public class DeathPanel : MonoBehaviour {

    [SerializeField] DeathManager deathManager;

    void OnEnable() {
        deathManager.OnPanelEnable();
    }
}
