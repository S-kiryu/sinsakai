using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("���W�����A�C�e���̃��X�g")]
    public List<string> collectedItems = new List<string>();

    void Awake()
    {
        // �V���O���g���p�^�[����GameManager���Ǘ�
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

    // �A�C�e�������W����
    public void CollectItem(string itemId)
    {
        if (!collectedItems.Contains(itemId))
        {
            collectedItems.Add(itemId);
            Debug.Log($"�A�C�e�� '{itemId}' �����W���܂����I");
        }
    }

    // �A�C�e�������W�ς݂��`�F�b�N
    public bool IsItemCollected(string itemId)
    {
        return collectedItems.Contains(itemId);
    }

    // ���W�����A�C�e�������擾
    public int GetCollectedItemCount()
    {
        return collectedItems.Count;
    }
}