using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverUIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Animator playerAnimator; // プレイヤーのAnimator
    [SerializeField] private string gameOverAnimationName = "GameOver"; // アニメーション名
    [SerializeField] private float animationWaitTime = 2f; // アニメーション待機時間（フォールバック用）

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        // プレイヤーのゲームオーバーアニメーションを再生
        if (playerAnimator != null)
        {
            // ゲームオーバーアニメーションをトリガー
            playerAnimator.SetTrigger("GameOver");

        }
        else
        {
            // Animatorが設定されていない場合は指定時間待機
            yield return new WaitForSeconds(animationWaitTime);
        }

        // ゲームオーバーパネルを表示
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // UIボタンから呼ぶ
    public void OnClickReturnToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("tutorial");
    }
}
