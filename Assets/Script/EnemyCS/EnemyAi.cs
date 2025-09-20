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
    [SerializeField]private float _detectionRange = 5f;
    private Vector3 _originalScale;
    private bool _isKnockback = false;

    //＝＝＝敵のステータス＝＝＝
    [SerializeField]private float _enemyHp = 1528.0f;
    private float _enemyMaxHp;

    [Tooltip("敵が回復を行う値")]
    [Header("敵が回復を行う値")]
    [SerializeField]private float _enemyHealValue = 509.0f;

    [Header("回復できる数")]
    [SerializeField]private int _potion = 1;

    [Tooltip("回復値")]
    [Header("回復値")]
    [SerializeField]private float _potionHeal = 1000.0f;
    private float _moveSpeed = 2f;

    [SerializeField]private float _attackRange = 2.5f; // 攻撃したい距離
    [SerializeField]private float _safeRange = 1.5f;   // 近すぎると後退する距離

    //＝＝＝バックの動作＝＝＝
    [Tooltip("後退スピード")]
    [SerializeField]private float _backSpeed = 5f;
    private bool _isMovingBack = false; // 修正: trueからfalseに変更

    private Transform _player_Pos;
    private Rigidbody2D _rb;
    private EnemyMoveState _enemyMoveState;
    private EnemyAttackState _enemyAtkState;
    private bool _enemyMoveStateEnter = true;

    private float _backCheckInterval = 0.5f;  // 判定間隔（秒）
    private float _backCheckTimer = 0f;       // タイマー
    private float _backChance = 0.1f;

    //＝＝＝敵の攻撃関連＝＝＝
    [SerializeField]private float _enemyNormalAtk_Dmg = 10;
    [SerializeField]private float _enemyHardAtk_Dmg = 20;
    [SerializeField]private float _enemyNormalAtk_Time = 1f;
    [SerializeField]private Collider2D _NormalAtk_col;
    [SerializeField] private Collider2D _HardAtk_col;

    private float _enemyFinalDmg;
    private bool _enemyAtkStateEnter = true;
    private bool _isAttackCooling = false;

    [Header("攻撃距離設定")]
    [SerializeField] private float closeRangeDistance = 3.0f;  // 近距離攻撃の範囲
    [SerializeField] private float longRangeDistance = 8.0f;   // 遠距離攻撃の範囲
    // EnemyAiクラスに追加するフィールド（既存のフィールドの近くに配置）

    [Header("羽攻撃設定")]
    [SerializeField] private GameObject[] featherPrefabs = new GameObject[2]; // 2種類の羽プレハブ
    [SerializeField] private float featherSpawnInterval = 0.5f; // スポーン間隔
    [SerializeField] private int feathersPerWave = 5; // 1回に出す羽の数
    [SerializeField] private float featherSpeed = 8f; // 羽の飛行速度
    [SerializeField] private float featherLifetime = 3f; // 羽の生存時間
    [SerializeField] private float featherAttackDuration = 3f; // 攻撃継続時間
    [SerializeField] private float featherSpreadAngle = 45f; // 羽の散らばり角度

    private bool isFeatherAttacking = false;

    //アニメーション
    private Animator anim = null;

    [SerializeField] private GameObject spearPrefab; // 槍プレハブ
    [SerializeField] private float spearOffset = 1.5f; // プレイヤーの後ろに出す距離
    [SerializeField] private float spearLifetime = 1.0f; // 槍の存在時間

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
        WarpAttack,
        HaneAttack,
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
        if (_player_Pos != null)
        {
            float distance = Vector2.Distance(transform.position, _player_Pos.position);
            // 距離判定処理
            HandleMoveState(distance);
            HandleAttackState();
        }
        else
        {
            // プレイヤーが消えたときの処理（敵をIdleにするとか）
            ChangeMoveState(EnemyMoveState.Idle);
            anim.SetBool("move", false);
        }
    }

    //＝＝＝移動関連＝＝＝
    #region
    private void HandleMoveState(float distance)
    {
        // 攻撃中は強制的に Idle にする
        if (_enemyAtkState == EnemyAttackState.HardAttack ||
            _enemyAtkState == EnemyAttackState.HaneAttack ||
            _enemyAtkState == EnemyAttackState.WarpAttack ||
            _enemyAtkState == EnemyAttackState.PreAtkState)
        {
            Idle();
            return;
        }

        switch (_enemyMoveState)
        {
            case EnemyMoveState.Idle:
                Idle();
                if (distance < _detectionRange)
                {
                    ChangeMoveState(EnemyMoveState.Walk);
                    anim.SetBool("move", true);
                }
                break;

            case EnemyMoveState.Walk:
                // 遠距離攻撃範囲外 → 近づく
                if (distance > longRangeDistance)
                {
                    Walk();
                }
                // 近すぎる → 後退
                else if (distance < _safeRange)
                {
                    Debug.Log("距離を取る");
                    ChangeMoveState(EnemyMoveState.Back);
                    anim.SetBool("move", true);
                }
                else
                {
                    // 攻撃レンジ内 → その場で止まる（攻撃準備に入る）
                    Idle();
                }

                EnemyBack();

                // HPが減ったら回復
                if (_enemyHp <= _enemyHealValue && _potion > 0)
                {
                    _potion--;
                    Debug.Log("ポーションを消費");
                    Heal();
                }
                break;

            case EnemyMoveState.Back:
                if (_enemyMoveStateEnter)
                {
                    _enemyMoveStateEnter = false;
                    _isMovingBack = true;
                    StartCoroutine(BackDuration(1.0f));
                }
                break;
        }
    }

    private IEnumerator BackDuration(float duration)
    {
        Debug.Log("後退開始");

        // 後退する
        _rb.linearVelocity = new Vector2(-Mathf.Sign(transform.localScale.x) * _backSpeed, 0);

        yield return new WaitForSeconds(duration);

        _isMovingBack = false; // 修正: バック終了
        Debug.Log("後退終了");

        // 後退後にワープ攻撃抽選 - 修正: 条件を簡略化
        if (!_isAttackCooling && spearPrefab != null) // 修正: spearPrefabがnullでないかチェック
        {
            float warpChance = 0.7f; // 修正: 70%の確率に変更
            if (Random.value < warpChance)
            {
                Debug.Log("後退後にワープ攻撃！");
                ChangeAttackState(EnemyAttackState.WarpAttack);
                yield break; // 攻撃するのでここで終了
            }
        }

        // ワープ攻撃しない場合はWalkに戻る
        Debug.Log("ワープ攻撃なし、Walk状態に戻る");
        ChangeMoveState(EnemyMoveState.Walk);
        anim.SetBool("move", true);
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
        // 修正: バック中やAttackCooling中は判定しない
        if (_isMovingBack || _isAttackCooling) return;

        _backCheckTimer += Time.deltaTime;
        if (_backCheckTimer >= _backCheckInterval)
        {
            _backCheckTimer = 0f;

            if (_enemyMoveState == EnemyMoveState.Walk)
            {
                if (Random.value < _backChance)
                {
                    Debug.Log("ランダム判定で後退！");
                    ChangeMoveState(EnemyMoveState.Back);
                }
            }
        }
        // 修正: else文を削除（常にWalk状態に戻すのは間違い）
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
        // HP減少
        _enemyHp -= damage;
        Debug.Log("敵の残りHP: " + _enemyHp);

        // ダメージ色変更（Coroutineで戻す）
        StartCoroutine(DamageFlash());

        // ノックバック処理
        float directionX = Mathf.Sign(transform.position.x - attackerPosition.x);
        Vector2 knockbackDir = new Vector2(directionX, 0.5f).normalized;

        _isKnockback = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(knockbackDir * 20f, ForceMode2D.Impulse);

        // 一時的に移動を停止
        StartCoroutine(DisableMovement(0.2f));

        // HPが0以下なら死亡処理
        if (_enemyHp <= 0)
        {
            Die();
        }
    }


    private IEnumerator DamageFlash()
    {
        var sr = GetComponent<SpriteRenderer>();
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f); // 赤くなる時間
        sr.color = Color.white;
    }

    private IEnumerator DisableMovement(float duration)
    {
        ChangeAttackState(EnemyAttackState.AttackIdle);
        ChangeMoveState(EnemyMoveState.Idle);

        yield return new WaitForSeconds(duration);

        // ノックバックフラグをOFF
        _isKnockback = false;

        if (_player_Pos != null)
        {
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

    private IEnumerator AttackCooldown(float time)
    {
        _isAttackCooling = true;
        yield return new WaitForSeconds(time);
        _isAttackCooling = false;
        Debug.Log("攻撃クールダウン終了"); // 修正: デバッグログ追加
    }

    private void HandleAttackState()
    {
        float distance = Vector2.Distance(transform.position, _player_Pos.position);

        // 攻撃クールタイム進行（通常攻撃は削除したので、適当な値を使用）
        CoolTime(PriorityType.NormalAtk, 2.0f);

        switch (_enemyAtkState)
        {
            case EnemyAttackState.AttackIdle:
                // 攻撃範囲内でクールダウンが終了していれば攻撃判定
                if (!_isAttackCooling && distance <= longRangeDistance && _enemyMoveState == EnemyMoveState.Walk)
                {
                    DecideAttack();
                }
                break;

            case EnemyAttackState.PreAtkState:
                if (_enemyAtkStateEnter)
                {
                    _enemyAtkStateEnter = false;
                    Debug.Log("攻撃を準備中");
                    StartCoroutine(DelayedAttack(0.5f));
                }
                break;

            case EnemyAttackState.Attack:
                // 通常攻撃は削除されたので、この状態は使用しない
                Debug.LogWarning("通常攻撃状態は削除されました");
                ChangeAttackState(EnemyAttackState.AttackIdle);
                break;

            case EnemyAttackState.HardAttack:
                if (_enemyAtkStateEnter)
                {
                    _enemyAtkStateEnter = false;
                    HardAttack();
                }
                break;

            case EnemyAttackState.WarpAttack:
                if (_enemyAtkStateEnter)
                {
                    _enemyAtkStateEnter = false;
                    WarpAttack();
                }
                break;

            case EnemyAttackState.HaneAttack:
                if (_enemyAtkStateEnter)
                {
                    _enemyAtkStateEnter = false;
                    HaneAttack();
                }
                break;
        }
    }

    private void DecideAttack()
    {
        float distance = Vector2.Distance(transform.position, _player_Pos.position);

        Debug.Log($"プレイヤーとの距離: {distance}");

        // 距離に応じて攻撃を選択
        if (distance <= closeRangeDistance)
        {
            // 近距離 → 強攻撃
            Debug.Log("近距離攻撃を選択：強攻撃");
            ChangeAttackState(EnemyAttackState.HardAttack);
        }
        else if (distance <= longRangeDistance)
        {
            // 遠距離 → 羽攻撃
            Debug.Log("遠距離攻撃を選択：羽攻撃");
            ChangeAttackState(EnemyAttackState.HaneAttack);
        }
        else
        {
            // 攻撃範囲外 → 攻撃しない
            Debug.Log("攻撃範囲外");
            ChangeAttackState(EnemyAttackState.AttackIdle);
        }
    }

    // 攻撃準備のための遅延コルーチン
    private IEnumerator DelayedAttack(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_enemyAtkState == EnemyAttackState.PreAtkState)
        {
            if (_player_Pos != null)
            {
                float distance = Vector2.Distance(transform.position, _player_Pos.position);
                if (distance <= 3.0f) // 2.0fから3.0fに変更
                {
                    ChangeAttackState(EnemyAttackState.HardAttack);
                }
                else
                {
                    ChangeAttackState(EnemyAttackState.AttackIdle);
                }
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
        Debug.Log("通常攻撃開始");
        // ダメージを設定
        _enemyFinalDmg = _enemyNormalAtk_Dmg;

        // アニメーションをトリガー
        anim.SetTrigger("normalAttack");
    }

    private void HardAttack()
    {
        Debug.Log("強攻撃開始");

        // ダメージを設定
        _enemyFinalDmg = _enemyHardAtk_Dmg;

        //アニメーションを再生
        anim.SetBool("hardAtk", true);
    }

    public void WarpAttack()
    {
        Debug.Log("ワープ攻撃開始");
        anim.SetBool("warpAtk", true);
    }

    public void HaneAttack()
    {
        Debug.Log("羽攻撃アニメーション開始");
        anim.SetTrigger("hane");
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D: " + other.name);
        if (_NormalAtk_col.enabled || _HardAtk_col.enabled)
        {
            if (other.CompareTag("PlayerHit"))
            {
                Player player = other.GetComponentInParent<Player>();
                if (player != null)
                {
                    player.TakePlayerDamage(_enemyFinalDmg);
                    Debug.Log("うっひょーーーーーーーーーーーーーー");
                }
            }
        }
    }

    //＝＝＝アニメーションイベント用メソッド＝＝＝
    #region Animation Events

    // 攻撃判定を有効化（アニメーションの攻撃タイミングで呼ばれる）
    #region
    // 通常攻撃用
    public void EnableNormalAttackCollider()
    {
        Debug.Log("通常攻撃判定ON");
        _NormalAtk_col.enabled = true;
    }

    public void DisableNormalAttackCollider()
    {
        Debug.Log("通常攻撃判定OFF");
        _NormalAtk_col.enabled = false;
    }

    // 強攻撃用
    public void EnableHardAttackCollider()
    {
        Debug.Log("強攻撃判定ON");
        _HardAtk_col.enabled = true;
    }

    //ワープ攻撃
    public void DisableHardAttackCollider()
    {
        Debug.Log("強攻撃判定OFF");
        _HardAtk_col.enabled = false;
    }

    public void EnableWarpAttackCollider()
    {
        Debug.Log("ワープ攻撃だよ");
        if (_player_Pos == null) return;

        if (spearPrefab == null)
        {
            Debug.LogWarning("spearPrefabが設定されていません！");
            return;
        }

        float playerDir = _player_Pos.localScale.x > 0 ? 1 : -1;

        // プレイヤーの「後ろ」側を計算
        Vector2 spawnPos = _player_Pos.position - new Vector3(playerDir * spearOffset, 0, 0);

        GameObject spear = Instantiate(spearPrefab, spawnPos, Quaternion.identity);

        // 槍をプレイヤーの向きに合わせて反転
        spear.transform.localScale = new Vector3(-playerDir, 1, 1);

        // 一定時間後に爆発コルーチン開始
        StartCoroutine(DestroySpearAfterDelay(spear, spearLifetime));
    }

    //羽攻撃
    public void EnableHaneAttackCollider()
    {
        Debug.Log("羽攻撃開始 - アニメーションイベントから呼び出し");

        // 既に攻撃中の場合は重複実行を防ぐ
        if (isFeatherAttacking)
        {
            Debug.Log("既に羽攻撃実行中のため、重複実行をキャンセル");
            return;
        }

        // プレイヤーが存在するかチェック
        if (_player_Pos == null)
        {
            Debug.LogWarning("プレイヤーが見つからないため羽攻撃をキャンセル");
            return;
        }

        // 羽プレハブが設定されているかチェック
        if (featherPrefabs == null || featherPrefabs.Length == 0)
        {
            Debug.LogWarning("羽プレハブが設定されていません！");
            return;
        }

        // 羽攻撃シーケンスを開始
        StartCoroutine(FeatherAttackSequence());
    }

    #endregion

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
        anim.SetBool("hardAtk", false);
        anim.SetBool("warpAtk", false);
        // 羽攻撃用のアニメーターパラメータもリセット（必要に応じて）
        // anim.SetBool("haneAtk", false);

        // 羽攻撃が継続中の場合は終了を待つ
        if (isFeatherAttacking)
        {
            StartCoroutine(WaitForFeatherAttackEnd());
        }
        else
        {
            StartCoroutine(AttackCooldown(2.0f)); // クールダウン時間を調整
            ResumeMovement();
        }
    }

    private IEnumerator WaitForFeatherAttackEnd()
    {
        while (isFeatherAttacking)
        {
            yield return new WaitForSeconds(0.1f);
        }

        StartCoroutine(AttackCooldown(2.0f));
        ResumeMovement();
    }

    #endregion

    private IEnumerator DestroySpearAfterDelay(GameObject spear, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (spear != null)
        {
            // 爆発エフェクトを出したい場合
            // Instantiate(explosionPrefab, spear.transform.position, Quaternion.identity);
            //Destroy(spear); // 修正: 槍の破棄処理を追加
        }
    }

    private IEnumerator FeatherAttackSequence()
    {
        isFeatherAttacking = true;
        float attackTimer = 0f;

        Debug.Log("羽攻撃シーケンス開始");

        while (attackTimer < featherAttackDuration)
        {
            // 羽を生成
            SpawnFeathers();

            // 次の生成まで待機
            yield return new WaitForSeconds(featherSpawnInterval);
            attackTimer += featherSpawnInterval;
        }

        isFeatherAttacking = false;
        Debug.Log("羽攻撃シーケンス終了");
    }


    // 羽を生成するメソッド
    private void SpawnFeathers()
    {
        if (_player_Pos == null) return;

        Debug.Log($"羽を{feathersPerWave}個生成");

        for (int i = 0; i < feathersPerWave; i++)
        {
            // ランダムに羽の種類を選択
            int featherType = Random.Range(0, featherPrefabs.Length);

            if (featherPrefabs[featherType] == null)
            {
                Debug.LogWarning($"羽プレハブ[{featherType}]が設定されていません！");
                continue;
            }

            // 敵の位置から羽を生成（少し上から）
            Vector2 spawnPos = transform.position + new Vector3(0, 1f, 0);

            GameObject feather = Instantiate(featherPrefabs[featherType], spawnPos, Quaternion.identity);

            // 羽の動作を設定
            FeatherProjectile featherScript = feather.GetComponent<FeatherProjectile>();
            if (featherScript == null)
            {
                featherScript = feather.AddComponent<FeatherProjectile>();
            }

            // プレイヤー方向にランダムな散らばりを加えた方向を計算
            Vector2 baseDirection = (_player_Pos.position - transform.position).normalized;
            float randomAngle = Random.Range(-featherSpreadAngle / 2f, featherSpreadAngle / 2f);
            Vector2 finalDirection = RotateVector2(baseDirection, randomAngle);

            featherScript.Initialize(finalDirection, featherSpeed, featherLifetime, 10f);
        }
    }

    private Vector2 RotateVector2(Vector2 vector, float angleDegrees)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRadians);
        float sin = Mathf.Sin(angleRadians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    private void ResumeMovement()
    {
        if (_player_Pos != null)
        {
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
    }
    #endregion
}