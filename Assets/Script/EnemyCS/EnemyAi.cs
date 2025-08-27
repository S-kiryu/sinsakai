using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class EnemyAi : MonoBehaviour
{
    private string  _daijoubu = "���v�����Ȃ�";

    //�������G�̏�񁁁���
    [Tooltip("�v���C���[�����m���鋗��")]
    [SerializeField]
    private float _detectionRange = 5f;
    

    //�������G�̃X�e�[�^�X������
    [SerializeField]
    private float _enemyHp = 1528.0f;
    [SerializeField]
    [Tooltip("�G���񕜂��s���l")]
    private float _enemyHealValue = 509.0f;
    [SerializeField]
    private int _potion = 1;
    [SerializeField]
    [Tooltip("�|�[�V�������g�����Ƃ��ɉ񕜂���l")]
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
    private EnemyAttackStare _enemyAttackState;
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
    private float _enemyNormalAttack_Dmg = 10;
    [SerializeField]
    private float _enemyNormalAttack_Time = 10f;
    private float _enemyAttackCoolTime = 0; //0�`1�Ŕ��f
    private bool _enemyAttackStateEnter = true;


    public enum EnemyAttackStare
    {
        NotAttack,
        Attack,
        HardAttack,
    }

    void ChangeAttackState(EnemyAttackStare newAttackState) 
    {
        _enemyAttackState = newAttackState;

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _enemyMoveState = EnemyMoveState.Idle;
        _enemyAttackState = EnemyAttackStare.NotAttack;
        _player_Pos = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector2.Distance(transform.position, _player_Pos.position);
        AtaackCollTime();

        switch (_enemyMoveState)
        {
            case EnemyMoveState.Idle:
                Idle();
                if (distance < _detectionRange)
                {
                    Debug.Log("�v���C���[�𔭌��I");
                    ChangeMoveState(EnemyMoveState.Walk);
                }
                break;

            case EnemyMoveState.Walk:
                Walk();
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
    }

    //�������ړ��֘A������
    private void Idle() 
    {
        
    }

    private void Walk()
    {
        Vector2 direction = (_player_Pos.position - transform.position).normalized;

        // ���������̈ړ�����
        _rb.linearVelocity = new Vector2(direction.x * _moveSpeed, _rb.linearVelocity.y);

        // ���E�𔽓]������
        if (direction.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
        }
    }

    private void EnemyBack()
    {

    }

    //�������̗͊֘A������
    public float GetEnemyHp()
    {
        return _enemyHp;
    }

    public void TakePlayerDamage(float damage)
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
        ChangeMoveState(EnemyMoveState.Walk);
        Debug.Log(_enemyHp);
    }

    private void Die() 
    {
        Debug.Log(_daijoubu);
        Destroy(gameObject);
    }

    //�������U���֘A������

    private void AtaackCollTime() 
    {
        _enemyAttackCoolTime += Time.deltaTime;
        if (_enemyAttackCoolTime >= _enemyNormalAttack_Time)
        {
            Debug.Log("�\�b������");
            _enemyAttackCoolTime = 0;
        }
    }

    private void Attack()
    {

    }

    private void HardAttack() 
    {

    }


}