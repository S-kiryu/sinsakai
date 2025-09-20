using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverUIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Animator playerAnimator; // �v���C���[��Animator
    [SerializeField] private string gameOverAnimationName = "GameOver"; // �A�j���[�V������
    [SerializeField] private float animationWaitTime = 2f; // �A�j���[�V�����ҋ@���ԁi�t�H�[���o�b�N�p�j

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
        // �v���C���[�̃Q�[���I�[�o�[�A�j���[�V�������Đ�
        if (playerAnimator != null)
        {
            // �Q�[���I�[�o�[�A�j���[�V�������g���K�[
            playerAnimator.SetTrigger("GameOver");

        }
        else
        {
            // Animator���ݒ肳��Ă��Ȃ��ꍇ�͎w�莞�ԑҋ@
            yield return new WaitForSeconds(animationWaitTime);
        }

        // �Q�[���I�[�o�[�p�l����\��
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
