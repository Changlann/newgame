using UnityEngine;
using System.Collections.Generic;

// 增强的子弹/投射物实现，支持更多武器特性
public class Projectile : MonoBehaviour
{
    [Header("基本属性")]
    [Tooltip("子弹造成的伤害值")]
    public int damage = 15;
    [Tooltip("子弹飞行速度")]
    public float speed = 20f;
    [Tooltip("子弹存在时间(秒)")]
    public float lifetime = 5f;

    [Header("特殊效果")]
    [Tooltip("是否具有穿透效果(可穿透敌人继续前进)")]
    public bool isPiercing = false; // 穿透效果
    [Tooltip("是否具有弹射效果(碰撞后反弹)")]
    public bool isBouncing = false; // 弹射效果
    [Tooltip("最大弹射次数")]
    public int maxBounces = 0; // 最大弹射次数
    [Tooltip("是否能追踪敌人")]
    public bool isHoming = false; // 追踪效果
    [Tooltip("追踪强度(数值越大转向越快)")]
    public float homingStrength = 1f; // 追踪强度

    [Header("视觉效果")]
    [Tooltip("子弹颜色")]
    public Color projectileColor = Color.yellow;
    [Tooltip("击中效果预制体")]
    public GameObject impactEffectPrefab;
    [Tooltip("拖尾效果预制体")]
    public GameObject trailEffectPrefab;

    // 内部变量
    private bool hasHit = false; // 是否已击中物体
    private int currentBounces = 0; // 当前已弹射次数
    private Transform homingTarget; // 追踪目标
    private Rigidbody rb; // 刚体组件
    private List<GameObject> hitObjects = new List<GameObject>(); // 记录已击中的对象，防止多次伤害
    private TrailRenderer trailRenderer; // 拖尾渲染器

    void Start()
    {
        // 初始化刚体组件
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
        }

        // 设置自动销毁计时器
        Destroy(gameObject, lifetime);

        // 应用颜色到渲染器
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = projectileColor;
            renderer.material.SetColor("_EmissionColor", projectileColor * 0.5f);
        }

        // 添加拖尾效果
        SetupTrailEffect();

        // 如果是追踪型子弹，寻找最近的敌人
        if (isHoming)
        {
            FindNearestEnemy();
        }

        Debug.Log($"[Projectile] 子弹生成，伤害：{damage}，速度：{speed}，特效：" +
                 $"穿透={isPiercing}，弹射={isBouncing}，追踪={isHoming}");
    }

    void Update()
    {
        // 如果已击中且不具有穿透或弹射效果，则不再更新
        if (hasHit && !isPiercing && !isBouncing) return;

        // 追踪逻辑 - 如果是追踪型子弹且有目标，则调整方向朝向目标
        if (isHoming && homingTarget != null)
        {
            Vector3 targetDirection = (homingTarget.position - transform.position).normalized;
            Vector3 newDirection = Vector3.Lerp(transform.forward, targetDirection, Time.deltaTime * homingStrength);
            transform.forward = newDirection;
        }
    }

    void FixedUpdate()
    {
        // 如果已击中且不具有穿透或弹射效果，则不再移动
        if (hasHit && !isPiercing && !isBouncing) return;

        // 移动子弹 - 优先使用刚体，如果没有则直接调整位置
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
        else
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    // 触发器碰撞检测
    void OnTriggerEnter(Collider other)
    {
        // 避免多次碰撞同一对象
        if (hitObjects.Contains(other.gameObject)) return;

        // 处理碰撞结果，决定是否销毁子弹
        bool shouldDestroy = ProcessHit(other);

        if (shouldDestroy)
        {
            Destroy(gameObject);
        }
    }

    // 处理碰撞逻辑
    private bool ProcessHit(Collider other)
    {
        // 忽略玩家、其他子弹和蛇尾的碰撞
        if (other.CompareTag("Player") || other.CompareTag("Bullet") || other.CompareTag("SnakeBody"))
        {
            return false;
        }

        // 标记为已击中，并记录碰撞对象
        hasHit = true;
        hitObjects.Add(other.gameObject);

        // 生成击中特效
        SpawnImpactEffect(other);

        // 如果击中敌人，处理伤害
        if (other.CompareTag("Enemy"))
        {
            HandleEnemyDamage(other);
        }

        // 处理弹射效果
        if (isBouncing && currentBounces < maxBounces)
        {
            PerformBounce(other);
            return false; // 不销毁子弹，继续弹射
        }

        // 处理穿透效果
        if (isPiercing)
        {
            return false; // 不销毁子弹，继续穿透
        }

        // 其他情况销毁子弹
        return true;
    }

    // 处理对敌人造成伤害
    private void HandleEnemyDamage(Collider other)
    {
        // 尝试获取敌人的Health组件
        Health enemyHealth = other.GetComponent<Health>();
        if (enemyHealth == null)
        {
            enemyHealth = other.GetComponentInParent<Health>();
        }

        // 如果找到Health组件，则造成伤害
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage, true);  // 传递true参数以显示伤害数字
            Debug.Log($"[Projectile] 对敌人造成 {damage} 点伤害");
        }
        else
        {
            Debug.LogWarning($"[Projectile] 击中的敌人没有Health组件: {other.gameObject.name}");
        }
    }

    // 执行弹射逻辑
    private void PerformBounce(Collider other)
    {
        currentBounces++;
        Debug.Log($"[Projectile] 弹射次数: {currentBounces}/{maxBounces}");

        // 计算弹射方向 - 默认向上弹射
        Vector3 normal = Vector3.up;

        // 尝试获取碰撞点的法线，用于计算反射方向
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1f))
        {
            normal = hit.normal;
        }

        // 计算反射方向并调整子弹朝向
        Vector3 reflectDirection = Vector3.Reflect(transform.forward, normal);
        transform.forward = reflectDirection;

        // 重设刚体速度，确保子弹沿新方向移动
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }

        // 清除hit标记，允许再次碰撞
        hasHit = false;

        // 弹射后重新寻找追踪目标
        if (isHoming)
        {
            FindNearestEnemy();
        }
    }

    // 生成击中特效
    private void SpawnImpactEffect(Collider other)
    {
        if (impactEffectPrefab == null) return;

        // 在碰撞点生成特效
        Vector3 hitPoint = other.ClosestPoint(transform.position);
        GameObject effect = Instantiate(impactEffectPrefab, hitPoint, Quaternion.identity);

        // 使特效面向碰撞点法线方向
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f))
        {
            effect.transform.forward = hit.normal;
        }

        // 自动销毁特效
        Destroy(effect, 2f);
    }

    // 设置拖尾效果
    private void SetupTrailEffect()
    {
        // 如果有预制体，则实例化预制体
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
            // 如果没有预制体，尝试添加一个简单的拖尾渲染器
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

    // 寻找最近的敌人作为追踪目标
    private void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        // 遍历所有敌人，找到最近的一个
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

    // 公开方法，设置伤害值
    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    // 公开方法，应用武器数据设置
    public void ApplyWeaponData(WeaponData weaponData)
    {
        if (weaponData == null) return;

        // 从武器数据设置子弹属性
        damage = weaponData.damage;
        speed = weaponData.bulletSpeed;
        isPiercing = weaponData.hasPiercing;
        isBouncing = weaponData.hasBounce;
        maxBounces = weaponData.bounceCount;
        isHoming = weaponData.isHoming;
        homingStrength = weaponData.homingStrength;
        projectileColor = weaponData.bulletColor;

        // 应用颜色到渲染器
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = projectileColor;
            renderer.material.SetColor("_EmissionColor", projectileColor * 0.5f);
        }

        // 更新拖尾颜色
        if (trailRenderer != null)
        {
            trailRenderer.startColor = projectileColor;
            trailRenderer.endColor = new Color(projectileColor.r, projectileColor.g, projectileColor.b, 0f);
        }
    }
}