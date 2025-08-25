using System.Collections;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
//�������ړ��n������
    [Tooltip("�ړ��X�s�[�h")]
    [SerializeField]
    private float moveSpeed = 5f;
    [Tooltip("�W�����v��")]
    [SerializeField]
    private float jumpForce = 7f;

//����������n������
    [Tooltip("����X�s�[�h")]
    [SerializeField]
    private float rollSpeed = 10f;
    [Tooltip("����̎�������")]
    [SerializeField]
    private float rollDuration = 0.4f;
    [Tooltip("����L�[")]
    [SerializeField]
    private KeyCode rollKey = KeyCode.LeftShift;
    [SerializeField]
    private Collider2D rollHit_col;

    //�������U���n������
    [Tooltip("�ʏ�U���L�[")]
    [SerializeField]
    private KeyCode punchAttackKey = KeyCode.Z;
    [SerializeField]
    private float punchDmg = 1.0f;
    [Tooltip("���U���L�[")]
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

    // �n�ʔ���
    private bool isGrounded;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        originalRotation = transform.rotation;

        _rb.gravityScale = 3f; // �d�͂�L���ɂ���
        _rb.freezeRotation = true; // ��]���Ȃ��悤�ɌŒ�
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

    //�������ړ��Ɋւ��铮��������
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

    //����������Ɋւ��铮��������
    private void HandleRoll()
    {
        if (!isRolling) return;

        rollTimer -= Time.deltaTime;
        _rb.linearVelocity = rollDirection * rollSpeed; // Rigidbody�ňړ�������

        transform.Rotate(0f, 0f, -360f * Time.deltaTime);

        if (rollTimer <= 0f) EndRoll();
    }

    private void StartRoll()
    {
        isRolling = true;
        isMovementLocked = true;
        rollTimer = rollDuration;

        rollDirection = isFacingRight ? Vector2.right : Vector2.left;

        rollHit_col.enabled = false; // ���G���Ԃ̕\��
    }

    private void EndRoll()
    {
        isRolling = false;
        isMovementLocked = false;
        transform.rotation = originalRotation;
        rollHit_col.enabled = true;

        // ���[����ɑ��x�����Z�b�g
        _rb.linearVelocity = Vector2.zero;
    }

    private void Flip(bool faceRight)
    {
        isFacingRight = faceRight;
        transform.localScale = new Vector3(faceRight ? 1 : -1, 1, 1);
    }


    //�������W�����v�Ɋւ��铮��������
    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isRolling)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }
    }

    //�������U���Ɋւ��铮��������
    private void HandleAttack()
    {
        if (Input.GetKeyDown(punchAttackKey))
        {
            Debug.Log("�ʏ�U���I");
            attackDamage = punchDmg;
            StartCoroutine(DoPunchAttack());
            // �A�j���[�V������U������Ăяo��
        }

        if (Input.GetKeyDown(projectileKey))
        {
            Debug.Log("���U���I");
            // ���U������Ăяo��
        }
    }

    //�U���̓����蔻��
    private IEnumerator DoPunchAttack()
    {
        Debug.Log("�R���C�_�[���I��");
        // �U���J�n���ɃR���C�_�[ON
        punchAttackCollider.enabled = true;

        // �U�����Ԃ����ҋ@�i��: 0.3�b�j
        yield return new WaitForSeconds(0.3f);

        // �U���I�����ɃR���C�_�[OFF
        punchAttackCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (punchAttackCollider.enabled) // �U�����肪ON�̂Ƃ�����
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyAi enemy = other.GetComponent<EnemyAi>();
                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage); // �G��HP�����
                }
            }
        }
    }


    // �������n�ʂ̓����蔻�聁����
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