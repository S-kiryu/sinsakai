using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    private string _sin = "����ł��܂��Ƃ͂Ȃ����Ȃ��I";

    //�������X�e�[�^�X�֘A������
    [SerializeField]
    private float _playerHp = 100;
    private Vector3 _originalScale;

    //�������ړ��n������
    [Tooltip("�ړ��X�s�[�h")]
    [SerializeField]
    private float _moveSpeed = 5f;

    [Tooltip("�W�����v��")]
    [SerializeField]
    private float _jumpForce = 7f;

    //����������n������
    [Tooltip("����X�s�[�h")]
    [SerializeField]
    private float _rollSpeed = 10f;

    [Tooltip("�������")]
    [Header("�������")]
    [SerializeField]
    private float _rollDuration = 0.4f;

    private float _rollTimer = 0f;

    [Tooltip("����L�[")]
    [SerializeField]
    private KeyCode _rollKey = KeyCode.LeftShift;

    private Vector2 _rollDirection;
    private bool _isRolling = false;
    private bool _isMovementLocked = false;
    private bool _isFacingRight = true;


    //�������U���n������
    [Tooltip("�ʏ�U���L�[")]
    [SerializeField]
    private KeyCode _punchAttackKey = KeyCode.Z;

    [Tooltip("�ʏ�U��")]
    [Header("�ʏ�U��")]
    [SerializeField]
    private float _punchDmg = 1.0f;

    [Tooltip("�ʏ�U���N�[���^�C��")]
    [Header("�ʏ�U���N�[���^�C��")]
    [SerializeField]
    private float _punchCollTime = 10f;

    [Tooltip("���U���L�[")]
    [SerializeField]
    private KeyCode _projectileKey = KeyCode.X;

    [Tooltip("�������U��")]
    [Header("�������U��")]
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

    // �������n�ʔ��聁����
    private bool _isGrounded;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _originalRotation = transform.rotation;

        _rb.gravityScale = 3f; // �d�͂�L���ɂ���
        _rb.freezeRotation = true; // ��]���Ȃ��悤�ɌŒ�
        _punchAttackCollider.enabled = false;
        _projectileAttackCollider.enabled = false;

        //���̃X�P�[����ۑ�
        _originalScale = transform.lossyScale;
    }

    void Update()
    {
        HandleMovement();
        HandleRoll();
        HandleJump();
        HandleAttack();
    }
    //������HP�Ɋւ��铮��������
    #region
    public float GetEnemyHp()
    {
        return _playerHp;
    }

    public void TakePlayerDamage(float damage)
    {
        _playerHp -= damage;
        Debug.Log("Player�̎c��HP: " + _playerHp);
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

    //�������ړ��Ɋւ��铮��������
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

    //����������Ɋւ��铮��������
    #region
    private void HandleRoll()
    {
        if (!_isRolling) return;

        _rollTimer -= Time.deltaTime;
        _rb.linearVelocity = _rollDirection * _rollSpeed; // Rigidbody�ňړ�������

        transform.Rotate(0f, 0f, -360f * Time.deltaTime);

        if (_rollTimer <= 0f) EndRoll();
    }

    private void StartRoll()
    {
        _isRolling = true;
        _isMovementLocked = true;
        _rollTimer = _rollDuration;

        _rollDirection = _isFacingRight ? Vector2.right : Vector2.left;

        _col.enabled = false; // ���G���Ԃ̕\��
    }

    private void EndRoll()
    {
        _isRolling = false;
        _isMovementLocked = false;
        transform.rotation = _originalRotation;
        _col.enabled = true;

        // ���[����ɑ��x�����Z�b�g
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

    //�������W�����v�Ɋւ��铮��������
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

    //�������U���Ɋւ��铮��������
    #region
    private void HandleAttack()
    {
        if (Input.GetKeyDown(_punchAttackKey))
        {
            Debug.Log("�ʏ�U���I");
            _attackDamage = _punchDmg;
            StartCoroutine(DoPunchAttack());
            // �A�j���[�V������U������Ăяo��
        }

        if (Input.GetKeyDown(_projectileKey))
        {
            Debug.Log("���U���I");
            _attackDamage = _projectileDmg;
            StartCoroutine(DoProjectileAttack());
            // ���U������Ăяo��
        }
    }

    //�U���̓����蔻��
    private IEnumerator DoPunchAttack()
    {
        Debug.Log("�R���C�_�[���I��");
        // �U���J�n���ɃR���C�_�[ON
        _punchAttackCollider.enabled = true;

        // �U�����Ԃ����ҋ@�i��: 0.3�b�j
        yield return new WaitForSeconds(0.3f);

        // �U���I�����ɃR���C�_�[OFF
        _punchAttackCollider.enabled = false;
    }

    private IEnumerator DoProjectileAttack()
    {
        Debug.Log("�R���C�_�[���I��");
        // �U���J�n���ɃR���C�_�[ON
        _projectileAttackCollider.enabled = true;

        // �U�����Ԃ����ҋ@�i��: 0.3�b�j
        yield return new WaitForSeconds(0.3f);

        // �U���I�����ɃR���C�_�[OFF
        _projectileAttackCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_punchAttackCollider.enabled || _projectileAttackCollider.enabled) // �U�����肪ON�̂Ƃ�����
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyAi enemy = other.GetComponent<EnemyAi>();
                if (enemy != null)
                {
                    enemy.TakeEnemyDamage(_attackDamage); // �G��HP�����
                }
            }
        }
    }
    #endregion

    // �������n�ʂ̓����蔻�聁����
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