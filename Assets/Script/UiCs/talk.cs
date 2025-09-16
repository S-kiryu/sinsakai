using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel; // ダイアログパネル全体
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Dialogue Content")]
    [TextArea]
    [SerializeField] private string[] lines; // 会話のセリフを並べる

    [Header("Settings")]
    [SerializeField] private KeyCode nextLineKey = KeyCode.Space;   // 次のセリフへ進むキー
    [SerializeField] private KeyCode closeDialogueKey = KeyCode.Escape; // ダイアログを閉じるキー

    private int currentLine = 0;
    private bool isPlayerInRange = false;  // プレイヤーがコライダー内にいるかどうか
    private bool isDialogueActive = false; // ダイアログが表示されているかどうか

    private void Start()
    {
        // 開始時はダイアログを非表示にする
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        currentLine = 0;
        isDialogueActive = false;
    }

    private void Update()
    {
        // プレイヤーがコライダー内にいる場合の処理
        if (isPlayerInRange)
        {
            // ダイアログが非表示の場合、Eキーで開始
            if (!isDialogueActive && Input.GetKeyDown(KeyCode.E))
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

    // プレイヤーがコライダーに入った時（2D用）
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Press E to start dialogue");
        }
    }

    // プレイヤーがコライダーから出た時（2D用）
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // プレイヤーが離れたらダイアログを終了
            if (isDialogueActive)
            {
                EndDialogue();
            }
        }
    }

    private void StartDialogue()
    {
        currentLine = 0;
        isDialogueActive = true;

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
    }
}
