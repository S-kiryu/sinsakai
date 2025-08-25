using System.Collections;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
//＝＝＝移動系＝＝＝
    [Tooltip("移動スピード")]
    [SerializeField]
    private float moveSpeed = 5f;
    [Tooltip("ジャンプ力")]
    [SerializeField]
    private float jumpForce = 7f;

//＝＝＝回避系＝＝＝
    [Tooltip("回避スピード")]
    [SerializeField]
    private float rollSpeed = 10f;
    [Tooltip("回避の持続時間")]
    [SerializeField]
    private float rollDuration = 0.4f;
    [Tooltip("回避キー")]
    [SerializeField]
    private KeyCode rollKey = KeyCode.LeftShift;
    [SerializeField]
    private Collider2D rollHit_col;

    //＝＝＝攻撃系＝＝＝
    [Tooltip("通常攻撃キー")]
    [SerializeField]
    private KeyCode punchAttackKey = KeyCode.Z;
    [SerializeField]
    private float punchDmg = 1.0f;
    [Tooltip("強攻撃キー")]
    [SerializeField]
    private KeyCode projectileKey = KeyCode.X;
    [SerializeField]
    private float projectileDmg = 2.0f;
    private float attackDamage;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private Quaternion originalRotation;

    [SerializeField] 
    private Collider2D punchAttackCollider;
    private Vector2 rollDirection;
    private bool isRolling = false;
    private bool isMovementLocked = false;
    private bool isFacingRight = true;
    private float rollTimer = 0f;

    // 地面判定
    private bool isGrounded;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        originalRotation = transform.rotation;

        _rb.gravityScale = 3f; // 重力を有効にする
        _rb.freezeRotation = true; // 回転しないように固定
        punchAttackCollider.enabled = false;
        rollHit_col.enabled = false;
    }

    void Update()
    {
        HandleMovement();
        HandleRoll();
        HandleJump();
        HandleAttack();
    }

    //＝＝＝移動に関する動き＝＝＝
    private void HandleMovement()
    {
        if (isMovementLocked || isRolling) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        Vector2 velocity = _rb.linearVelocity;
        velocity.x = moveX * moveSpeed;
        _rb.linearVelocity = velocity;

        if (moveX > 0 && !isFacingRight) Flip(true);
        else if (moveX < 0 && isFacingRight) Flip(false);

        if (Input.GetKeyDown(rollKey) && !isRolling) StartRoll();
    }

    //＝＝＝回避に関する動き＝＝＝
    private void HandleRoll()
    {
        if (!isRolling) return;

        rollTimer -= Time.deltaTime;
        _rb.linearVelocity = rollDirection * rollSpeed; // Rigidbodyで移動させる

        transform.Rotate(0f, 0f, -360f * Time.deltaTime);

        if (rollTimer <= 0f) EndRoll();
    }

    private void StartRoll()
    {
        isRolling = true;
        isMovementLocked = true;
        rollTimer = rollDuration;

        rollDirection = isFacingRight ? Vector2.right : Vector2.left;

        rollHit_col.enabled = false; // 無敵時間の表現
    }

    private void EndRoll()
    {
        isRolling = false;
        isMovementLocked = false;
        transform.rotation = originalRotation;
        rollHit_col.enabled = true;

        // ロール後に速度をリセット
        _rb.linearVelocity = Vector2.zero;
    }

    private void Flip(bool faceRight)
    {
        isFacingRight = faceRight;
        transform.localScale = new Vector3(faceRight ? 1 : -1, 1, 1);
    }


    //＝＝＝ジャンプに関する動き＝＝＝
    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isRolling)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }
    }

    //＝＝＝攻撃に関する動き＝＝＝
    private void HandleAttack()
    {
        if (Input.GetKeyDown(punchAttackKey))
        {
            Debug.Log("通常攻撃！");
            attackDamage = punchDmg;
            StartCoroutine(DoPunchAttack());
            // アニメーションや攻撃判定呼び出し
        }

        if (Input.GetKeyDown(projectileKey))
        {
            Debug.Log("強攻撃！");
            // 強攻撃判定呼び出し
        }
    }

    //攻撃の当たり判定
    private IEnumerator DoPunchAttack()
    {
        Debug.Log("コライダーをオン");
        // 攻撃開始時にコライダーON
        punchAttackCollider.enabled = true;

        // 攻撃時間だけ待機（例: 0.3秒）
        yield return new WaitForSeconds(0.3f);

        // 攻撃終了時にコライダーOFF
        punchAttackCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (punchAttackCollider.enabled) // 攻撃判定がONのときだけ
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyAi enemy = other.GetComponent<EnemyAi>();
                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage); // 敵のHPを削る
                }
            }
        }
    }


    // ＝＝＝地面の当たり判定＝＝＝
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("groundCheck"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("groundCheck"))
        {
            isGrounded = false;
        }
    }

}