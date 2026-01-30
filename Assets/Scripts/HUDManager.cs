using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] public Player player;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject typingText;
    [HideInInspector] public int score  = 0;
    // private float playerStartX;
    // private float playerHorizontalMoveDistance;

    private int highScore;
    [SerializeField] private TextMeshProUGUI highScoreText;

    public TextMeshProUGUI timeText;
    private float timeElapsed = 0f;
    private bool timeStopped = false;
    
    void Start() {
        
        // playerStartX = player.transform.position.x;
        // playerHorizontalMoveDistance = player.GetHorizontalMoveDistance();
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

        if (player.isEaten) {
            AudioManager.instance.gameplay = false;
            return;
        }

        UpdateTime();
    }

    void UpdateTime() { if (player.isEaten) { return; }

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

   public void UpdateScore() {

        /*if (Mathf.Floor(((player.position.x - playerStartX) / playerHorizontalMoveDistance) + 0.01f) > score) {
        //    score++;
        //    scoreText.text = $"{score}";
        //    UpdateHighScore();
        }*/
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

    public void StopTime() { timeStopped = true; }

    public void DisableTypingText() { typingText.SetActive(false); }
}