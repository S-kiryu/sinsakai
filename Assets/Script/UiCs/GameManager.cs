using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("収集したアイテムのリスト")]
    public List<string> collectedItems = new List<string>();

    void Awake()
    {
        // シングルトンパターンでGameManagerを管理
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // アイテムを収集する
    public void CollectItem(string itemId)
    {
        if (!collectedItems.Contains(itemId))
        {
            collectedItems.Add(itemId);
            Debug.Log($"アイテム '{itemId}' を収集しました！");
        }
    }

    // アイテムが収集済みかチェック
    public bool IsItemCollected(string itemId)
    {
        return collectedItems.Contains(itemId);
    }

    // 収集したアイテム数を取得
    public int GetCollectedItemCount()
    {
        return collectedItems.Count;
    }
}