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

    [Header("Dialogue Content - Default")]
    [TextArea]
    [SerializeField] private string[] defaultLines; // �ʏ�̉�b�i���ڂ���������Ă��Ȃ����j

    [Header("Dialogue Content - With Item")]
    [SerializeField] private string requiredItemId = "pumpkin_01"; // �K�v�ȃA�C�e����ID
    [TextArea]
    [SerializeField] private string[] itemLines; // ���ڂ���������Ă��鎞�̉�b

    [Header("Post-Item Dialogue")]
    [TextArea]
    [SerializeField] private string[] afterItemLines; // �A�C�e�����g������̉�b

    [Header("Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E; // �C���^���N�V�����L�[
    [SerializeField] private KeyCode nextLineKey = KeyCode.Space; // ���̃Z���t�֐i�ރL�[
    [SerializeField] private KeyCode closeDialogueKey = KeyCode.Escape; // �_�C�A���O�����L�[
    [SerializeField] private bool consumeItemAfterDialogue = true; // ��b��ɃA�C�e��������邩

    private int currentLine = 0;
    private bool isPlayerInRange = false;  // �v���C���[���R���C�_�[���ɂ��邩�ǂ���
    private bool isDialogueActive = false; // �_�C�A���O���\������Ă��邩�ǂ���
    private Camera mainCamera; // ���C���J�����̎Q��
    private string[] currentLines; // ���ݎg�p���Ă����b�z��
    private bool hasUsedItem = false; // �A�C�e�����g�p�������ǂ����̋L�^

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

        // �f�o�b�O���
        Debug.Log("=== DialogueManager ������ ===");
        Debug.Log($"Required Item ID: '{requiredItemId}'");
        Debug.Log($"Default Lines Count: {(defaultLines != null ? defaultLines.Length : 0)}");
        Debug.Log($"Item Lines Count: {(itemLines != null ? itemLines.Length : 0)}");
        Debug.Log($"After Item Lines Count: {(afterItemLines != null ? afterItemLines.Length : 0)}");

        // GameManager�̊m�F
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
        // ���[���hUI�̈ʒu���X�V�i�J�����Ɍ�����j
        UpdateWorldUIPosition();

        // �f�o�b�O�p�L�[����
        if (Input.GetKeyDown(KeyCode.I))
        {
            DebugItemStatus();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            DebugAddTestItem();
        }

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

    // �f�o�b�O�p�F�A�C�e���󋵂��m�F
    private void DebugItemStatus()
    {
        Debug.Log("=== �f�o�b�O�F�A�C�e���� ===");
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null!");
            return;
        }

        Debug.Log($"Total items: {GameManager.Instance.GetCollectedItemCount()}");
        Debug.Log($"Required item '{requiredItemId}' collected: {GameManager.Instance.IsItemCollected(requiredItemId)}");
        Debug.Log($"hasUsedItem: {hasUsedItem}");

        // ShowInventory��������Ȃ��ꍇ�̑�փR�[�h
        try
        {
            GameManager.Instance.ShowInventory();
        }
        catch (System.Exception e)
        {
            Debug.LogError("ShowInventory method not found: " + e.Message);
            // �蓮�ŃA�C�e���ꗗ��\��
            Debug.Log("=== Manual Inventory Display ===");
            for (int i = 0; i < GameManager.Instance.collectedItems.Count; i++)
            {
                Debug.Log($"Item {i}: '{GameManager.Instance.collectedItems[i]}'");
            }
            Debug.Log("==============================");
        }
    }

    // �f�o�b�O�p�F�e�X�g�A�C�e����ǉ�
    private void DebugAddTestItem()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectItem(requiredItemId);
            Debug.Log($"Test item '{requiredItemId}' added!");
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

    // �g�p�����b�z�������
    private string[] GetCurrentDialogueLines()
    {
        // ���ɃA�C�e�����g�p�ς݂̏ꍇ
        if (hasUsedItem && afterItemLines.Length > 0)
        {
            return afterItemLines;
        }

        // GameManager�����݂��A���ڂ���������Ă���ꍇ
        if (GameManager.Instance != null &&
            GameManager.Instance.IsItemCollected(requiredItemId) &&
            itemLines.Length > 0)
        {
            return itemLines;
        }

        // �f�t�H���g�̉�b
        return defaultLines;
    }

    private void StartDialogue()
    {
        currentLine = 0;
        isDialogueActive = true;

        // �g�p�����b������
        currentLines = GetCurrentDialogueLines();

        // ��b�z�񂪋�̏ꍇ�͉������Ȃ�
        if (currentLines == null || currentLines.Length == 0)
        {
            Debug.LogWarning("Dialogue lines are empty!");
            EndDialogue();
            return;
        }

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
            // ��b�I�����̏���
            OnDialogueComplete();
            EndDialogue();
        }
    }

    // ��b�������̏���
    private void OnDialogueComplete()
    {
        // GameManager�̑��݃`�F�b�N
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager��������܂���B�A�C�e���̏������X�L�b�v���܂��B");
            return;
        }

        // ���ڂ���������Ă��āA�A�C�e���p�̉�b���I������ꍇ
        if (GameManager.Instance.IsItemCollected(requiredItemId) &&
            currentLines == itemLines &&
            !hasUsedItem)
        {
            if (consumeItemAfterDialogue)
            {
                // �A�C�e��������
                bool consumed = GameManager.Instance.ConsumeItem(requiredItemId);
                if (consumed)
                {
                    hasUsedItem = true;
                    Debug.Log($"�A�C�e�� '{requiredItemId}' ���g�p���܂����I");
                }
                else
                {
                    Debug.LogWarning($"�A�C�e�� '{requiredItemId}' �̏���Ɏ��s���܂����B");
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

        // �v���C���[���܂��͈͓��ɂ���ꍇ�A�C���^���N�V����UI���ĕ\��
        if (isPlayerInRange)
        {
            ShowInteractionUI();
        }
    }

    // �f�o�b�O�p�F���݂̏�Ԃ��m�F
    [System.Obsolete("Debug method - remove in production")]
    private void OnDrawGizmosSelected()
    {
        // �G�f�B�^�ł̊m�F�p
        if (GameManager.Instance != null)
        {
            bool hasItem = GameManager.Instance.IsItemCollected(requiredItemId);
            Gizmos.color = hasItem ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}