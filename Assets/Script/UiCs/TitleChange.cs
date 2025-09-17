using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorWarp : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "GameStage"; // ワープ先のシーン名
    private bool _isPlayerInDoor = false; // 扉に立っているかどうか
    private bool _changeStage = false;   // 連続遷移防止用フラグ



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
            SceneManager.LoadScene(nextSceneName);
        }
    }

    public void StartButton()
    {
        Debug.Log("Start");
        SceneManager.LoadScene("tutorial");
    }
}


