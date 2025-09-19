using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel; // ダイアログパネル全体
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionUI; // インタラクション用UI（キャンバス上のUI）
    [SerializeField] private GameObject interactionWorldUI; // ワールド座標でのUI（NPCの頭上に表示）
    [SerializeField] private TextMeshProUGUI interactionText; // "Press E to talk" などのテキスト
    [SerializeField] private string interactionMessage = "Press E to talk"; // 表示メッセージ
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 2f, 0); // NPCからのUIオフセット

    [Header("Dialogue Content - Default")]
    [TextArea]
    [SerializeField] private string[] defaultLines; // 通常の会話（かぼちゃを持っていない時）

    [Header("Dialogue Content - With Item")]
    [SerializeField] private string requiredItemId = "pumpkin_01"; // 必要なアイテムのID
    [TextArea]
    [SerializeField] private string[] itemLines; // かぼちゃを持っている時の会話

    [Header("Post-Item Dialogue")]
    [TextArea]
    [SerializeField] private string[] afterItemLines; // アイテムを使った後の会話

    [Header("Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E; // インタラクションキー
    [SerializeField] private KeyCode nextLineKey = KeyCode.Space; // 次のセリフへ進むキー
    [SerializeField] private KeyCode closeDialogueKey = KeyCode.Escape; // ダイアログを閉じるキー
    [SerializeField] private bool consumeItemAfterDialogue = true; // 会話後にアイテムを消費するか

    private int currentLine = 0;
    private bool isPlayerInRange = false;  // プレイヤーがコライダー内にいるかどうか
    private bool isDialogueActive = false; // ダイアログが表示されているかどうか
    private Camera mainCamera; // メインカメラの参照
    private string[] currentLines; // 現在使用している会話配列
    private bool hasUsedItem = false; // アイテムを使用したかどうかの記録

    private void Start()
    {
        // メインカメラを取得
        mainCamera = Camera.main;

        // 開始時はダイアログを非表示にする
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        // インタラクションUIを非表示にする
        HideInteractionUI();

        currentLine = 0;
        isDialogueActive = false;

        // デバッグ情報
        Debug.Log("=== DialogueManager 初期化 ===");
        Debug.Log($"Required Item ID: '{requiredItemId}'");
        Debug.Log($"Default Lines Count: {(defaultLines != null ? defaultLines.Length : 0)}");
        Debug.Log($"Item Lines Count: {(itemLines != null ? itemLines.Length : 0)}");
        Debug.Log($"After Item Lines Count: {(afterItemLines != null ? afterItemLines.Length : 0)}");

        // GameManagerの確認
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance is null at Start!");
        }
        else
        {
            Debug.Log("GameManager found successfully");
            Debug.Log($"Current items count: {GameManager.Instance.GetCollectedItemCount()}");
        }
        Debug.Log("==============================");
    }

    private void Update()
    {
        // ワールドUIの位置を更新（カメラに向ける）
        UpdateWorldUIPosition();

        // デバッグ用キー入力
        if (Input.GetKeyDown(KeyCode.I))
        {
            DebugItemStatus();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            DebugAddTestItem();
        }

        // プレイヤーがコライダー内にいる場合の処理
        if (isPlayerInRange)
        {
            // ダイアログが非表示の場合、Eキーで開始
            if (!isDialogueActive && Input.GetKeyDown(interactionKey))
            {
                StartDialogue();
            }
        }

        // ダイアログが表示されている場合の処理
        if (isDialogueActive)
        {
            // 次のセリフへ進む
            if (Input.GetKeyDown(nextLineKey))
            {
                NextLine();
            }
            // ダイアログを強制終了
            if (Input.GetKeyDown(closeDialogueKey))
            {
                EndDialogue();
            }
        }
    }

    // デバッグ用：アイテム状況を確認
    private void DebugItemStatus()
    {
        Debug.Log("=== デバッグ：アイテム状況 ===");
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null!");
            return;
        }

        Debug.Log($"Total items: {GameManager.Instance.GetCollectedItemCount()}");
        Debug.Log($"Required item '{requiredItemId}' collected: {GameManager.Instance.IsItemCollected(requiredItemId)}");
        Debug.Log($"hasUsedItem: {hasUsedItem}");

        // ShowInventoryが見つからない場合の代替コード
        try
        {
            GameManager.Instance.ShowInventory();
        }
        catch (System.Exception e)
        {
            Debug.LogError("ShowInventory method not found: " + e.Message);
            // 手動でアイテム一覧を表示
            Debug.Log("=== Manual Inventory Display ===");
            for (int i = 0; i < GameManager.Instance.collectedItems.Count; i++)
            {
                Debug.Log($"Item {i}: '{GameManager.Instance.collectedItems[i]}'");
            }
            Debug.Log("==============================");
        }
    }

    // デバッグ用：テストアイテムを追加
    private void DebugAddTestItem()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectItem(requiredItemId);
            Debug.Log($"Test item '{requiredItemId}' added!");
        }
    }

    // ワールドUIの位置とカメラ向きを更新
    private void UpdateWorldUIPosition()
    {
        if (interactionWorldUI != null && interactionWorldUI.activeInHierarchy && mainCamera != null)
        {
            // NPCの頭上にUIを配置
            interactionWorldUI.transform.position = transform.position + uiOffset;

            // UIをカメラに向ける
            interactionWorldUI.transform.LookAt(mainCamera.transform);
            interactionWorldUI.transform.Rotate(0, 180, 0); // 180度回転させて正面を向かせる
        }
    }

    // プレイヤーがコライダーに入った時（2D用）
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            ShowInteractionUI();
            Debug.Log("Press E to start dialogue");
        }
    }

    // プレイヤーがコライダーから出た時（2D用）
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            HideInteractionUI();

            // プレイヤーが離れたらダイアログを終了
            if (isDialogueActive)
            {
                EndDialogue();
            }
        }
    }

    // 3D用のトリガー（必要に応じて使用）
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            ShowInteractionUI();
            Debug.Log("Press E to start dialogue");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            HideInteractionUI();

            if (isDialogueActive)
            {
                EndDialogue();
            }
        }
    }

    // インタラクションUIを表示
    private void ShowInteractionUI()
    {
        if (!isDialogueActive) // ダイアログ中でない場合のみ表示
        {
            if (interactionUI != null)
            {
                interactionUI.SetActive(true);
            }

            if (interactionWorldUI != null)
            {
                interactionWorldUI.SetActive(true);
            }

            if (interactionText != null)
            {
                interactionText.text = interactionMessage;
            }
        }
    }

    // インタラクションUIを非表示
    private void HideInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        if (interactionWorldUI != null)
        {
            interactionWorldUI.SetActive(false);
        }
    }

    // 使用する会話配列を決定
    private string[] GetCurrentDialogueLines()
    {
        // 既にアイテムを使用済みの場合
        if (hasUsedItem && afterItemLines.Length > 0)
        {
            return afterItemLines;
        }

        // GameManagerが存在し、かぼちゃを持っている場合
        if (GameManager.Instance != null &&
            GameManager.Instance.IsItemCollected(requiredItemId) &&
            itemLines.Length > 0)
        {
            return itemLines;
        }

        // デフォルトの会話
        return defaultLines;
    }

    private void StartDialogue()
    {
        currentLine = 0;
        isDialogueActive = true;

        // 使用する会話を決定
        currentLines = GetCurrentDialogueLines();

        // 会話配列が空の場合は何もしない
        if (currentLines == null || currentLines.Length == 0)
        {
            Debug.LogWarning("Dialogue lines are empty!");
            EndDialogue();
            return;
        }

        // インタラクションUIを非表示
        HideInteractionUI();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }

        ShowLine();
    }

    private void ShowLine()
    {
        Debug.Log($"ShowLine - Current Line: {currentLine}, Total Lines: {(currentLines != null ? currentLines.Length : 0)}");

        if (currentLine < currentLines.Length && dialogueText != null)
        {
            dialogueText.text = currentLines[currentLine];
            Debug.Log($"Displaying: '{currentLines[currentLine]}'");
        }
        else
        {
            if (dialogueText == null)
            {
                Debug.LogError("dialogueText is null!");
            }
            if (currentLine >= currentLines.Length)
            {
                Debug.LogWarning($"Current line ({currentLine}) exceeds array length ({currentLines.Length})");
            }
        }
    }

    private void NextLine()
    {
        currentLine++;
        if (currentLine < currentLines.Length)
        {
            ShowLine();
        }
        else
        {
            // 会話終了時の処理
            OnDialogueComplete();
            EndDialogue();
        }
    }

    // 会話完了時の処理
    private void OnDialogueComplete()
    {
        // GameManagerの存在チェック
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManagerが見つかりません。アイテムの処理をスキップします。");
            return;
        }

        // かぼちゃを持っていて、アイテム用の会話が終わった場合
        if (GameManager.Instance.IsItemCollected(requiredItemId) &&
            currentLines == itemLines &&
            !hasUsedItem)
        {
            if (consumeItemAfterDialogue)
            {
                // アイテムを消費
                bool consumed = GameManager.Instance.ConsumeItem(requiredItemId);
                if (consumed)
                {
                    hasUsedItem = true;
                    Debug.Log($"アイテム '{requiredItemId}' を使用しました！");
                }
                else
                {
                    Debug.LogWarning($"アイテム '{requiredItemId}' の消費に失敗しました。");
                }
            }
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        currentLine = 0;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        // プレイヤーがまだ範囲内にいる場合、インタラクションUIを再表示
        if (isPlayerInRange)
        {
            ShowInteractionUI();
        }
    }

    // デバッグ用：現在の状態を確認
    [System.Obsolete("Debug method - remove in production")]
    private void OnDrawGizmosSelected()
    {
        // エディタでの確認用
        if (GameManager.Instance != null)
        {
            bool hasItem = GameManager.Instance.IsItemCollected(requiredItemId);
            Gizmos.color = hasItem ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}