using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class EnemyAi : MonoBehaviour
{
    private string _daijoubu = "大丈夫だ問題ない";

    //＝＝＝敵の情報＝＝＝
    [Tooltip("プレイヤーを検知する距離")]
    [Header("プレイヤー感知距離")]
    [SerializeField]
    private float _detectionRange = 5f;
    private Vector3 _originalScale;


    //＝＝＝敵のステータス＝＝＝
    [SerializeField]
    private float _enemyHp = 1528.0f;
    private float _enemyMaxHp;

    [Tooltip("敵が回復を行う値")]
    [Header("敵が回復を行う値")]
    [SerializeField]
    private float _enemyHealValue = 509.0f;

    [Header("回復できる数")]
    [SerializeField]
    private int _potion = 1;

    [Tooltip("回復値")]
    [Header("回復値")]
    [SerializeField]
    private float _potionHeal = 1000.0f;
    private float _moveSpeed = 2f;

    //＝＝＝バックの動作＝＝＝
    [Tooltip("後退スピード")]
    [SerializeField]
    private float _backSpeed = 5f;
    private bool _isMovingBack = true;

    private Transform _player_Pos;
    private Rigidbody2D _rb;
    private EnemyMoveState _enemyMoveState;
    private EnemyAttackState _enemyAtkState;
    private bool _enemyMoveStateEnter = true;

    //＝＝＝敵の攻撃関連＝＝＝
    [SerializeField]
    private float _enemyNormalAtk_Dmg = 10;
    [SerializeField]
    private float _enemyNormalAtk_Time = 10f;
    [SerializeField]
    private Collider2D _NormalAtk_col;

    private float _enemyFinalDmg;
    private float _enemyAtkCoolTime = 0; //0～1で判断
    private bool _enemyAtkStateEnter = true;

    public enum EnemyMoveState
    {
        Idle,
        Walk,
        Dash,
        Back,
        Heal,
    }

    public enum EnemyAttackState
    {
        AttackIdle,
        NotAttack,
        Attack,
        HardAttack,
    }

    public enum AttackPriority 
    {
        Atk,
        HardAtk,
    }

    class Priority
    {
        public AttackPriority _priority { get; private set; }
        public float value;

        public Priority(AttackPriority priority) 
        {
            _priority = priority;
            value = 0f;
        }
    }

    class Priortys 
    {
        public List<Priority> desireList { get; private set; } = new List<Priority>();

        public Priortys() 
        {
            //列挙型を配列に変更し、Lenghthを使えるようにする
            int priortyNum = System.Enum.GetNames(typeof(AttackPriority)).Length;
            for (int i = 0; i < priortyNum; i++) 
            {
                //列挙型は配列ではないのでインデックスから参照できない
                AttackPriority priority = (AttackPriority)System.Enum.ToObject(typeof(AttackPriority), i);
                Priority newPriorty = new Priority(priority);

                desireList.Add(newPriorty);
            }
        }
    }

    void ChangeMoveState(EnemyMoveState newMoveState)
    {
        _enemyMoveState = newMoveState;
        _enemyMoveStateEnter = true;
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
                    Debug.Log("プレイヤーを発見！");
                    ChangeMoveState(EnemyMoveState.Walk);
                }
                break;

            case EnemyMoveState.Walk:
                if (_enemyAtkState != EnemyAttackState.Attack)
                {
                    Walk();
                }

                ChangeAttackState(EnemyAttackState.NotAttack);
                Debug.Log("歩いてる！！！");

                if (_enemyHp <= _enemyHealValue && _potion > 0)
                {
                    _potion--;
                    Debug.Log(" ポーションを消費");
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
                Debug.Log("回復してる！！！");
                Heal();
                //}

                break;

            case EnemyMoveState.Dash:

                break;
        }

        //攻撃のクールタイムの計算
        //優先度の設定
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
                    Debug.Log("攻撃を準備中");
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

    //＝＝＝移動関連＝＝＝
    #region
    private void Idle()
    {

    }

    private void Walk()
    {
        Vector2 direction = (_player_Pos.position - transform.position).normalized;

        // 水平方向の移動だけ
        _rb.linearVelocity = new Vector2(direction.x * _moveSpeed, _rb.linearVelocity.y);

        // 左右を反転させる（スケールを維持したまま）
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

    //＝＝＝体力関連＝＝＝
    #region
    public float GetEnemyHp()
    {
        return _enemyHp;
    }

    public void TakeEnemyDamage(float damage)
    {
        _enemyHp -= damage;
        Debug.Log("敵の残りHP: " + _enemyHp);
        if (_enemyHp <= 0)
        {
            Die();
        }
    }

    private void Heal()
    {
        //ヒール
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

    //＝＝＝攻撃関連＝＝＝
    #region
    /// <summary>
    /// 攻撃のクールタイムを取得
    /// </summary>
    /// <param name="State">EnemyAttackStateを入力</param>
    /// <param name="attackJudge">計算した時間を保存する引数(float)</param>
    /// <param name="attackCollTime">待つ時間を入力(float)</param>
    private void CoolTime(float attackCollTime)
    {
        _enemyAtkCoolTime += Time.deltaTime / attackCollTime;
        //ちゃんと動く
    }

    private void Attack()
    {
        _enemyFinalDmg = _enemyNormalAtk_Dmg;
        StartCoroutine(DoEnemyNormalAttack());
    }

    private void HardAttack()
    {

    }
   

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D: " + other.name);
        if (_NormalAtk_col.enabled) // 攻撃判定がONのときだけ
        {
            if (other.CompareTag("Player"))
            {
                Player player = other.GetComponentInParent<Player>();
                if (player != null)
                {
                    player.TakePlayerDamage(_enemyFinalDmg); // プレイヤーのHPを削る
                    Debug.Log("うっひょーーーーーーーーーーーーーー");
                    ChangeAttackState(EnemyAttackState.NotAttack);
                }
            }
        }
    }

    private IEnumerator DoEnemyNormalAttack()
    {
        Debug.Log("攻撃のコライダーをオン");
        _NormalAtk_col.enabled = true;

        yield return new WaitForSeconds(0.3f);

        _NormalAtk_col.enabled = false;

        ChangeAttackState(EnemyAttackState.NotAttack);
        
    }
    #endregion
}