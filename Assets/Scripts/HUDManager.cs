using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class HUDManager : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject deathPanel;
    private float playerStartX;
    private float playerHorizontalMoveDistance;
    private int score  = 0;

    private int highScore;
    [SerializeField] private TextMeshProUGUI highScoreText;

    [SerializeField] private TextMeshProUGUI timeText;
    private bool timeStopped = false;
    
    void Start() {
        
        playerStartX = player.position.x;
        playerHorizontalMoveDistance = player.GetComponent<Player>().GetHorizontalMoveDistance();
        scoreText.text = "0";
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = $"HI:{highScore}";
    }

    void Update() { 
        
        if (timeStopped) {
            deathPanel.SetActive(true);
            gameObject.SetActive(false);
            return;
        }

        UpdateTime();
        UpdateScore();
    }

    void UpdateTime() {

        // Displays time as m:ss, mm:ss, mmm:ss etc.
        int minutes = Mathf.FloorToInt(Time.time / 60);
        int seconds = Mathf.FloorToInt(Time.time % 60);
        if (minutes < 10) {
            timeText.text = $"{minutes}:{seconds:00}";
        }
        else {
            timeText.text = $"{minutes:D}:{seconds:00}";
        }
    }

    // Update the score when the player moves forward, detected when camera moves forward
    void UpdateScore() {

        if (Mathf.Floor(((player.position.x - playerStartX) / playerHorizontalMoveDistance) + 0.01f) > score) {
            score++;
            scoreText.text = $"{score}";
            UpdateHighScore();
        }
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
}