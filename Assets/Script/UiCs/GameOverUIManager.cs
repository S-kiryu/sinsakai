using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        
    }

    // UI�{�^������Ă�
    public void OnClickReturnToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("tutorial");
    }
}
