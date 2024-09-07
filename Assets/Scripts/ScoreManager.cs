using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public TextMeshProUGUI scoreText; // Reference to your TextMeshProUGUI component
    public int Score { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Optional: only if you want the ScoreManager to persist across scenes
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ResetScore();
    }

    public void AddPoint()
    {
        Score++;
        UpdateScoreUI();
    }

    public void ResetScore()
    {
        Score = 0;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Score.ToString();
        }
        else
        {
            Debug.LogWarning("Score text component is not assigned.");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // You might need to find the scoreText again here if it's not persistent
        if (scoreText == null)
        {
            scoreText = GameObject.FindWithTag("ScoreText").GetComponent<TextMeshProUGUI>();
            UpdateScoreUI(); // Ensure UI is updated with current score
        }
    }

    // Include methods for ResetScore and other functionalities as needed
}
