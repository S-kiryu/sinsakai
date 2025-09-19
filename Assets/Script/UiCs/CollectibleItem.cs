using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("�A�C�e���ݒ�")]
    public string itemId = "pumpkin_01"; // �e�A�C�e���ɌŗL��ID��ݒ�
    public string itemName = "���ڂ���";

    [Header("�G�t�F�N�g")]
    public GameObject collectEffect; // ���W���̃G�t�F�N�g�i�I�v�V�����j
    public AudioClip collectSound;   // ���W���̉��i�I�v�V�����j

    private bool isCollected = false;

    void Start()
    {
        // ���łɎ��W�ς݂̏ꍇ�͔�\���ɂ���
        if (GameManager.Instance != null && GameManager.Instance.IsItemCollected(itemId))
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // �v���C���[���g���K�[�ɓ��������i2D�p�j
        if (other.CompareTag("Player") && !isCollected)
        {
            CollectItem();
        }
    }

    void CollectItem()
    {
        isCollected = true;

        // GameManager�ɃA�C�e�����W��ʒm
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectItem(itemId);
        }

        // ���W�G�t�F�N�g���Đ�
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, transform.rotation);
        }

        // ���W�����Đ�
        if (collectSound != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(collectSound);
            }
        }

        // �I�u�W�F�N�g���\��
        gameObject.SetActive(false);
    }
}