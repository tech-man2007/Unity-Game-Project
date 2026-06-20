using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject gameOverPanel; 
    public TextMeshProUGUI statusText; 
    
    private bool isGameOver = false;

    void Update()
    {
        if (isGameOver) return;

        // NEW LOGIC: We look for the "EnemyAI" script instead of a tag.
        // This ensures shattered blocks are NEVER counted as living enemies.
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        // This will now accurately show "0" once the last red man is shattered
        Debug.Log("Living Enemies: " + enemies.Length);

        if (enemies.Length == 0)
        {
            WinGame();
        }
    }

    public void WinGame()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        statusText.text = "YOU SURVIVED"; 
        statusText.color = Color.green;
        ShowEndScreen();
    }

    public void LoseGame()
    {
        if (isGameOver) return;

        isGameOver = true;
        statusText.text = "YOU GOT HIT";
        statusText.color = Color.red;
        ShowEndScreen();
    }

    void ShowEndScreen()
    {
        gameOverPanel.SetActive(true);
        
        // Unlock the mouse so you can click the button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Stop time completely
        Time.timeScale = 0f; 
    }

    public void Retry()
    {
        // Reset time so the next game actually runs!
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}