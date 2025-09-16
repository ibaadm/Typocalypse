using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class HUDManager : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] public Transform player;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject typingText;
    private float playerStartX;
    private float playerHorizontalMoveDistance;
    private int score  = 0;

    private int highScore;
    [SerializeField] private TextMeshProUGUI highScoreText;

    [SerializeField] private TextMeshProUGUI timeText;
    private float timeElapsed = 0f;
    private bool timeStopped = false;
    
    void Start() {
        
        playerStartX = player.position.x;
        playerHorizontalMoveDistance = player.GetComponent<Player>().GetHorizontalMoveDistance();
        scoreText.text = "0";
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = $"HI:{highScore}";
        timeElapsed = 0f;
        FindAnyObjectByType<TextManager>().HUDManager = this;
    }

    void Update() { 
        
        if (timeStopped) {
            deathPanel.SetActive(true);
            AudioManager.instance.gameplay = false;
            GetComponent<HUDManager>().enabled = false;
            return;
        }

        UpdateTime();
    }

    void UpdateTime() {

        // Displays time as m:ss, mm:ss, mmm:ss etc.
        int minutes = Mathf.FloorToInt(timeElapsed / 60);
        int seconds = Mathf.FloorToInt(timeElapsed % 60);
        if (minutes < 10) {
            timeText.text = $"{minutes}:{seconds:00}";
        }
        else {
            timeText.text = $"{minutes:D}:{seconds:00}";
        }
        timeElapsed += Time.deltaTime;
    }

    // Update the score when the player moves forward, detected when camera moves forward
   public void UpdateScore() {

        //if (Mathf.Floor(((player.position.x - playerStartX) / playerHorizontalMoveDistance) + 0.01f) > score) {
        //    score++;
        //    scoreText.text = $"{score}";
        //    UpdateHighScore();
        //}
        score++;
        scoreText.text = $"{score}";
        UpdateHighScore();
    }

    void UpdateHighScore() {

        if (score > highScore) {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            highScoreText.text = $"HI:{highScore}";
        }
    }

    // When player dies, stop the time
    public void StopTime() { timeStopped = true; }

    public void DisableTypingText() { typingText.SetActive(false); }
}