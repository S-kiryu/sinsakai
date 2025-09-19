using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("アイテム設定")]
    public string itemId = "pumpkin_01"; // 各アイテムに固有のIDを設定
    public string itemName = "かぼちゃ";

    [Header("エフェクト")]
    public GameObject collectEffect; // 収集時のエフェクト（オプション）
    public AudioClip collectSound;   // 収集時の音（オプション）

    private bool isCollected = false;

    void Start()
    {
        // すでに収集済みの場合は非表示にする
        if (GameManager.Instance != null && GameManager.Instance.IsItemCollected(itemId))
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーがトリガーに入った時（2D用）
        if (other.CompareTag("Player") && !isCollected)
        {
            CollectItem();
        }
    }

    void CollectItem()
    {
        isCollected = true;

        // GameManagerにアイテム収集を通知
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectItem(itemId);
        }

        // 収集エフェクトを再生
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, transform.rotation);
        }

        // 収集音を再生
        if (collectSound != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(collectSound);
            }
        }

        // オブジェクトを非表示
        gameObject.SetActive(false);
    }
}