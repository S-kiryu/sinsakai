using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    private string _sin = "しんでしまうとはなさけない！";

    //＝＝＝ステータス関連＝＝＝
    [SerializeField]
    private float _playerHp = 100;
    private Vector3 _originalScale;

    //＝＝＝移動系＝＝＝
    [Tooltip("移動スピード")]
    [SerializeField]
    private float _moveSpeed = 5f;

    [Tooltip("ジャンプ力")]
    [SerializeField]
    private float _jumpForce = 7f;

    //＝＝＝回避系＝＝＝
    [Tooltip("回避スピード")]
    [SerializeField]
    private float _rollSpeed = 10f;

    [Tooltip("回避時間")]
    [Header("回避時間")]
    [SerializeField]
    private float _rollDuration = 0.4f;

    private float _rollTimer = 0f;

    [Tooltip("回避キー")]
    [SerializeField]
    private KeyCode _rollKey = KeyCode.LeftShift;

    private Vector2 _rollDirection;
    private bool _isRolling = false;
    private bool _isMovementLocked = false;
    private bool _isFacingRight = true;


    //＝＝＝攻撃系＝＝＝
    [Tooltip("通常攻撃キー")]
    [SerializeField]
    private KeyCode _punchAttackKey = KeyCode.Z;

    [Tooltip("通常攻撃")]
    [Header("通常攻撃")]
    [SerializeField]
    private float _punchDmg = 1.0f;

    [Tooltip("通常攻撃クールタイム")]
    [Header("通常攻撃クールタイム")]
    [SerializeField]
    private float _punchCollTime = 10f;

    [Tooltip("強攻撃キー")]
    [SerializeField]
    private KeyCode _projectileKey = KeyCode.X;

    [Tooltip("遠距離攻撃")]
    [Header("遠距離攻撃")]
    [SerializeField]
    private float _projectileDmg = 2.0f;
    private float _attackDamage;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private Quaternion _originalRotation;

    [SerializeField] 
    private Collider2D _punchAttackCollider;
    [SerializeField]
    private Collider2D _projectileAttackCollider;

    // ＝＝＝地面判定＝＝＝
    private bool _isGrounded;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _originalRotation = transform.rotation;

        _rb.gravityScale = 3f; // 重力を有効にする
        _rb.freezeRotation = true; // 回転しないように固定
        _punchAttackCollider.enabled = false;
        _projectileAttackCollider.enabled = false;

        //元のスケールを保存
        _originalScale = transform.lossyScale;
    }

    void Update()
    {
        HandleMovement();
        HandleRoll();
        HandleJump();
        HandleAttack();
    }
    //＝＝＝HPに関する動き＝＝＝
    #region
    public float GetEnemyHp()
    {
        return _playerHp;
    }

    public void TakePlayerDamage(float damage)
    {
        _playerHp -= damage;
        Debug.Log("Playerの残りHP: " + _playerHp);
        if (_playerHp <= 0)
        {
            PlayerDie();
        }
    }

    private void PlayerDie() 
    {
        Debug.Log(_sin);
        Destroy(gameObject);
    }
    #endregion

    //＝＝＝移動に関する動き＝＝＝
    #region
    private void HandleMovement()
    {
        if (_isMovementLocked || _isRolling) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        Vector2 velocity = _rb.linearVelocity;
        velocity.x = moveX * _moveSpeed;
        _rb.linearVelocity = velocity;

        if (moveX > 0 && !_isFacingRight) Flip(true);
        else if (moveX < 0 && _isFacingRight) Flip(false);

        if (Input.GetKeyDown(_rollKey) && !_isRolling) StartRoll();
    }
    #endregion

    //＝＝＝回避に関する動き＝＝＝
    #region
    private void HandleRoll()
    {
        if (!_isRolling) return;

        _rollTimer -= Time.deltaTime;
        _rb.linearVelocity = _rollDirection * _rollSpeed; // Rigidbodyで移動させる

        transform.Rotate(0f, 0f, -360f * Time.deltaTime);

        if (_rollTimer <= 0f) EndRoll();
    }

    private void StartRoll()
    {
        _isRolling = true;
        _isMovementLocked = true;
        _rollTimer = _rollDuration;

        _rollDirection = _isFacingRight ? Vector2.right : Vector2.left;

        _col.enabled = false; // 無敵時間の表現
    }

    private void EndRoll()
    {
        _isRolling = false;
        _isMovementLocked = false;
        transform.rotation = _originalRotation;
        _col.enabled = true;

        // ロール後に速度をリセット
        _rb.linearVelocity = Vector2.zero;
    }

    private void Flip(bool faceRight)
    {
            _isFacingRight = faceRight;
            transform.localScale = new Vector3(
        _originalScale.x * (faceRight ? 1 : -1),
        _originalScale.y,
        _originalScale.z);
    }
    #endregion

    //＝＝＝ジャンプに関する動き＝＝＝
    #region
    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded && !_isRolling)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
            _isGrounded = false;
        }
    }
    #endregion

    //＝＝＝攻撃に関する動き＝＝＝
    #region
    private void HandleAttack()
    {
        if (Input.GetKeyDown(_punchAttackKey))
        {
            Debug.Log("通常攻撃！");
            _attackDamage = _punchDmg;
            StartCoroutine(DoPunchAttack());
            // アニメーションや攻撃判定呼び出し
        }

        if (Input.GetKeyDown(_projectileKey))
        {
            Debug.Log("強攻撃！");
            _attackDamage = _projectileDmg;
            StartCoroutine(DoProjectileAttack());
            // 強攻撃判定呼び出し
        }
    }

    //攻撃の当たり判定
    private IEnumerator DoPunchAttack()
    {
        Debug.Log("コライダーをオン");
        // 攻撃開始時にコライダーON
        _punchAttackCollider.enabled = true;

        // 攻撃時間だけ待機（例: 0.3秒）
        yield return new WaitForSeconds(0.3f);

        // 攻撃終了時にコライダーOFF
        _punchAttackCollider.enabled = false;
    }

    private IEnumerator DoProjectileAttack()
    {
        Debug.Log("コライダーをオン");
        // 攻撃開始時にコライダーON
        _projectileAttackCollider.enabled = true;

        // 攻撃時間だけ待機（例: 0.3秒）
        yield return new WaitForSeconds(0.3f);

        // 攻撃終了時にコライダーOFF
        _projectileAttackCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_punchAttackCollider.enabled || _projectileAttackCollider.enabled) // 攻撃判定がONのときだけ
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyAi enemy = other.GetComponent<EnemyAi>();
                if (enemy != null)
                {
                    enemy.TakeEnemyDamage(_attackDamage); // 敵のHPを削る
                }
            }
        }
    }
    #endregion

    // ＝＝＝地面の当たり判定＝＝＝
    #region
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("groundCheck"))
        {
            _isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("groundCheck"))
        {
            _isGrounded = false;
        }
    }
    #endregion
}