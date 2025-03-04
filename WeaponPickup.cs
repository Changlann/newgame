using UnityEngine;

// 此脚本附加到武器拾取物上
public class WeaponPickup : MonoBehaviour
{
    [Header("武器信息")]
    [Tooltip("此拾取物提供的武器数据")]
    public WeaponData weaponData; // 此拾取物提供的武器数据

    [Header("视觉设置")]
    [Tooltip("旋转速度")]
    public float rotationSpeed = 50f; // 旋转速度
    [Tooltip("上下浮动高度")]
    public float bobHeight = 0.5f; // 上下浮动高度
    [Tooltip("浮动速度")]
    public float bobSpeed = 1f; // 浮动速度

    [Header("拾取设置")]
    [Tooltip("拾取范围")]
    public float pickupRadius = 2f; // 拾取范围
    [Tooltip("拾取特效")]
    public GameObject pickupEffectPrefab; // 拾取特效
    [Tooltip("拾取音效")]
    public AudioClip pickupSound; // 拾取音效
    [Tooltip("拾取冷却时间(秒)")]
    public float pickupCooldown = 0.5f; // 拾取冷却时间

    private Vector3 startPosition;
    private float bobTime;

    // 添加一个静态变量，跟踪上次拾取的时间
    private static float lastPickupTime = 0f;

    void Start()
    {
        startPosition = transform.position;
        bobTime = Random.Range(0f, 2f * Mathf.PI); // 随机初始相位

        // 如果没有设置武器数据，给出警告
        if (weaponData == null)
        {
            Debug.LogWarning($"[WeaponPickup] {gameObject.name} 没有设置武器数据!");
        }
    }

    void Update()
    {
        // 旋转效果
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 上下浮动效果
        bobTime += bobSpeed * Time.deltaTime;
        float newY = startPosition.y + Mathf.Sin(bobTime) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        // 检查是否在冷却时间内
        if (Time.time - lastPickupTime < pickupCooldown)
        {
            Debug.Log("[WeaponPickup] 拾取冷却中，忽略此次碰撞");
            return;
        }

        Debug.Log($"触发器检测到：{other.gameObject.name}, 标签：{other.tag}");

        // 检查是否是玩家(蛇头)触发
        if (other.CompareTag("Player"))
        {
            Debug.Log("检测到玩家碰撞，尝试拾取武器");
            lastPickupTime = Time.time; // 更新上次拾取时间
            AttemptPickup(other.gameObject);
        }
    }

    // 尝试拾取武器
    private void AttemptPickup(GameObject player)
    {
        if (weaponData == null) return;

        // 首先尝试在蛇头上查找SnakeBody组件
        SnakeBody snakeBody = player.GetComponent<SnakeBody>();

        // 如果蛇头上没有，尝试查找全局SnakeBody对象
        if (snakeBody == null)
        {
            Debug.Log("[WeaponPickup] 玩家对象上没有SnakeBody组件，尝试查找全局SnakeBody");
            snakeBody = FindObjectOfType<SnakeBody>();
        }

        if (snakeBody == null)
        {
            Debug.LogWarning("[WeaponPickup] 找不到任何SnakeBody组件!");
            return;
        }

        Debug.Log("[WeaponPickup] 找到SnakeBody组件，尝试分配武器");

        // 直接使用SnakeBody上的GetBodyPartTransforms获取所有身体部分
        Transform[] bodyPartTransforms = snakeBody.GetBodyPartTransforms();

        if (bodyPartTransforms == null || bodyPartTransforms.Length == 0)
        {
            Debug.LogWarning("[WeaponPickup] SnakeBody没有任何身体部分!");
            return;
        }

        Debug.Log($"[WeaponPickup] 找到 {bodyPartTransforms.Length} 个身体部分");

        // 查找空的或正确索引的武器持有者
        bool weaponAssigned = false;
        int lastEquippedIndex = -1; // 跟踪上次装备的索引

        // 首先寻找一个没有武器的持有者
        for (int i = 0; i < bodyPartTransforms.Length; i++)
        {
            Transform bodyPart = bodyPartTransforms[i];
            if (bodyPart == null) continue;

            WeaponHolder holder = bodyPart.GetComponent<WeaponHolder>();
            if (holder != null && !holder.HasWeapon())
            {
                if (holder.EquipWeapon(weaponData))
                {
                    weaponAssigned = true;
                    lastEquippedIndex = i;
                    Debug.Log($"[WeaponPickup] 武器成功分配给空的持有者 {bodyPart.name}，索引 {i}");
                    break;
                }
            }
        }

        // 如果没有找到空的持有者，使用下一个索引的持有者
        if (!weaponAssigned)
        {
            // 获取当前已装备的最高索引
            for (int i = 0; i < bodyPartTransforms.Length; i++)
            {
                Transform bodyPart = bodyPartTransforms[i];
                if (bodyPart == null) continue;

                WeaponHolder holder = bodyPart.GetComponent<WeaponHolder>();
                if (holder != null && holder.HasWeapon())
                {
                    lastEquippedIndex = Mathf.Max(lastEquippedIndex, i);
                }
            }

            // 尝试装备给下一个索引的持有者
            int nextIndex = lastEquippedIndex + 1;
            if (nextIndex < bodyPartTransforms.Length)
            {
                Transform bodyPart = bodyPartTransforms[nextIndex];
                if (bodyPart != null)
                {
                    WeaponHolder holder = bodyPart.GetComponent<WeaponHolder>();
                    if (holder != null)
                    {
                        // 如果已有武器，先移除
                        if (holder.HasWeapon())
                        {
                            holder.RemoveWeapon();
                        }

                        if (holder.EquipWeapon(weaponData))
                        {
                            weaponAssigned = true;
                            Debug.Log($"[WeaponPickup] 武器成功分配给下一个持有者 {bodyPart.name}，索引 {nextIndex}");
                        }
                    }
                }
            }
        }

        // 如果仍然没有分配，强制分配给第一个持有者（后备方案）
        if (!weaponAssigned && bodyPartTransforms.Length > 0)
        {
            Transform bodyPart = bodyPartTransforms[0];
            WeaponHolder holder = bodyPart.GetComponent<WeaponHolder>();
            if (holder != null)
            {
                if (holder.HasWeapon())
                {
                    holder.RemoveWeapon();
                }

                if (holder.EquipWeapon(weaponData))
                {
                    weaponAssigned = true;
                    Debug.Log($"[WeaponPickup] 武器强制分配给第一个持有者 {bodyPart.name}");
                }
            }
        }

        if (weaponAssigned)
        {
            // 播放拾取特效
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            // 播放拾取音效
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            Debug.Log($"[WeaponPickup] 玩家拾取了武器: {weaponData.weaponName}");

            // 销毁拾取物
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("[WeaponPickup] 无法分配武器，没有找到可用的武器持有者");
        }
    }

    // 在Scene视图中显示拾取范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}