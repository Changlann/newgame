using UnityEngine;using System.Collections;

// 此组件附加到每个士兵(蛇尾)上，使其能够持有和使用武器
public class WeaponHolder : MonoBehaviour{    [Header("武器设置")]    [Tooltip("武器挂载点")]    public Transform weaponSocket; // 武器挂载点
    [Tooltip("当前装备的武器数据")]    public WeaponData equippedWeaponData; // 当前装备的武器数据

    [Header("状态")]    [Tooltip("是否持有武器")]    [SerializeField] private bool hasWeapon = false;

    private GameObject weaponInstance; // 实例化的武器模型
    private SoldierShooter shooter; // 引用射击组件
    private float nextFireTime = 0f;

    [Header("视觉效果")]    [Tooltip("装备武器时播放的特效")]    public GameObject weaponPickupEffect; // 装备武器时播放的特效

    // 获取下一个WeaponHolder(更靠近蛇尾的下一节)
    private WeaponHolder nextHolder;    void Start()    {        shooter = GetComponent<SoldierShooter>();        if (shooter == null)        {            Debug.LogError($"[WeaponHolder] {gameObject.name} 没有找到SoldierShooter组件!");        }

        // 在开始时尝试查找下一个持有者
        FindNextHolder();    }

    // 尝试装备武器
    public bool EquipWeapon(WeaponData weaponData)    {
        // 如果已经有武器，尝试传递给下一个士兵
        if (hasWeapon)        {
            // 如果有下一个持有者，尝试把武器传递下去
            if (nextHolder != null)            {                return nextHolder.EquipWeapon(weaponData);            }            return false; // 没有可用的下一个持有者
        }

        // 装备武器
        equippedWeaponData = weaponData;        hasWeapon = true;

        // 更新射击组件参数
        UpdateShooterParameters();

        // 实例化武器模型
        InstantiateWeaponModel();

        // 播放获取武器特效
        PlayWeaponPickupEffect();

        Debug.Log($"[WeaponHolder] {gameObject.name} 装备了武器: {weaponData.weaponName}");        return true;    }

    // 移除武器
    public WeaponData RemoveWeapon()    {        if (!hasWeapon) return null;

        WeaponData removedWeapon = equippedWeaponData;        equippedWeaponData = null;        hasWeapon = false;

        // 销毁武器模型
        if (weaponInstance != null)        {            Destroy(weaponInstance);            weaponInstance = null;        }

        // 重置射击组件
        ResetShooterParameters();

        Debug.Log($"[WeaponHolder] {gameObject.name} 移除了武器");        return removedWeapon;    }

    // 查找并设置下一个持有者
    private void FindNextHolder()    {
        // 获取当前蛇身的索引(通过父对象查找子对象列表)
        Transform parent = transform.parent;        if (parent == null) return;

        int currentIndex = -1;        for (int i = 0; i < parent.childCount; i++)        {            if (parent.GetChild(i) == transform)            {                currentIndex = i;                break;            }        }

        // 查找下一个具有WeaponHolder组件的子对象
        if (currentIndex >= 0 && currentIndex < parent.childCount - 1)        {            for (int i = currentIndex + 1; i < parent.childCount; i++)            {                WeaponHolder holder = parent.GetChild(i).GetComponent<WeaponHolder>();                if (holder != null)                {                    nextHolder = holder;                    return;                }            }        }

















        // 这部分可以省略，或者根据你实际的蛇身结构来调整
        // 如果前面的查找方法已经足够，可以直接注释掉这段代码
        /*        SnakeBody snakeBody = FindObjectOfType<SnakeBody>();        if (snakeBody != null)        {            // A根据你的SnakeBody实现方式自定义查找逻辑            // 例如，如果你的蛇身部分都是SnakeBody的子对象，可以直接遍历其子对象            // 或者使用其他方式获取蛇身部分列表        }        */
    }

    // 实例化武器模型
    private void InstantiateWeaponModel()    {        if (equippedWeaponData == null || equippedWeaponData.weaponPrefab == null) return;

        // 先销毁旧的武器模型
        if (weaponInstance != null)        {            Destroy(weaponInstance);        }

        // 确保有挂载点
        if (weaponSocket == null)        {            weaponSocket = transform; // 默认使用自身作为挂载点
        }

        // 实例化新武器
        weaponInstance = Instantiate(equippedWeaponData.weaponPrefab, weaponSocket);        weaponInstance.transform.localPosition = Vector3.zero;        weaponInstance.transform.localRotation = Quaternion.identity;    }

    // 更新射击组件参数
    private void UpdateShooterParameters()    {        if (shooter == null || equippedWeaponData == null) return;

        shooter.fireRate = equippedWeaponData.fireRate;        shooter.bulletDamage = equippedWeaponData.damage;        shooter.detectionRange = equippedWeaponData.range;

        // 如果有自定义子弹预制体，设置它
        if (equippedWeaponData.projectilePrefab != null)        {            shooter.SetBulletPrefab(equippedWeaponData.projectilePrefab);        }    }

    // 重置射击组件参数
    private void ResetShooterParameters()    {        if (shooter == null) return;

        // 恢复默认参数
        shooter.fireRate = 1f;        shooter.bulletDamage = 15;        shooter.detectionRange = 15f;

        // 恢复默认子弹预制体
        SnakeBody snakeBody = FindObjectOfType<SnakeBody>();        if (snakeBody != null && snakeBody.bulletPrefab != null)        {            shooter.SetBulletPrefab(snakeBody.bulletPrefab);        }    }

    // 播放获取武器特效
    private void PlayWeaponPickupEffect()    {        if (weaponPickupEffect == null) return;

        GameObject effect = Instantiate(weaponPickupEffect, transform.position, Quaternion.identity);        Destroy(effect, 2f); // 2秒后销毁特效
    }

    // 公开方法以检查是否有武器
    public bool HasWeapon()    {        return hasWeapon;    }

    // 公开方法获取当前武器数据
    public WeaponData GetWeaponData()    {        return equippedWeaponData;    }

    // 公开方法获取下一个持有者
    public WeaponHolder GetNextHolder()    {        return nextHolder;    }}