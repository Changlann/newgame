using System.Collections.Generic;
using UnityEngine;

public class SnakeBody : MonoBehaviour
{
    [Header("预制体设置")]
    [Tooltip("蛇身预制体")]
    public GameObject bodyPartPrefab;
    [Tooltip("子弹预制体")]
    public GameObject bulletPrefab;

    [Header("移动设置")]
    [Tooltip("身体部分之间的距离")]
    public float spacing = 1f;
    [Tooltip("移动平滑度")]
    public float moveSpeed = 8f;
    [Tooltip("旋转平滑度")]
    public float rotateSpeed = 8f;

    [Header("生命值设置")]
    [Tooltip("身体部分的初始生命值")]
    public float bodyPartHealth = 100f;
    [Tooltip("新生成身体部分的无敌时间")]
    public float invincibleDuration = 2f;

    // 存储所有身体部分
    private List<BodyPart> bodyParts = new List<BodyPart>();

    // 用来存储身体部分信息的类
    [System.Serializable]
    private class BodyPart
    {
        public GameObject gameObject;
        public Transform transform;
        public SoldierShooter shooter;
        public Vector3 targetPosition;
        public Quaternion targetRotation;
        public float invincibleTime;  // 新增：无敌时间

        public BodyPart(GameObject go)
        {
            gameObject = go;
            transform = go.transform;
            shooter = go.GetComponent<SoldierShooter>();
            targetPosition = transform.position;
            targetRotation = transform.rotation;
            invincibleTime = Time.time + 2f;  // 默认2秒无敌时间
        }

        public bool IsValid()
        {
            return gameObject != null && transform != null;
        }
    }

    private void Start()
    {
        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (bodyPartPrefab == null)
        {
            Debug.LogError("[SnakeBody] Body Part Prefab 未设置！请在 Inspector 中设置！", this);
            enabled = false;
            return;
        }

        if (bulletPrefab == null)
        {
            Debug.LogError("[SnakeBody] Bullet Prefab 未设置！请在 Inspector 中设置！", this);
            enabled = false;
            return;
        }
    }

    public void AddBodyPart()
    {
        // 清理已经无效的身体部分
        bodyParts.RemoveAll(part => part == null || !part.IsValid());

        Vector3 newPosition;
        Quaternion newRotation;

        if (bodyParts.Count == 0)
        {
            // 第一个身体部分跟随头部
            newPosition = transform.position - transform.forward * spacing;
            newRotation = transform.rotation;
        }
        else
        {
            // 获取最后一个有效的身体部分
            BodyPart lastPart = bodyParts[bodyParts.Count - 1];
            if (!lastPart.IsValid())
            {
                Debug.LogError("[SnakeBody] 最后一个身体部分无效！");
                return;
            }
            newPosition = lastPart.transform.position - lastPart.transform.forward * spacing;
            newRotation = lastPart.transform.rotation;
        }

        try
        {
            GameObject newPart = Instantiate(bodyPartPrefab, newPosition, newRotation);

            // 设置标签和层级
            newPart.tag = "SnakeBody";
            newPart.layer = LayerMask.NameToLayer("SnakeBody");

            // 确保有碰撞器
            SphereCollider sphereCollider = newPart.GetComponent<SphereCollider>();
            if (sphereCollider == null)
            {
                sphereCollider = newPart.AddComponent<SphereCollider>();
                sphereCollider.radius = 0.5f;
            }
            sphereCollider.isTrigger = true;

            // 设置刚体
            Rigidbody rb = newPart.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = newPart.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            rb.useGravity = false;

            // 设置Health组件
            Health health = newPart.GetComponent<Health>();
            if (health == null)
            {
                health = newPart.AddComponent<Health>();
            }
            health.maxHealth = bodyPartHealth;
            health.currentHealth = bodyPartHealth;
            health.SetInvincible(invincibleDuration);  // 设置初始无敌时间

            // 添加被动治疗组件
            PassiveHealing passiveHealing = newPart.AddComponent<PassiveHealing>();
            if (passiveHealing != null)
            {
                passiveHealing.healthThreshold = 0.3f; // 30%生命值时触发
                passiveHealing.cooldown = 30f; // 30秒冷却
                passiveHealing.maxHealthIncreasePercent = 0.1f; // 增加10%最大生命值
            }

            // 添加武器持有者组件
            WeaponHolder weaponHolder = newPart.AddComponent<WeaponHolder>();
            if (weaponHolder != null)
            {
                // 创建武器挂载点
                GameObject socketObj = new GameObject("WeaponSocket");
                socketObj.transform.SetParent(newPart.transform);
                socketObj.transform.localPosition = new Vector3(0, 0.5f, 0); // 调整位置
                socketObj.transform.localRotation = Quaternion.identity;

                weaponHolder.weaponSocket = socketObj.transform;
            }

            // 设置射击组件
            SoldierShooter shooter = newPart.GetComponent<SoldierShooter>();
            if (shooter == null)
            {
                shooter = newPart.AddComponent<SoldierShooter>();
            }
            shooter.SetBulletPrefab(this.bulletPrefab);
            shooter.fireRate = 1f;
            shooter.detectionRange = 15f;
            shooter.bulletDamage = 15;
            shooter.enabled = true;

            // 添加到列表
            bodyParts.Add(new BodyPart(newPart));
            Debug.Log($"[SnakeBody] 成功添加新的身体部分 - 当前长度: {bodyParts.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SnakeBody] 添加身体部分时出错：{e.Message}");
        }
    }

    private void Update()
    {
        UpdateBodyParts();
    }

    private void UpdateBodyParts()
    {
        // 清理无效的身体部分
        bodyParts.RemoveAll(part => part == null || !part.IsValid());

        for (int i = 0; i < bodyParts.Count; i++)
        {
            BodyPart part = bodyParts[i];
            if (!part.IsValid()) continue;

            Transform target = (i == 0) ? transform : bodyParts[i - 1].transform;
            if (target == null) continue;

            // 更新目标位置和旋转
            part.targetPosition = target.position - target.forward * spacing;
            part.targetRotation = target.rotation;

            // 平滑移动到目标位置
            part.transform.position = Vector3.Lerp(
                part.transform.position,
                part.targetPosition,
                moveSpeed * Time.deltaTime
            );

            // 平滑旋转到目标朝向
            part.transform.rotation = Quaternion.Slerp(
                part.transform.rotation,
                part.targetRotation,
                rotateSpeed * Time.deltaTime
            );
        }
    }

    // 提供一个方法获取所有身体部分的Transform，供WeaponHolder使用
    public Transform[] GetBodyPartTransforms()
    {
        Transform[] transforms = new Transform[bodyParts.Count];
        for (int i = 0; i < bodyParts.Count; i++)
        {
            transforms[i] = bodyParts[i].transform;
        }
        return transforms;
    }

    private void OnDrawGizmos()
    {
        // 清理无效的身体部分
        if (Application.isPlaying)
        {
            bodyParts.RemoveAll(part => part == null || !part.IsValid());
        }

        Gizmos.color = Color.green;

        // 绘制头部到第一个身体部分的连接
        if (bodyParts.Count > 0 && bodyParts[0] != null && bodyParts[0].IsValid())
        {
            Gizmos.DrawLine(transform.position, bodyParts[0].transform.position);
        }

        // 绘制身体部分之间的连接
        for (int i = 1; i < bodyParts.Count; i++)
        {
            if (bodyParts[i] != null && bodyParts[i].IsValid() &&
                bodyParts[i - 1] != null && bodyParts[i - 1].IsValid())
            {
                Gizmos.DrawLine(
                    bodyParts[i].transform.position,
                    bodyParts[i - 1].transform.position
                );
            }
        }
    }
}