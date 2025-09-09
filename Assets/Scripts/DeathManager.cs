using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour {

    public void Replay(){
        
        AudioManager.instance.PlayButtonPressSFX();
        SceneManager.LoadScene("SingleScene");
    }

    public void Menu() {
        
        AudioManager.instance.PlayButtonPressSFX();
        SceneManager.LoadScene("MenuScene");
    }
}
