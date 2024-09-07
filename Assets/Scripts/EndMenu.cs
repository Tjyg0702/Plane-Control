using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndMenu : MonoBehaviour
{

    public TextMeshProUGUI finalScoreText; // Reference to the TextMeshPro element

    private void Start()
    {
        if (ScoreManager.instance != null && finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + ScoreManager.instance.Score.ToString();
        }
        else
        {
            if (ScoreManager.instance == null)
            {
                Debug.LogError("ScoreManager instance is null.");
            }

            if (finalScoreText == null)
            {
                Debug.LogError("FinalScoreText is not assigned in the Inspector.");
            }
        }
    }
    public void Quit()
    {
        Debug.Log("Quitting...");
        Application.Quit();
    }
}
