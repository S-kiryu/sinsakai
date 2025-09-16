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

    [Header("Dialogue Content")]
    [TextArea]
    [SerializeField] private string[] lines; // 会話のセリフを並べる

    [Header("Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E; // インタラクションキー
    [SerializeField] private KeyCode nextLineKey = KeyCode.Space; // 次のセリフへ進むキー
    [SerializeField] private KeyCode closeDialogueKey = KeyCode.Escape; // ダイアログを閉じるキー

    private int currentLine = 0;
    private bool isPlayerInRange = false;  // プレイヤーがコライダー内にいるかどうか
    private bool isDialogueActive = false; // ダイアログが表示されているかどうか
    private Camera mainCamera; // メインカメラの参照

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
    }

    private void Update()
    {
        // ワールドUIの位置を更新（カメラに向ける）
        UpdateWorldUIPosition();

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

    private void StartDialogue()
    {
        currentLine = 0;
        isDialogueActive = true;

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
        if (currentLine < lines.Length && dialogueText != null)
        {
            dialogueText.text = lines[currentLine];
        }
    }

    private void NextLine()
    {
        currentLine++;
        if (currentLine < lines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
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
}