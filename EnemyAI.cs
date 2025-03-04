using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("攻击设置")]
    [Tooltip("每次攻击造成的伤害值")]
    [SerializeField] private int damageAmount = 1;
    [Tooltip("攻击间隔时间(秒)")]
    [SerializeField] private float damageInterval = 1f;

    [Header("移动设置")]
    [Tooltip("移动速度")]
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("碰撞检测半径")]
    [SerializeField] private float collisionRadius = 1.5f;

    private Transform player;
    private NavMeshAgent agent;
    private SphereCollider sphereCollider;
    private Rigidbody enemyRigidbody;
    private bool isAttacking = false;
    private float lastDamageTime;

    void Start()
    {
        InitializeComponents();
    }

    /// <summary>
    /// 初始化敌人所需的所有组件
    /// </summary>
    void InitializeComponents()
    {
        // 初始化球形碰撞器
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
        }
        sphereCollider.radius = collisionRadius;
        sphereCollider.isTrigger = true;

        // 初始化刚体组件
        enemyRigidbody = GetComponent<Rigidbody>();
        if (enemyRigidbody == null)
        {
            enemyRigidbody = gameObject.AddComponent<Rigidbody>();
        }
        enemyRigidbody.isKinematic = true;
        enemyRigidbody.useGravity = false;

        // 初始化导航代理
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }

        // 查找玩家对象
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // 添加对Health组件死亡事件的监听
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.onDeath.AddListener(OnEnemyDeath);
        }

        // Debug.Log($"[EnemyAI] {gameObject.name} 初始化 - 伤害值: {damageAmount}, 伤害间隔: {damageInterval}");
    }

    void Update()
    {
        // 更新导航目标为玩家位置
        if (player != null && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }

    /// <summary>
    /// 当其他碰撞器停留在触发器内时触发
    /// </summary>
    void OnTriggerStay(Collider other)
    {
        // 检测是否碰到蛇身且可以攻击
        if (other.CompareTag("SnakeBody") && !isAttacking && Time.time - lastDamageTime >= damageInterval)
        {
            HandleDamage(other);
        }
    }

    /// <summary>
    /// 处理对目标造成伤害的逻辑
    /// </summary>
    /// <param name="other">被攻击的碰撞器</param>
    void HandleDamage(Collider other)
    {
        // 尝试获取Health组件
        Health bodyPartHealth = other.GetComponent<Health>();
        if (bodyPartHealth == null)
        {
            bodyPartHealth = other.GetComponentInParent<Health>();
        }

        if (bodyPartHealth != null)
        {
            isAttacking = true;
            // Debug.Log($"[EnemyAI] {gameObject.name} 尝试对 {other.gameObject.name} 造成 {damageAmount} 点伤害");
            bodyPartHealth.TakeDamage(damageAmount);
            lastDamageTime = Time.time;
            isAttacking = false;
        }
    }

    /// <summary>
    /// 敌人死亡事件处理方法
    /// </summary>
    void OnEnemyDeath()
    {
        // 通知PassiveSkillManager增加击杀计数
        if (PassiveSkillManager.Instance != null)
        {
            PassiveSkillManager.Instance.AddKill();
            // Debug.Log("[EnemyAI] 敌人被击杀，已通知PassiveSkillManager");
        }
    }

    /// <summary>
    /// 设置敌人的伤害值
    /// </summary>
    /// <param name="amount">新的伤害值</param>
    public void SetDamageAmount(int amount)
    {
        damageAmount = Mathf.Max(1, amount);
        // Debug.Log($"[EnemyAI] {gameObject.name} 伤害值被设置为: {damageAmount}");
    }

    /// <summary>
    /// 在Scene视图中显示碰撞半径
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sphereCollider != null ? sphereCollider.radius : collisionRadius);
    }
}