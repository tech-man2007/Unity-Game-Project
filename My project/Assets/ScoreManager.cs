using UnityEngine;
using TMPro; // Needed to talk to the TextMeshPro component

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public TextMeshProUGUI scoreText;
    private int score = 0;

    void Awake()
    {
        // This allows other scripts to find this one easily
        instance = this;
    }

    public void AddPoint()
    {
        score++;
        scoreText.text = "Enemies Shattered: " + score;
    }
}