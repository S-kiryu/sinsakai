using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class EnemyAi : MonoBehaviour
{
    private string  _daijoubu = "���v�����Ȃ�";

    //�������G�̏�񁁁���
    [Tooltip("�v���C���[�����m���鋗��")]
    [Header("�v���C���[���m����")]
    [SerializeField]
    private float _detectionRange = 5f;
    private Vector3 _originalScale;


    //�������G�̃X�e�[�^�X������
    [SerializeField]
    private float _enemyHp = 1528.0f;
    private float _enemyMaxHp;

    [Tooltip("�G���񕜂��s���l")]
    [Header("�G���񕜂��s���l")]
    [SerializeField]
    private float _enemyHealValue = 509.0f;

    [Header("�񕜂ł��鐔")]
    [SerializeField]
    private int _potion = 1;

    [Tooltip("�񕜒l")]
    [Header("�񕜒l")]
    [SerializeField]
    private float _potionHeal = 1000.0f;
    private float _moveSpeed = 2f;

    //�������o�b�N�̓��쁁����
    [Tooltip("��ރX�s�[�h")]
    [SerializeField]
    private float _backSpeed = 5f;
    private bool _isMovingBack = true;

    private Transform _player_Pos;
    private Rigidbody2D _rb;
    private EnemyMoveState _enemyMoveState;
    private EnemyAttackState _enemyAtkState;
    private bool _enemyMoveStateEnter = true;


    public enum EnemyMoveState
    {
        Idle,
        Walk,
        Dash,
        Back,
        Heal,
    }

    void ChangeMoveState(EnemyMoveState newMoveState) 
    {
        _enemyMoveState = newMoveState;
        _enemyMoveStateEnter = true;
    }

    //�������G�̍U���֘A������
    [SerializeField]
    private float _enemyNormalAtk_Dmg = 10;
    [SerializeField]
    private float _enemyNormalAtk_Time = 10f;
    private float _enemyFinalDmg;
    [SerializeField]
    private Collider2D _NormalAtk_col;
    private float _enemyAtkCoolTime = 0; //0�`1�Ŕ��f
    private bool _enemyAtkStateEnter = true;


    public enum EnemyAttackState
    {
        AttackIdle,
        NotAttack,
        Attack,
        HardAttack,
    }

    void ChangeAttackState(EnemyAttackState newAttackState) 
    {
        _enemyAtkState = newAttackState;
        _enemyAtkStateEnter = true;

    }


    void Start()
    {
        _NormalAtk_col.enabled = false;
        _enemyMaxHp = _enemyHp;
        _originalScale = transform.localScale;
        _rb = GetComponent<Rigidbody2D>();
        _enemyMoveState = EnemyMoveState.Idle;
        _enemyAtkState = EnemyAttackState.AttackIdle;
        _player_Pos = GameObject.FindGameObjectWithTag("Player").transform;
    }


    void Update()
    {
        float distance = Vector2.Distance(transform.position, _player_Pos.position);

        switch (_enemyMoveState)
        {
            case EnemyMoveState.Idle:
                Idle();
                if (distance < _detectionRange && _enemyAtkState != EnemyAttackState.Attack)
                {
                    Debug.Log("�v���C���[�𔭌��I");
                    ChangeMoveState(EnemyMoveState.Walk);
                }
                break;

            case EnemyMoveState.Walk:
                if (_enemyAtkState != EnemyAttackState.Attack)
                {
                    Walk();
                }

                ChangeAttackState(EnemyAttackState.NotAttack);
                Debug.Log("�����Ă�I�I�I");
                
                if (_enemyHp <= _enemyHealValue && _potion > 0) 
                {
                    _potion--;
                    Debug.Log(" �|�[�V����������");
                    ChangeMoveState(EnemyMoveState.Heal);
                }

                break;

            case EnemyMoveState.Back:
                EnemyBack();
                break;

            case EnemyMoveState.Heal:
                //isMovingBack = false;
                //if (isMovingBack == true)
               // {
                    Debug.Log("�񕜂��Ă�I�I�I");
                    Heal();
                //}
        
                break;

            case EnemyMoveState.Dash:

                break;
        }

        //�U���̃N�[���^�C���̌v�Z
        //�D��x�̐ݒ�
        #region
        CoolTime(_enemyNormalAtk_Time);
        #endregion
        switch (_enemyAtkState) 
        {
            case EnemyAttackState.AttackIdle:
                break;

            case EnemyAttackState.NotAttack:
                if (_enemyAtkStateEnter) 
                {
                    _enemyAtkStateEnter = false;
                    Debug.Log("�U����������");
                }

                if (_enemyAtkCoolTime >= 1) 
                {
                    ChangeAttackState(EnemyAttackState.Attack);
                    ChangeMoveState(EnemyMoveState.Idle);
                    _enemyAtkCoolTime = 0;
                }

                break;

            case EnemyAttackState.Attack:

                if (_enemyAtkStateEnter)
                {
                    _enemyAtkStateEnter = false;
                    Attack();
                }

                break;

            case EnemyAttackState.HardAttack:

                HardAttack();

                break;
        }
    }

    //�������ړ��֘A������
    #region
    private void Idle() 
    {
        
    }

    private void Walk()
    {
        Vector2 direction = (_player_Pos.position - transform.position).normalized;

        // ���������̈ړ�����
        _rb.linearVelocity = new Vector2(direction.x * _moveSpeed, _rb.linearVelocity.y);

        // ���E�𔽓]������i�X�P�[�����ێ������܂܁j
        if (direction.x != 0)
        {
            transform.localScale = new Vector3(
                _originalScale.x * Mathf.Sign(direction.x),
                _originalScale.y,
                _originalScale.z
            );
        }
    }

    private void EnemyBack()
    {
        Vector2 direction = (transform.position - _player_Pos.position).normalized;
        _rb.linearVelocity = new Vector2(direction.x * _backSpeed, _rb.linearVelocity.y);
    }
    #endregion

    //�������̗͊֘A������
    #region
    public float GetEnemyHp()
    {
        return _enemyHp;
    }

    public void TakeEnemyDamage(float damage)
    {
        _enemyHp -= damage;
        Debug.Log("�G�̎c��HP: " + _enemyHp);
        if (_enemyHp <= 0) 
        {
            Die();
        }
    }

    private void Heal() 
    {
        //�q�[��
        _enemyHp += _potionHeal;
        if (_enemyHp > _enemyHealValue) 
        {
            _enemyHp = _enemyMaxHp;
        }
        ChangeMoveState(EnemyMoveState.Walk);
        Debug.Log(_enemyHp);
    }

    private void Die() 
    {
        Debug.Log(_daijoubu);
        Destroy(gameObject);
    }
    #endregion

    //�������U���֘A������
    #region
    /// <summary>
    /// �U���̃N�[���^�C�����擾
    /// </summary>
    /// <param name="State">EnemyAttackState�����</param>
    /// <param name="attackJudge">�v�Z�������Ԃ�ۑ��������(float)</param>
    /// <param name="attackCollTime">�҂��Ԃ����(float)</param>
    private void CoolTime(float attackCollTime)
    {
        _enemyAtkCoolTime += Time.deltaTime / attackCollTime;
        //�����Ɠ���
    }
    private void Attack()
    {
        _enemyFinalDmg = _enemyNormalAtk_Dmg;
        StartCoroutine(DoEnemyNormalAttack());
    }

    private void HardAttack() 
    {

    }
    #endregion

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D: " + other.name);
        if (_NormalAtk_col.enabled) // �U�����肪ON�̂Ƃ�����
        {
            if (other.CompareTag("Player"))
            {
                Player player = other.GetComponentInParent<Player>();
                if (player != null)
                {
                    player.TakePlayerDamage(_enemyFinalDmg); // �v���C���[��HP�����
                    Debug.Log("�����Ђ�[�[�[�[�[�[�[�[�[�[�[�[�[�[");
                    ChangeAttackState(EnemyAttackState.NotAttack);
                }
            }
        }
    }

    private IEnumerator DoEnemyNormalAttack()
    {
        Debug.Log("�U���̃R���C�_�[���I��");
        _NormalAtk_col.enabled = true;

        yield return new WaitForSeconds(0.3f);

        _NormalAtk_col.enabled = false;

        ChangeAttackState(EnemyAttackState.NotAttack);
    }
}