using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class EnemyAi : MonoBehaviour
{
    private string  Daijoubu = "大丈夫だ問題ない";

    //＝＝＝敵の情報＝＝＝
    [Tooltip("プレイヤーを検知する距離")]
    private float detectionRange = 5f;
    private bool isMovingBack = true;

    //＝＝＝敵のステータス＝＝＝
    [SerializeField]
    private float enemyHp = 1528.0f;
    [SerializeField]
    [Tooltip("敵が回復を行う値")]
    private float enemyHealValue = 509.0f;
    [SerializeField]
    private int potion = 1;
    [SerializeField]
    [Tooltip("ポーションを使ったときに回復する値")]
    private float potionHeal = 1000.0f;
    private float moveSpeed = 2f;

//バックの動作
    [Tooltip("後退スピード")]
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
                    Debug.Log("プレイヤーを発見！");
                    _enemyState = EnemyState.Walk;
                }
                break;

            case EnemyState.Walk:
                Walk();
                Debug.Log("歩いてる！！！");
                
                if (enemyHp <= enemyHealValue && potion > 0) 
                {
                    potion--;
                    //ポーションを消費
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
                    Debug.Log("回復してる！！！");
                    Heal(); 
                }
                break;

            case EnemyState.Attack:
                Attack();
                Debug.Log("攻撃！！！");
                break;

            case EnemyState.HardAttack:
                HardAttack();
                Debug.Log("強攻撃！！！");
                break;

            case EnemyState.Dash:

                break;
        }
    }

    //＝＝＝移動関連＝＝＝
    private void Idle() 
    {

    }

    private void Walk() 
    {
        Vector2 direction = (player_Pos.position - transform.position).normalized;

        // 敵を移動させる
        transform.position += moveSpeed * Time.deltaTime * (Vector3)direction;

        // 左右を反転させる（Spriteの場合）
        if (direction.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
        }
    }

    private void EnemyBack()
    {

    }

    //＝＝＝体力関連＝＝＝
    public float GetHp()
    {
        return enemyHp;
    }

    public void TakeDamage(float damage)
    {
        enemyHp -= damage;
        Debug.Log("敵の残りHP: " + enemyHp);
        if (enemyHp <= 0) 
        {
            Die();
        }
    }

    private void Heal() 
    {
        //ヒール
        enemyHp += potionHeal;
        _enemyState = EnemyState.Walk;
        Debug.Log(enemyHp);
    }

    private void Die() 
    {
        Debug.Log(Daijoubu);
        Destroy(gameObject);
    }

    //＝＝＝攻撃関連＝＝＝
    private void Attack()
    {

    }

    private void HardAttack() 
    {

    }
}