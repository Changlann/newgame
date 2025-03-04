using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("��������")]
    [Tooltip("ÿ�ι�����ɵ��˺�ֵ")]
    [SerializeField] private int damageAmount = 1;
    [Tooltip("�������ʱ��(��)")]
    [SerializeField] private float damageInterval = 1f;

    [Header("�ƶ�����")]
    [Tooltip("�ƶ��ٶ�")]
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("��ײ���뾶")]
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
    /// ��ʼ������������������
    /// </summary>
    void InitializeComponents()
    {
        // ��ʼ��������ײ��
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
        }
        sphereCollider.radius = collisionRadius;
        sphereCollider.isTrigger = true;

        // ��ʼ���������
        enemyRigidbody = GetComponent<Rigidbody>();
        if (enemyRigidbody == null)
        {
            enemyRigidbody = gameObject.AddComponent<Rigidbody>();
        }
        enemyRigidbody.isKinematic = true;
        enemyRigidbody.useGravity = false;

        // ��ʼ����������
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }

        // ������Ҷ���
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // ��Ӷ�Health��������¼��ļ���
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.onDeath.AddListener(OnEnemyDeath);
        }

        // Debug.Log($"[EnemyAI] {gameObject.name} ��ʼ�� - �˺�ֵ: {damageAmount}, �˺����: {damageInterval}");
    }

    void Update()
    {
        // ���µ���Ŀ��Ϊ���λ��
        if (player != null && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }

    /// <summary>
    /// ��������ײ��ͣ���ڴ�������ʱ����
    /// </summary>
    void OnTriggerStay(Collider other)
    {
        // ����Ƿ����������ҿ��Թ���
        if (other.CompareTag("SnakeBody") && !isAttacking && Time.time - lastDamageTime >= damageInterval)
        {
            HandleDamage(other);
        }
    }

    /// <summary>
    /// �����Ŀ������˺����߼�
    /// </summary>
    /// <param name="other">����������ײ��</param>
    void HandleDamage(Collider other)
    {
        // ���Ի�ȡHealth���
        Health bodyPartHealth = other.GetComponent<Health>();
        if (bodyPartHealth == null)
        {
            bodyPartHealth = other.GetComponentInParent<Health>();
        }

        if (bodyPartHealth != null)
        {
            isAttacking = true;
            // Debug.Log($"[EnemyAI] {gameObject.name} ���Զ� {other.gameObject.name} ��� {damageAmount} ���˺�");
            bodyPartHealth.TakeDamage(damageAmount);
            lastDamageTime = Time.time;
            isAttacking = false;
        }
    }

    /// <summary>
    /// ���������¼�������
    /// </summary>
    void OnEnemyDeath()
    {
        // ֪ͨPassiveSkillManager���ӻ�ɱ����
        if (PassiveSkillManager.Instance != null)
        {
            PassiveSkillManager.Instance.AddKill();
            // Debug.Log("[EnemyAI] ���˱���ɱ����֪ͨPassiveSkillManager");
        }
    }

    /// <summary>
    /// ���õ��˵��˺�ֵ
    /// </summary>
    /// <param name="amount">�µ��˺�ֵ</param>
    public void SetDamageAmount(int amount)
    {
        damageAmount = Mathf.Max(1, amount);
        // Debug.Log($"[EnemyAI] {gameObject.name} �˺�ֵ������Ϊ: {damageAmount}");
    }

    /// <summary>
    /// ��Scene��ͼ����ʾ��ײ�뾶
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sphereCollider != null ? sphereCollider.radius : collisionRadius);
    }
}