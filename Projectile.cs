using UnityEngine;
using System.Collections.Generic;

// ��ǿ���ӵ�/Ͷ����ʵ�֣�֧�ָ�����������
public class Projectile : MonoBehaviour
{
    [Header("��������")]
    [Tooltip("�ӵ���ɵ��˺�ֵ")]
    public int damage = 15;
    [Tooltip("�ӵ������ٶ�")]
    public float speed = 20f;
    [Tooltip("�ӵ�����ʱ��(��)")]
    public float lifetime = 5f;

    [Header("����Ч��")]
    [Tooltip("�Ƿ���д�͸Ч��(�ɴ�͸���˼���ǰ��)")]
    public bool isPiercing = false; // ��͸Ч��
    [Tooltip("�Ƿ���е���Ч��(��ײ�󷴵�)")]
    public bool isBouncing = false; // ����Ч��
    [Tooltip("��������")]
    public int maxBounces = 0; // ��������
    [Tooltip("�Ƿ���׷�ٵ���")]
    public bool isHoming = false; // ׷��Ч��
    [Tooltip("׷��ǿ��(��ֵԽ��ת��Խ��)")]
    public float homingStrength = 1f; // ׷��ǿ��

    [Header("�Ӿ�Ч��")]
    [Tooltip("�ӵ���ɫ")]
    public Color projectileColor = Color.yellow;
    [Tooltip("����Ч��Ԥ����")]
    public GameObject impactEffectPrefab;
    [Tooltip("��βЧ��Ԥ����")]
    public GameObject trailEffectPrefab;

    // �ڲ�����
    private bool hasHit = false; // �Ƿ��ѻ�������
    private int currentBounces = 0; // ��ǰ�ѵ������
    private Transform homingTarget; // ׷��Ŀ��
    private Rigidbody rb; // �������
    private List<GameObject> hitObjects = new List<GameObject>(); // ��¼�ѻ��еĶ��󣬷�ֹ����˺�
    private TrailRenderer trailRenderer; // ��β��Ⱦ��

    void Start()
    {
        // ��ʼ���������
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
        }

        // �����Զ����ټ�ʱ��
        Destroy(gameObject, lifetime);

        // Ӧ����ɫ����Ⱦ��
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = projectileColor;
            renderer.material.SetColor("_EmissionColor", projectileColor * 0.5f);
        }

        // �����βЧ��
        SetupTrailEffect();

        // �����׷�����ӵ���Ѱ������ĵ���
        if (isHoming)
        {
            FindNearestEnemy();
        }

        Debug.Log($"[Projectile] �ӵ����ɣ��˺���{damage}���ٶȣ�{speed}����Ч��" +
                 $"��͸={isPiercing}������={isBouncing}��׷��={isHoming}");
    }

    void Update()
    {
        // ����ѻ����Ҳ����д�͸����Ч�������ٸ���
        if (hasHit && !isPiercing && !isBouncing) return;

        // ׷���߼� - �����׷�����ӵ�����Ŀ�꣬�����������Ŀ��
        if (isHoming && homingTarget != null)
        {
            Vector3 targetDirection = (homingTarget.position - transform.position).normalized;
            Vector3 newDirection = Vector3.Lerp(transform.forward, targetDirection, Time.deltaTime * homingStrength);
            transform.forward = newDirection;
        }
    }

    void FixedUpdate()
    {
        // ����ѻ����Ҳ����д�͸����Ч���������ƶ�
        if (hasHit && !isPiercing && !isBouncing) return;

        // �ƶ��ӵ� - ����ʹ�ø��壬���û����ֱ�ӵ���λ��
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
        else
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    // ��������ײ���
    void OnTriggerEnter(Collider other)
    {
        // ��������ײͬһ����
        if (hitObjects.Contains(other.gameObject)) return;

        // ������ײ����������Ƿ������ӵ�
        bool shouldDestroy = ProcessHit(other);

        if (shouldDestroy)
        {
            Destroy(gameObject);
        }
    }

    // ������ײ�߼�
    private bool ProcessHit(Collider other)
    {
        // ������ҡ������ӵ�����β����ײ
        if (other.CompareTag("Player") || other.CompareTag("Bullet") || other.CompareTag("SnakeBody"))
        {
            return false;
        }

        // ���Ϊ�ѻ��У�����¼��ײ����
        hasHit = true;
        hitObjects.Add(other.gameObject);

        // ���ɻ�����Ч
        SpawnImpactEffect(other);

        // ������е��ˣ������˺�
        if (other.CompareTag("Enemy"))
        {
            HandleEnemyDamage(other);
        }

        // ������Ч��
        if (isBouncing && currentBounces < maxBounces)
        {
            PerformBounce(other);
            return false; // �������ӵ�����������
        }

        // ����͸Ч��
        if (isPiercing)
        {
            return false; // �������ӵ���������͸
        }

        // ������������ӵ�
        return true;
    }

    // ����Ե�������˺�
    private void HandleEnemyDamage(Collider other)
    {
        // ���Ի�ȡ���˵�Health���
        Health enemyHealth = other.GetComponent<Health>();
        if (enemyHealth == null)
        {
            enemyHealth = other.GetComponentInParent<Health>();
        }

        // ����ҵ�Health�����������˺�
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage, true);  // ����true��������ʾ�˺�����
            Debug.Log($"[Projectile] �Ե������ {damage} ���˺�");
        }
        else
        {
            Debug.LogWarning($"[Projectile] ���еĵ���û��Health���: {other.gameObject.name}");
        }
    }

    // ִ�е����߼�
    private void PerformBounce(Collider other)
    {
        currentBounces++;
        Debug.Log($"[Projectile] �������: {currentBounces}/{maxBounces}");

        // ���㵯�䷽�� - Ĭ�����ϵ���
        Vector3 normal = Vector3.up;

        // ���Ի�ȡ��ײ��ķ��ߣ����ڼ��㷴�䷽��
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1f))
        {
            normal = hit.normal;
        }

        // ���㷴�䷽�򲢵����ӵ�����
        Vector3 reflectDirection = Vector3.Reflect(transform.forward, normal);
        transform.forward = reflectDirection;

        // ��������ٶȣ�ȷ���ӵ����·����ƶ�
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }

        // ���hit��ǣ������ٴ���ײ
        hasHit = false;

        // ���������Ѱ��׷��Ŀ��
        if (isHoming)
        {
            FindNearestEnemy();
        }
    }

    // ���ɻ�����Ч
    private void SpawnImpactEffect(Collider other)
    {
        if (impactEffectPrefab == null) return;

        // ����ײ��������Ч
        Vector3 hitPoint = other.ClosestPoint(transform.position);
        GameObject effect = Instantiate(impactEffectPrefab, hitPoint, Quaternion.identity);

        // ʹ��Ч������ײ�㷨�߷���
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f))
        {
            effect.transform.forward = hit.normal;
        }

        // �Զ�������Ч
        Destroy(effect, 2f);
    }

    // ������βЧ��
    private void SetupTrailEffect()
    {
        // �����Ԥ���壬��ʵ����Ԥ����
        if (trailEffectPrefab != null)
        {
            GameObject trailObj = Instantiate(trailEffectPrefab, transform);
            trailRenderer = trailObj.GetComponent<TrailRenderer>();
            if (trailRenderer != null)
            {
                trailRenderer.startColor = projectileColor;
                trailRenderer.endColor = new Color(projectileColor.r, projectileColor.g, projectileColor.b, 0f);
            }
        }
        else
        {
            // ���û��Ԥ���壬�������һ���򵥵���β��Ⱦ��
            trailRenderer = GetComponent<TrailRenderer>();
            if (trailRenderer == null && GetComponent<Renderer>() != null)
            {
                trailRenderer = gameObject.AddComponent<TrailRenderer>();
                trailRenderer.startWidth = 0.1f;
                trailRenderer.endWidth = 0.01f;
                trailRenderer.time = 0.2f;
                trailRenderer.startColor = projectileColor;
                trailRenderer.endColor = new Color(projectileColor.r, projectileColor.g, projectileColor.b, 0f);
                trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }
        }
    }

    // Ѱ������ĵ�����Ϊ׷��Ŀ��
    private void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        // �������е��ˣ��ҵ������һ��
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        homingTarget = closestEnemy;
    }

    // ���������������˺�ֵ
    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    // ����������Ӧ��������������
    public void ApplyWeaponData(WeaponData weaponData)
    {
        if (weaponData == null) return;

        // ���������������ӵ�����
        damage = weaponData.damage;
        speed = weaponData.bulletSpeed;
        isPiercing = weaponData.hasPiercing;
        isBouncing = weaponData.hasBounce;
        maxBounces = weaponData.bounceCount;
        isHoming = weaponData.isHoming;
        homingStrength = weaponData.homingStrength;
        projectileColor = weaponData.bulletColor;

        // Ӧ����ɫ����Ⱦ��
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = projectileColor;
            renderer.material.SetColor("_EmissionColor", projectileColor * 0.5f);
        }

        // ������β��ɫ
        if (trailRenderer != null)
        {
            trailRenderer.startColor = projectileColor;
            trailRenderer.endColor = new Color(projectileColor.r, projectileColor.g, projectileColor.b, 0f);
        }
    }
}