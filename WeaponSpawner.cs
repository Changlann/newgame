using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 此脚本用于在场景中生成武器拾取物
public class WeaponSpawner : MonoBehaviour
{
    [Header("生成设置")]
    [Tooltip("可用武器类型列表")]
    public List<WeaponData> availableWeapons = new List<WeaponData>(); // 可用武器类型
    [Tooltip("武器拾取物预制体")]
    public GameObject weaponPickupPrefab; // 武器拾取物预制体
    [Tooltip("场景中最大武器数量")]
    public int maxWeapons = 5; // 场景中最大武器数量
    [Tooltip("生成间隔时间(秒)")]
    public float spawnInterval = 10f; // 生成间隔
    [Tooltip("初始延迟时间(秒)")]
    public float initialDelay = 5f; // 初始延迟

    [Header("生成区域")]
    [Tooltip("生成半径范围")]
    public float spawnRadius = 30f; // 生成半径
    [Tooltip("距离玩家的最小距离")]
    public float minDistanceFromPlayer = 8f; // 距离玩家的最小距离
    [Tooltip("武器之间的最小距离")]
    public float minDistanceFromWeapons = 5f; // 武器之间的最小距离

    [Header("调试")]
    [Tooltip("显示调试信息")]
    public bool showDebugInfo = true;

    private Transform playerTransform;
    private List<GameObject> activeWeapons = new List<GameObject>();
    private bool isSpawning = false;

    void Start()
    {
        // 查找玩家
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("[WeaponSpawner] 找不到玩家对象！生成位置可能不理想！");
        }

        // 检查设置
        if (availableWeapons.Count == 0)
        {
            Debug.LogError("[WeaponSpawner] 没有设置可用武器！请在Inspector中添加武器数据！");
            enabled = false;
            return;
        }

        if (weaponPickupPrefab == null)
        {
            Debug.LogError("[WeaponSpawner] 未设置武器拾取物预制体！");
            enabled = false;
            return;
        }

        // 开始生成循环
        StartCoroutine(SpawnRoutine());
    }

    // 武器生成循环
    private IEnumerator SpawnRoutine()
    {
        isSpawning = true;

        // 初始延迟
        yield return new WaitForSeconds(initialDelay);

        while (isSpawning)
        {
            // 清理无效的武器引用
            activeWeapons.RemoveAll(weapon => weapon == null);

            // 如果武器数量未达上限，尝试生成
            if (activeWeapons.Count < maxWeapons)
            {
                SpawnWeapon();
            }

            // 等待下一次生成
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // 生成一个武器
    private void SpawnWeapon()
    {
        // 随机选择一个武器数据
        WeaponData selectedWeapon = availableWeapons[Random.Range(0, availableWeapons.Count)];
        if (selectedWeapon == null)
        {
            Debug.LogWarning("[WeaponSpawner] 选中的武器数据为空！");
            return;
        }

        // 获取生成位置
        Vector3 spawnPosition = GetSpawnPosition();
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("[WeaponSpawner] 无法找到合适的生成位置，跳过本次生成！");
            return;
        }

        // 实例化武器拾取物
        GameObject weaponInstance = Instantiate(weaponPickupPrefab, spawnPosition, Quaternion.identity);

        // 设置武器数据
        WeaponPickup pickup = weaponInstance.GetComponent<WeaponPickup>();
        if (pickup != null)
        {
            pickup.weaponData = selectedWeapon;
        }
        else
        {
            Debug.LogError("[WeaponSpawner] 武器拾取物预制体没有WeaponPickup组件！");
            Destroy(weaponInstance);
            return;
        }

        // 添加到活动武器列表
        activeWeapons.Add(weaponInstance);

        if (showDebugInfo)
        {
            Debug.Log($"[WeaponSpawner] 生成武器: {selectedWeapon.weaponName} 在位置: {spawnPosition}");
        }
    }

    // 获取合适的生成位置
    private Vector3 GetSpawnPosition()
    {
        // 尝试找到合适位置的最大次数
        int maxAttempts = 30;
        Vector3 position = Vector3.zero;

        for (int i = 0; i < maxAttempts; i++)
        {
            // 在玩家周围的圆形区域随机一个点
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 potentialPosition;

            if (playerTransform != null)
            {
                // 以玩家为中心
                potentialPosition = new Vector3(
                    playerTransform.position.x + randomCircle.x,
                    0.5f, // 高度设置为略高于地面
                    playerTransform.position.z + randomCircle.y
                );
            }
            else
            {
                // 以生成器为中心
                potentialPosition = new Vector3(
                    transform.position.x + randomCircle.x,
                    0.5f,
                    transform.position.z + randomCircle.y
                );
            }

            // 检查是否符合距离条件
            if (IsValidSpawnPosition(potentialPosition))
            {
                position = potentialPosition;
                break;
            }
        }

        return position;
    }

    // 检查生成位置是否有效
    private bool IsValidSpawnPosition(Vector3 position)
    {
        // 检查与玩家的距离
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(position, playerTransform.position);
            if (distanceToPlayer < minDistanceFromPlayer)
            {
                return false;
            }
        }

        // 检查与其他武器的距离
        foreach (GameObject weapon in activeWeapons)
        {
            if (weapon == null) continue;

            float distanceToWeapon = Vector3.Distance(position, weapon.transform.position);
            if (distanceToWeapon < minDistanceFromWeapons)
            {
                return false;
            }
        }

        // 简化的地面检测 - 不使用标签检查，只要射线击中任何碰撞体就认为是有效位置
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 5f, Vector3.down, out hit, 10f))
        {
            // 任何碰撞都视为有效地面
            position.y = hit.point.y + 0.5f;
            return true;
        }

        return false;
    }

    // 在Scene视图中显示生成区域
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, minDistanceFromPlayer);
        }
    }

    // 停止生成
    public void StopSpawning()
    {
        isSpawning = false;
    }

    // 恢复生成
    public void ResumeSpawning()
    {
        if (!isSpawning)
        {
            StartCoroutine(SpawnRoutine());
        }
    }
}