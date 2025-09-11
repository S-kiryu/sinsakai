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
    private bool _isKnockback = false;

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
    private float _enemyHardAtk_Dmg = 20;
    [SerializeField]
    private float _enemyNormalAtk_Time = 1f;
    [SerializeField]
    private Collider2D _NormalAtk_col;
    [SerializeField]
    private Collider2D _HardAtk_col;

    private float _enemyFinalDmg;
    private bool _enemyAtkStateEnter = true;
    //アニメーション
    private Animator anim = null;

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
        PreAtkState,
        Attack,
        HardAttack,
    }

    public enum PriorityType
    {
        NormalAtk,
        HardAtk,
    }

    class Priority
    {
        public PriorityType _type { get; private set; }
        public float value;

        public Priority(PriorityType type)
        {
            _type = type;
            value = 0f;
        }
    }

    class Priortys
    {
        public List<Priority> PriorityList { get; private set; } = new List<Priority>();

        public Priority GetPriority(PriorityType Type)
        #region
        {
            foreach (Priority priority in PriorityList)
            {
                if (priority._type == Type)
                {
                    return priority;
                }
            }
            return null;
        }
        #endregion

        public void SortPriority()
        #region
        {
            PriorityList.Sort((Priority1, Priority2) => Priority2.value.CompareTo(Priority1.value));
        }
        #endregion

        //コンストラクタ
        public Priortys()
        #region
        {
            //列挙型を配列に変更し、Lenghthを使えるようにする
            int priortyNum = System.Enum.GetNames(typeof(PriorityType)).Length;
            for (int i = 0; i < priortyNum; i++)
            {
                //列挙型は配列ではないのでインデックスから参照できない
                PriorityType type = (PriorityType)System.Enum.ToObject(typeof(PriorityType), i);
                Priority newPriorty = new Priority(type);

                PriorityList.Add(newPriorty);
            }
        }
        #endregion
    }

    Priortys prioritys = new Priortys();

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
        anim = GetComponent<Animator>();
        _enemyMoveState = EnemyMoveState.Idle;
        _enemyAtkState = EnemyAttackState.AttackIdle;
        _player_Pos = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distance = Vector2.Distance(transform.position, _player_Pos.position);

        HandleMoveState(distance);
        HandleAttackState();
    }

    //＝＝＝移動関連＝＝＝
    #region
    private void HandleMoveState(float distance)
    {
        switch (_enemyMoveState)
        {
            case EnemyMoveState.Idle:
                Idle();
                if (distance < _detectionRange && _enemyAtkState != EnemyAttackState.Attack)
                {
                    Debug.Log("プレイヤーを発見！");
                    ChangeMoveState(EnemyMoveState.Walk);
                    anim.SetBool("move", true);
                }
                break;

            case EnemyMoveState.Walk:
                //もし攻撃中でなければ歩く
                if (_enemyAtkState != EnemyAttackState.Attack && _enemyAtkState != EnemyAttackState.PreAtkState)
                {
                    Walk();
                }

                Debug.Log("歩いてる！！！");

                //体力が一定の値を超えたら回復
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
                Debug.Log("回復してる！！！");
                Heal();
                break;

            //走る動作を
            case EnemyMoveState.Dash:

                break;
        }
    }


    private void Idle()
    {
        if (!_isKnockback)
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
        }
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

    public void TakeEnemyDamage(float damage, Vector2 attackerPosition)
    {
        _enemyHp -= damage;
        Debug.Log("敵の残りHP: " + _enemyHp);

        // ノックバック処理
        float directionX = Mathf.Sign(transform.position.x - attackerPosition.x);
        Vector2 knockbackDir = new Vector2(directionX, 0.5f).normalized;

        // ノックバックフラグをON
        _isKnockback = true;

        // いったん速度リセットしてからノックバック
        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(knockbackDir * 20f, ForceMode2D.Impulse);

        // 一時的に移動を停止
        StartCoroutine(DisableMovement(0.2f));

        if (_enemyHp <= 0)
        {
            Die();
        }
    }

    private IEnumerator DisableMovement(float duration)
    {
        ChangeAttackState(EnemyAttackState.AttackIdle);
        ChangeMoveState(EnemyMoveState.Idle);

        yield return new WaitForSeconds(duration);

        // ノックバックフラグをOFF
        _isKnockback = false;

        float distance = Vector2.Distance(transform.position, _player_Pos.position);

        if (distance < _detectionRange)
        {
            ChangeMoveState(EnemyMoveState.Walk);
            anim.SetBool("move", true);
        }
        else
        {
            ChangeMoveState(EnemyMoveState.Idle);
            anim.SetBool("move", false);
        }
    }


    private void Heal()
    {
        //ヒール
        _enemyHp += _potionHeal;
        ChangeMoveState(EnemyMoveState.Walk);
        _enemyHp = Mathf.Min(_enemyHp, _enemyMaxHp); // この行が抜けていた

        Debug.Log("回復完了！回復後HP: " + _enemyHp);
    }

    private void Die()
    {
        Debug.Log(_daijoubu);
        Destroy(gameObject);
    }
    #endregion

    //＝＝＝攻撃関連＝＝＝
    #region


    private void HandleAttackState()
    {
        float distance = Vector2.Distance(transform.position, _player_Pos.position);
        float attackRange = 3.0f;

        // デバッグ用ログ追加
        Debug.Log($"攻撃状態: {_enemyAtkState}, 移動状態: {_enemyMoveState}");

        //攻撃のクールタイムの計算
        CoolTime(PriorityType.NormalAtk, _enemyNormalAtk_Time);

        switch (_enemyAtkState)
        {
            case EnemyAttackState.AttackIdle:
                // プレイヤーが攻撃範囲内にいて、移動状態がWalkの時のみ攻撃準備
                if (distance <= attackRange && _enemyMoveState == EnemyMoveState.Walk)
                {
                    ChangeAttackState(EnemyAttackState.PreAtkState);
                }
                break;

            case EnemyAttackState.PreAtkState:
                if (_enemyAtkStateEnter)
                {
                    _enemyAtkStateEnter = false;
                    Debug.Log("攻撃を準備中");

                    // 0.5秒後に攻撃実行
                    StartCoroutine(DelayedAttack(0.5f));
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
                if (_enemyAtkStateEnter)
                {
                    _enemyAtkStateEnter = false;
                    HardAttack();
                }
                break;
        }
    }

    // 攻撃準備のための遅延コルーチン
    private IEnumerator DelayedAttack(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_enemyAtkState == EnemyAttackState.PreAtkState)
        {
            float distance = Vector2.Distance(transform.position, _player_Pos.position);
            if (distance <= 3.0f) // 2.0fから3.0fに変更
            {
                ChangeAttackState(EnemyAttackState.Attack);
            }
            else
            {
                ChangeAttackState(EnemyAttackState.AttackIdle);
            }
        }
    }

    /// <summary>
    /// 攻撃のクールタイムを取得
    /// </summary>
    /// <param name="State">EnemyAttackStateを入力</param>
    /// <param name="attackJudge">計算した時間を保存する引数(float)</param>
    /// <param name="attackCollTime">待つ時間を入力(float)</param>
    private void CoolTime(PriorityType type, float attackCollTime)
    {
        Priority priority = prioritys.GetPriority(type);
        if (priority != null)
        {
            priority.value += Time.deltaTime / attackCollTime;
        }
    }

    private void Attack()
    {
        //Debug.Log("攻撃開始: 赤色に変更");
        //GetComponent<SpriteRenderer>().color = Color.red;
        //Debug.Log("ダメージを代入");
        //_enemyFinalDmg = _enemyNormalAtk_Dmg;
        //StartCoroutine(DoEnemyNormalAttack());
        Debug.Log("通常攻撃開始");
        GetComponent<SpriteRenderer>().color = Color.red;

        // ダメージを設定
        _enemyFinalDmg = _enemyNormalAtk_Dmg;

        // アニメーションをトリガー
        anim.SetTrigger("NormalAttack");
    }

    private void HardAttack()
    {
        Debug.Log("強攻撃開始");

        // ダメージを設定
        _enemyFinalDmg = _enemyHardAtk_Dmg;

        // アニメーションをトリガー
        anim.SetTrigger("HardAttack");
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D: " + other.name);
        if (_NormalAtk_col.enabled)
        {
            if (other.CompareTag("PlayerHit"))
            {
                Player player = other.GetComponentInParent<Player>();
                if (player != null)
                {
                    player.TakePlayerDamage(_enemyFinalDmg);
                    Debug.Log("うっひょーーーーーーーーーーーーーー");
                    // ChangeAttackState(EnemyAttackState.PreAtkState); ←この行を削除！
                }
            }
        }
    }

    //＝＝＝アニメーションイベント用メソッド＝＝＝
    #region Animation Events

    // 攻撃判定を有効化（アニメーションの攻撃タイミングで呼ばれる）
    public void EnableAttackCollider()
    {
        Debug.Log("攻撃判定ON（アニメーションイベント）");

        // 現在の攻撃タイプに応じてコライダーを有効化
        if (_enemyAtkState == EnemyAttackState.Attack)
        {
            _NormalAtk_col.enabled = true;
        }
        else if (_enemyAtkState == EnemyAttackState.HardAttack)
        {
            _HardAtk_col.enabled = true;
        }
    }

    // 攻撃判定を無効化
    public void DisableAttackCollider()
    {
        Debug.Log("攻撃判定OFF（アニメーションイベント）");
        _NormalAtk_col.enabled = false;
        _HardAtk_col.enabled = false;
    }

    // 攻撃アニメーション終了時に呼ばれる
    public void OnAttackAnimationEnd()
    {
        Debug.Log("攻撃アニメーション終了");
        GetComponent<SpriteRenderer>().color = Color.white;

        // 攻撃状態をリセット
        ChangeAttackState(EnemyAttackState.AttackIdle);
    }

    #endregion

    //private IEnumerator DoEnemyNormalAttack()
    //{
    //    Debug.Log("攻撃のコライダーをオン");
    //    _NormalAtk_col.enabled = true;

    //    yield return new WaitForSeconds(0.3f);

    //    _NormalAtk_col.enabled = false;

    //    GetComponent<SpriteRenderer>().color = Color.white;

    //    // 攻撃完了後は一度Idleに戻す（重要！）
    //    ChangeAttackState(EnemyAttackState.AttackIdle);
    //}

    //private IEnumerator DoEnemyHardAttack()
    //{
    //    Debug.Log("攻撃のコライダーをオン");
    //    _HardAtk_col.enabled = true;
    //    yield return new WaitForSeconds(0.3f);
    //    _HardAtk_col.enabled = false;
    //    ChangeAttackState(EnemyAttackState.AttackIdle); // PreAtkStateから変更
    //}
    #endregion
}