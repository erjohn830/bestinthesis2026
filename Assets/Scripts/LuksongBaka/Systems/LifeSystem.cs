using UnityEngine;
using UnityEngine.UI;

public class LifeSystem : MonoBehaviour
{
    public int lives = 3;
    public Transform spawnPoint;
    public PlayerController player;

    [Header("Heart UI")]
    public Image[] hearts;

    public LevelManager levelManager;   // ✅ ADD THIS

    public void LoseLife()
    {
        lives--;

        UpdateHearts();

        if (lives <= 0)
        {
            GameOver();
        }
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < lives;
        }
    }

    void GameOver()
    {
        Debug.Log("GAME OVER");

        player.StopPlayer();

        // ✅ CALL LOSE PANEL
        if (levelManager != null)
        {
            levelManager.LoseGame();
        }
    }
}