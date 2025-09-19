using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorWarp : MonoBehaviour
{
    [Header("シーン設定")]
    [SerializeField] private string nextSceneName = "GameStage"; // ワープ先のシーン名

    [Header("移行後位置設定")]
    [SerializeField] private Vector2 spawnPosition = Vector2.zero; // プレイヤーの出現位置
    [SerializeField] private bool useCustomSpawnPosition = true; // カスタム位置を使用するかどうか

    private bool _isPlayerInDoor = false; // 扉に立っているかどうか
    private bool _changeStage = false;   // 連続遷移防止用フラグ

    // 静的変数で位置情報を保持（シーン間でデータを受け渡し）
    public static Vector2 nextSpawnPosition;
    public static bool hasSpawnPosition = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Playerタグを付けて判定
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
            _changeStage = true; // 一度だけ遷移させる

            // カスタム位置を使用する場合、静的変数に位置を保存
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