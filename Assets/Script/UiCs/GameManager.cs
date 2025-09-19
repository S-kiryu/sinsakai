using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("���W�����A�C�e���̃��X�g")]
    public List<string> collectedItems = new List<string>();

    [Header("�f�o�b�O���")]
    [SerializeField] private bool showDebugLogs = true;

    void Awake()
    {
        // ��茘�S�ȃV���O���g���p�^�[��
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

        // �V�[���ύX���̃C�x���g�o�^
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // �C�x���g�o�^����
        if (Instance == this)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // �V�[�����[�h���̏���
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Scene loaded: {scene.name}");
            Debug.Log($"GameManager items count: {collectedItems.Count}");
            ShowInventory();
        }
    }

    // �A�C�e�������W����
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
                Debug.Log($"�A�C�e�� '{itemId}' �����W���܂����I�i����: {collectedItems.Count}�j");
            }
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"�A�C�e�� '{itemId}' �͊��ɏ������Ă��܂��B");
            }
        }
    }

    // �A�C�e�������W�ς݂��`�F�b�N
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
            Debug.Log($"�A�C�e�� '{itemId}' �����`�F�b�N: {result}");
        }
        return result;
    }

    // ���W�����A�C�e�������擾
    public int GetCollectedItemCount()
    {
        return collectedItems.Count;
    }

    // �A�C�e��������i���X�g����폜�j
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
                Debug.Log($"�A�C�e�� '{itemId}' ���g�p���܂����B�i�c��: {collectedItems.Count}�j");
            }
            return true;
        }
        if (showDebugLogs)
        {
            Debug.LogWarning($"�A�C�e�� '{itemId}' ��������܂���B");
        }
        return false;
    }

    // ����̃A�C�e���𕡐������Ă��邩�`�F�b�N
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

    // ���ׂẴA�C�e�����N���A�i�Q�[�����Z�b�g�p�j
    public void ClearAllItems()
    {
        collectedItems.Clear();
        if (showDebugLogs)
        {
            Debug.Log("�S�ẴA�C�e�����N���A���܂����B");
        }
    }

    // �f�o�b�O�p�F�����A�C�e���ꗗ��\��
    public void ShowInventory()
    {
        Debug.Log("=== GameManager �����A�C�e�� ===");
        Debug.Log($"Instance exists: {Instance != null}");
        Debug.Log($"This GameObject: {gameObject.name}");
        Debug.Log($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

        if (collectedItems.Count == 0)
        {
            Debug.Log("�A�C�e���������Ă��܂���B");
        }
        else
        {
            for (int i = 0; i < collectedItems.Count; i++)
            {
                Debug.Log($"{i + 1}. '{collectedItems[i]}'");
            }
        }
        Debug.Log($"���v�A�C�e����: {collectedItems.Count}");
        Debug.Log("===============================");
    }

    // Update���Ńe�X�g�p�L�[���́i�f�o�b�O�p�j
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

        // �����I�ɃA�C�e���ǉ��i�e�X�g�p�j
        if (Input.GetKeyDown(KeyCode.L))
        {
            CollectItem("test_pumpkin");
            Debug.Log("Test pumpkin added via L key");
        }
    }

    // GameManager������ɋ@�\���Ă��邩�`�F�b�N
    public bool IsValid()
    {
        return Instance == this && gameObject != null;
    }
}