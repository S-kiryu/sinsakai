using UnityEngine;

public class BombCS : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    private bool hasDealtDamage = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Playerに当たったらアニメーションを再生
        if (other.CompareTag("PlayerHit"))
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Bomb"); // アニメーション開始
            }
        }
    }

    // --- Animation Event から呼ぶ関数 ---
    public void DoDamage()
    {
        Debug.Log("ダメージ処理！！！");
        if (hasDealtDamage) return; // 多重ヒット防止
        hasDealtDamage = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.0f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("PlayerHit"))
            {
                Debug.Log("Hitしたよ");
                Player player = hit.GetComponentInParent<Player>();
                if (player != null)
                {
                    player.TakePlayerDamage(damage);
                    Debug.Log("うっひょーーーーーーーーーーーーーー");
                }
            }
        }
    }

    // --- アニメーション終了時に呼ぶ ---
    public void OnExplosionEnd()
    {
        Destroy(gameObject);
    }
}