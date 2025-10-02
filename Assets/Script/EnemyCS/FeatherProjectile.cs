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

        // 重力を無効化
        rb.gravityScale = 0f;

        // コライダーがない場合は追加
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

        // まっすぐ飛ぶ
        rb.linearVelocity = direction * speed;

        // 生存時間後に破棄
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
                Debug.Log($"羽がプレイヤーにヒット！ダメージ: {damage}");

                // 羽を破棄
                Destroy(gameObject);
            }
        }
        // 壁や地面にヒットした場合
        //else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        //{
        //    Debug.Log("羽が障害物にヒット");
        //    Destroy(gameObject);
        //}
    }

    void OnBecameInvisible()
    {
        // 画面外に出たら破棄（パフォーマンス向上）
        Destroy(gameObject);
    }
}