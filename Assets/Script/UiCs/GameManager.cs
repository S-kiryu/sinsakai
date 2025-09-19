using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("収集したアイテムのリスト")]
    public List<string> collectedItems = new List<string>();

    [Header("デバッグ情報")]
    [SerializeField] private bool showDebugLogs = true;

    void Awake()
    {
        // より堅牢なシングルトンパターン
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (showDebugLogs)
            {
                Debug.Log($"GameManager initialized in scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
                Debug.Log($"GameManager GameObject name: {gameObject.name}");
            }
        }
        else if (Instance != this)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"Destroying duplicate GameManager in scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
                Debug.Log($"Existing GameManager has {Instance.collectedItems.Count} items");
            }
            Destroy(gameObject);
            return;
        }

        // シーン変更時のイベント登録
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // イベント登録解除
        if (Instance == this)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // シーンロード時の処理
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Scene loaded: {scene.name}");
            Debug.Log($"GameManager items count: {collectedItems.Count}");
            ShowInventory();
        }
    }

    // アイテムを収集する
    public void CollectItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogWarning("Item ID is null or empty!");
            return;
        }

        if (!collectedItems.Contains(itemId))
        {
            collectedItems.Add(itemId);
            if (showDebugLogs)
            {
                Debug.Log($"アイテム '{itemId}' を収集しました！（総数: {collectedItems.Count}）");
            }
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"アイテム '{itemId}' は既に所持しています。");
            }
        }
    }

    // アイテムが収集済みかチェック
    public bool IsItemCollected(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogWarning("Item ID is null or empty in IsItemCollected!");
            return false;
        }

        bool result = collectedItems.Contains(itemId);
        if (showDebugLogs)
        {
            Debug.Log($"アイテム '{itemId}' 所持チェック: {result}");
        }
        return result;
    }

    // 収集したアイテム数を取得
    public int GetCollectedItemCount()
    {
        return collectedItems.Count;
    }

    // アイテムを消費（リストから削除）
    public bool ConsumeItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogWarning("Item ID is null or empty in ConsumeItem!");
            return false;
        }

        if (collectedItems.Contains(itemId))
        {
            collectedItems.Remove(itemId);
            if (showDebugLogs)
            {
                Debug.Log($"アイテム '{itemId}' を使用しました。（残り: {collectedItems.Count}）");
            }
            return true;
        }
        if (showDebugLogs)
        {
            Debug.LogWarning($"アイテム '{itemId}' が見つかりません。");
        }
        return false;
    }

    // 特定のアイテムを複数個持っているかチェック
    public bool HasItemCount(string itemId, int count)
    {
        int itemCount = 0;
        foreach (string item in collectedItems)
        {
            if (item == itemId)
                itemCount++;
        }
        return itemCount >= count;
    }

    // すべてのアイテムをクリア（ゲームリセット用）
    public void ClearAllItems()
    {
        collectedItems.Clear();
        if (showDebugLogs)
        {
            Debug.Log("全てのアイテムをクリアしました。");
        }
    }

    // デバッグ用：所持アイテム一覧を表示
    public void ShowInventory()
    {
        Debug.Log("=== GameManager 所持アイテム ===");
        Debug.Log($"Instance exists: {Instance != null}");
        Debug.Log($"This GameObject: {gameObject.name}");
        Debug.Log($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

        if (collectedItems.Count == 0)
        {
            Debug.Log("アイテムを持っていません。");
        }
        else
        {
            for (int i = 0; i < collectedItems.Count; i++)
            {
                Debug.Log($"{i + 1}. '{collectedItems[i]}'");
            }
        }
        Debug.Log($"合計アイテム数: {collectedItems.Count}");
        Debug.Log("===============================");
    }

    // Update中でテスト用キー入力（デバッグ用）
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("=== GameManager Status ===");
            ShowInventory();
            Debug.Log($"Instance reference: {Instance}");
            Debug.Log($"This object: {this}");
            Debug.Log($"Are they same: {Instance == this}");
            Debug.Log("=========================");
        }

        // 強制的にアイテム追加（テスト用）
        if (Input.GetKeyDown(KeyCode.L))
        {
            CollectItem("test_pumpkin");
            Debug.Log("Test pumpkin added via L key");
        }
    }

    // GameManagerが正常に機能しているかチェック
    public bool IsValid()
    {
        return Instance == this && gameObject != null;
    }
}