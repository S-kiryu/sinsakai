using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorWarp : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "GameStage"; // ���[�v��̃V�[����
    private bool _isPlayerInDoor = false; // ���ɗ����Ă��邩�ǂ���
    private bool _changeStage = false;   // �A���J�ږh�~�p�t���O



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Player�^�O��t���Ĕ���
        {
            _isPlayerInDoor = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInDoor = false;
        }
    }

    private void Update()
    {
        if (_isPlayerInDoor && !_changeStage && Input.GetKeyDown(KeyCode.C))
        {
            _changeStage = true; // ��x�����J�ڂ�����
            SceneManager.LoadScene(nextSceneName);
        }
    }

    public void StartButton()
    {
        Debug.Log("Start");
        SceneManager.LoadScene("tutorial");
    }
}


