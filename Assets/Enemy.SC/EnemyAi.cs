using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class EnemyAi : MonoBehaviour
{
    private string  Daijoubu = "���v�����Ȃ�";

    //�������G�̏�񁁁���
    [Tooltip("�v���C���[�����m���鋗��")]
    private float detectionRange = 5f;
    private bool isMovingBack = true;

    //�������G�̃X�e�[�^�X������
    [SerializeField]
    private float enemyHp = 1528.0f;
    [SerializeField]
    [Tooltip("�G���񕜂��s���l")]
    private float enemyHealValue = 509.0f;
    [SerializeField]
    private int potion = 1;
    [SerializeField]
    [Tooltip("�|�[�V�������g�����Ƃ��ɉ񕜂���l")]
    private float potionHeal = 1000.0f;
    private float moveSpeed = 2f;

//�o�b�N�̓���
    [Tooltip("��ރX�s�[�h")]
    [SerializeField]
    private float backSpeed = 5f;

    private Transform player_Pos;
    private EnemyState _enemyState;


    public enum EnemyState
    {
        Idle,
        Walk,
        Dash,
        Back,
        Heal,
        Attack,
        HardAttack,

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _enemyState = EnemyState.Idle;
        player_Pos = GameObject.FindGameObjectWithTag("Player").transform;
        enemyHp = 100;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector2.Distance(transform.position, player_Pos.position);

        switch (_enemyState)
        {
            case EnemyState.Idle:
                Idle();
                if (distance < detectionRange)
                {
                    Debug.Log("�v���C���[�𔭌��I");
                    _enemyState = EnemyState.Walk;
                }
                break;

            case EnemyState.Walk:
                Walk();
                Debug.Log("�����Ă�I�I�I");
                
                if (enemyHp <= enemyHealValue && potion > 0) 
                {
                    potion--;
                    //�|�[�V����������
                    _enemyState = EnemyState.Heal;
                }                
                break;

            case EnemyState.Back:
                EnemyBack();
                break;


            case EnemyState.Heal:
                isMovingBack = false;
                if (isMovingBack == true) 
                {
                    Debug.Log("�񕜂��Ă�I�I�I");
                    Heal(); 
                }
                break;

            case EnemyState.Attack:
                Attack();
                Debug.Log("�U���I�I�I");
                break;

            case EnemyState.HardAttack:
                HardAttack();
                Debug.Log("���U���I�I�I");
                break;

            case EnemyState.Dash:

                break;
        }
    }

    //�������ړ��֘A������
    private void Idle() 
    {

    }

    private void Walk() 
    {
        Vector2 direction = (player_Pos.position - transform.position).normalized;

        // �G���ړ�������
        transform.position += moveSpeed * Time.deltaTime * (Vector3)direction;

        // ���E�𔽓]������iSprite�̏ꍇ�j
        if (direction.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
        }
    }

    private void EnemyBack()
    {

    }

    //�������̗͊֘A������
    public float GetHp()
    {
        return enemyHp;
    }

    public void TakeDamage(float damage)
    {
        enemyHp -= damage;
        Debug.Log("�G�̎c��HP: " + enemyHp);
        if (enemyHp <= 0) 
        {
            Die();
        }
    }

    private void Heal() 
    {
        //�q�[��
        enemyHp += potionHeal;
        _enemyState = EnemyState.Walk;
        Debug.Log(enemyHp);
    }

    private void Die() 
    {
        Debug.Log(Daijoubu);
        Destroy(gameObject);
    }

    //�������U���֘A������
    private void Attack()
    {

    }

    private void HardAttack() 
    {

    }
}