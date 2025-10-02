using System.Collections;
using UnityEngine;

public class FeatherProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float lifetime;
    private float damage;
    private Rigidbody2D rb;
    private bool isInitialized = false;

    void Awake()
    {
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // �d�͂𖳌���
        rb.gravityScale = 0f;

        // �R���C�_�[���Ȃ��ꍇ�͒ǉ�
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.2f;
        }
    }

    public void Initialize(Vector2 flyDirection, float flySpeed, float lifeTime, float dmg)
    {
        direction = flyDirection.normalized;
        speed = flySpeed;
        lifetime = lifeTime;
        damage = dmg;
        isInitialized = true;

        // �܂��������
        rb.linearVelocity = direction * speed;

        // �������Ԍ�ɔj��
        StartCoroutine(DestroyAfterLifetime());
    }

    private IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerHit"))
        {
            Player player = other.GetComponentInParent<Player>();
            if (player != null)
            {
                player.TakePlayerDamage(damage);
                Debug.Log($"�H���v���C���[�Ƀq�b�g�I�_���[�W: {damage}");

                // �H��j��
                Destroy(gameObject);
            }
        }
        // �ǂ�n�ʂɃq�b�g�����ꍇ
        //else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        //{
        //    Debug.Log("�H����Q���Ƀq�b�g");
        //    Destroy(gameObject);
        //}
    }

    void OnBecameInvisible()
    {
        // ��ʊO�ɏo����j���i�p�t�H�[�}���X����j
        Destroy(gameObject);
    }
}