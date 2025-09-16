using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel; // �_�C�A���O�p�l���S��
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Dialogue Content")]
    [TextArea]
    [SerializeField] private string[] lines; // ��b�̃Z���t����ׂ�

    [Header("Settings")]
    [SerializeField] private KeyCode nextLineKey = KeyCode.Space;   // ���̃Z���t�֐i�ރL�[
    [SerializeField] private KeyCode closeDialogueKey = KeyCode.Escape; // �_�C�A���O�����L�[

    private int currentLine = 0;
    private bool isPlayerInRange = false;  // �v���C���[���R���C�_�[���ɂ��邩�ǂ���
    private bool isDialogueActive = false; // �_�C�A���O���\������Ă��邩�ǂ���

    private void Start()
    {
        // �J�n���̓_�C�A���O���\���ɂ���
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
        // �v���C���[���R���C�_�[���ɂ���ꍇ�̏���
        if (isPlayerInRange)
        {
            // �_�C�A���O����\���̏ꍇ�AE�L�[�ŊJ�n
            if (!isDialogueActive && Input.GetKeyDown(KeyCode.E))
            {
                StartDialogue();
            }
        }

        // �_�C�A���O���\������Ă���ꍇ�̏���
        if (isDialogueActive)
        {
            // ���̃Z���t�֐i��
            if (Input.GetKeyDown(nextLineKey))
            {
                NextLine();
            }

            // �_�C�A���O�������I��
            if (Input.GetKeyDown(closeDialogueKey))
            {
                EndDialogue();
            }
        }
    }

    // �v���C���[���R���C�_�[�ɓ��������i2D�p�j
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Press E to start dialogue");
        }
    }

    // �v���C���[���R���C�_�[����o�����i2D�p�j
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // �v���C���[�����ꂽ��_�C�A���O���I��
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
