using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel; // �_�C�A���O�p�l���S��
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionUI; // �C���^���N�V�����pUI�i�L�����o�X���UI�j
    [SerializeField] private GameObject interactionWorldUI; // ���[���h���W�ł�UI�iNPC�̓���ɕ\���j
    [SerializeField] private TextMeshProUGUI interactionText; // "Press E to talk" �Ȃǂ̃e�L�X�g
    [SerializeField] private string interactionMessage = "Press E to talk"; // �\�����b�Z�[�W
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 2f, 0); // NPC�����UI�I�t�Z�b�g

    [Header("Dialogue Content")]
    [TextArea]
    [SerializeField] private string[] lines; // ��b�̃Z���t����ׂ�

    [Header("Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E; // �C���^���N�V�����L�[
    [SerializeField] private KeyCode nextLineKey = KeyCode.Space; // ���̃Z���t�֐i�ރL�[
    [SerializeField] private KeyCode closeDialogueKey = KeyCode.Escape; // �_�C�A���O�����L�[

    private int currentLine = 0;
    private bool isPlayerInRange = false;  // �v���C���[���R���C�_�[���ɂ��邩�ǂ���
    private bool isDialogueActive = false; // �_�C�A���O���\������Ă��邩�ǂ���
    private Camera mainCamera; // ���C���J�����̎Q��

    private void Start()
    {
        // ���C���J�������擾
        mainCamera = Camera.main;

        // �J�n���̓_�C�A���O���\���ɂ���
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        // �C���^���N�V����UI���\���ɂ���
        HideInteractionUI();

        currentLine = 0;
        isDialogueActive = false;
    }

    private void Update()
    {
        // ���[���hUI�̈ʒu���X�V�i�J�����Ɍ�����j
        UpdateWorldUIPosition();

        // �v���C���[���R���C�_�[���ɂ���ꍇ�̏���
        if (isPlayerInRange)
        {
            // �_�C�A���O����\���̏ꍇ�AE�L�[�ŊJ�n
            if (!isDialogueActive && Input.GetKeyDown(interactionKey))
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

    // ���[���hUI�̈ʒu�ƃJ�����������X�V
    private void UpdateWorldUIPosition()
    {
        if (interactionWorldUI != null && interactionWorldUI.activeInHierarchy && mainCamera != null)
        {
            // NPC�̓����UI��z�u
            interactionWorldUI.transform.position = transform.position + uiOffset;

            // UI���J�����Ɍ�����
            interactionWorldUI.transform.LookAt(mainCamera.transform);
            interactionWorldUI.transform.Rotate(0, 180, 0); // 180�x��]�����Đ��ʂ���������
        }
    }

    // �v���C���[���R���C�_�[�ɓ��������i2D�p�j
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            ShowInteractionUI();
            Debug.Log("Press E to start dialogue");
        }
    }

    // �v���C���[���R���C�_�[����o�����i2D�p�j
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            HideInteractionUI();

            // �v���C���[�����ꂽ��_�C�A���O���I��
            if (isDialogueActive)
            {
                EndDialogue();
            }
        }
    }

    // 3D�p�̃g���K�[�i�K�v�ɉ����Ďg�p�j
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

    // �C���^���N�V����UI��\��
    private void ShowInteractionUI()
    {
        if (!isDialogueActive) // �_�C�A���O���łȂ��ꍇ�̂ݕ\��
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

    // �C���^���N�V����UI���\��
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

        // �C���^���N�V����UI���\��
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

        // �v���C���[���܂��͈͓��ɂ���ꍇ�A�C���^���N�V����UI���ĕ\��
        if (isPlayerInRange)
        {
            ShowInteractionUI();
        }
    }
}