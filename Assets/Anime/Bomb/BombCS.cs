using UnityEngine;

public class BombCS : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    private bool hasDealtDamage = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player�ɓ���������A�j���[�V�������Đ�
        if (other.CompareTag("PlayerHit"))
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Bomb"); // �A�j���[�V�����J�n
            }
        }
    }

    // --- Animation Event ����ĂԊ֐� ---
    public void DoDamage()
    {
        Debug.Log("�_���[�W�����I�I�I");
        if (hasDealtDamage) return; // ���d�q�b�g�h�~
        hasDealtDamage = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.0f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("PlayerHit"))
            {
                Debug.Log("Hit������");
                Player player = hit.GetComponentInParent<Player>();
                if (player != null)
                {
                    player.TakePlayerDamage(damage);
                    Debug.Log("�����Ђ�[�[�[�[�[�[�[�[�[�[�[�[�[�[");
                }
            }
        }
    }

    // --- �A�j���[�V�����I�����ɌĂ� ---
    public void OnExplosionEnd()
    {
        Destroy(gameObject);
    }
}