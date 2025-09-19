using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorWarp : MonoBehaviour
{
    [Header("�V�[���ݒ�")]
    [SerializeField] private string nextSceneName = "GameStage"; // ���[�v��̃V�[����

    [Header("�ڍs��ʒu�ݒ�")]
    [SerializeField] private Vector2 spawnPosition = Vector2.zero; // �v���C���[�̏o���ʒu
    [SerializeField] private bool useCustomSpawnPosition = true; // �J�X�^���ʒu���g�p���邩�ǂ���

    private bool _isPlayerInDoor = false; // ���ɗ����Ă��邩�ǂ���
    private bool _changeStage = false;   // �A���J�ږh�~�p�t���O

    // �ÓI�ϐ��ňʒu����ێ��i�V�[���ԂŃf�[�^���󂯓n���j
    public static Vector2 nextSpawnPosition;
    public static bool hasSpawnPosition = false;

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

            // �J�X�^���ʒu���g�p����ꍇ�A�ÓI�ϐ��Ɉʒu��ۑ�
            if (useCustomSpawnPosition)
            {
                nextSpawnPosition = spawnPosition;
                hasSpawnPosition = true;
            }
            else
            {
                hasSpawnPosition = false;
            }

            SceneManager.LoadScene(nextSceneName);
        }
    }

    public void StartButton()
    {
        Debug.Log("Start");
        SceneManager.LoadScene("tutorial");
    }
}