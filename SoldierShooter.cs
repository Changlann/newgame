using UnityEngine;

public class SoldierShooter : MonoBehaviour
{
    [Header("基本设置")]
    public GameObject bulletPrefab;
    public float fireRate = 1f;
    public float detectionRange = 15f;
    public int bulletDamage = 15;

    [Header("高级射击设置")]
    public float bulletSpeed = 20f;
    public int bulletsPerShot = 1;  // 每次射击发射的子弹数量
    public float spreadAngle = 5f;  // 散射角度

    [Header("武器系统")]
    public WeaponData currentWeapon;  // 当前使用的武器数据
    public Transform shootPoint;  // 射击点

    [Header("音效设置")]
    public AudioClip shootSound;  // 射击音效
    [Range(0f, 1f)]
    public float shootVolume = 0.5f;  // 射击音量

    [Header("视觉效果")]
    public GameObject muzzleFlashPrefab;  // 枪口闪光效果

    private float nextFireTime;
    private Transform currentTarget;
    private bool isInitialized = false;
    private AudioSource audioSource;
    private WeaponHolder weaponHolder;  // 持有武器的组件引用

    void Start()
    {
        // 初始化音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D音效
            audioSource.maxDistance = 20f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }

        // 获取武器持有者组件
        weaponHolder = GetComponent<WeaponHolder>();

        // 检查子弹预制体
        if (bulletPrefab == null)
        {
            SnakeBody snakeBody = FindObjectOfType<SnakeBody>();
            if (snakeBody != null)
            {
                bulletPrefab = snakeBody.bulletPrefab;
                Debug.Log($"Retrieved bullet prefab from SnakeBody: {(bulletPrefab != null ? "Success" : "Failed")}");
            }
        }

        if (bulletPrefab == null)
        {
            Debug.LogError($"Bullet Prefab not set on {gameObject.name}");
            enabled = false;
            return;
        }

        // 确保有射击点
        if (shootPoint == null)
        {
            shootPoint = transform;
        }

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        // 如果有武器数据，使用武器数据更新参数
        if (weaponHolder != null && weaponHolder.HasWeapon())
        {
            currentWeapon = weaponHolder.GetWeaponData();

            if (currentWeapon != null)
            {
                // 更新射击参数
                fireRate = currentWeapon.fireRate;
                bulletDamage = currentWeapon.damage;
                detectionRange = currentWeapon.range;
                bulletSpeed = currentWeapon.bulletSpeed;
                bulletsPerShot = currentWeapon.bulletsPerShot;
                spreadAngle = currentWeapon.spreadAngle;

                // 更新音效
                if (currentWeapon.shootSound != null)
                {
                    shootSound = currentWeapon.shootSound;
                    shootVolume = currentWeapon.shootVolume;
                }
            }
        }

        FindNearestEnemy();

        if (currentTarget != null && Time.time > nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;  // 使用击发间隔而非固定时间
        }
    }

    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float minDistance = Mathf.Infinity;
        currentTarget = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < detectionRange && distance < minDistance)
                {
                    minDistance = distance;
                    currentTarget = enemy.transform;
                }
            }
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || currentTarget == null) return;

        try
        {
            // 基本射击方向（朝向目标）
            Vector3 shootDirection = (currentTarget.position - shootPoint.position).normalized;

            // 发射多颗子弹（散射）
            for (int i = 0; i < bulletsPerShot; i++)
            {
                // 计算散射角度
                Vector3 spreadDirection = shootDirection;
                if (spreadAngle > 0 && bulletsPerShot > 1)
                {
                    // 计算随机散射偏移
                    float randomSpread = Random.Range(-spreadAngle, spreadAngle);
                    Vector3 randomDir = Random.insideUnitSphere;
                    Quaternion spreadRot = Quaternion.AngleAxis(randomSpread, Vector3.Cross(shootDirection, randomDir).normalized);
                    spreadDirection = spreadRot * shootDirection;
                }

                // 子弹生成位置（从射击点稍微偏移，避免碰撞问题）
                Vector3 spawnPosition = shootPoint.position + (spreadDirection * 0.5f);

                // 实例化子弹
                GameObject bullet = Instantiate(
                    currentWeapon != null && currentWeapon.projectilePrefab != null ?
                    currentWeapon.projectilePrefab : bulletPrefab,
                    spawnPosition,
                    Quaternion.LookRotation(spreadDirection)
                );

                // 设置子弹属性
                SetupBullet(bullet, spreadDirection);
            }

            // 播放枪口闪光
            if (muzzleFlashPrefab != null || (currentWeapon != null && currentWeapon.muzzleFlashPrefab != null))
            {
                GameObject flashPrefab = currentWeapon != null && currentWeapon.muzzleFlashPrefab != null ?
                                         currentWeapon.muzzleFlashPrefab : muzzleFlashPrefab;

                GameObject flash = Instantiate(flashPrefab, shootPoint.position, Quaternion.LookRotation(shootDirection));
                Destroy(flash, 0.1f);  // 短暂显示后销毁
            }

            // 播放射击音效
            if (shootSound != null && audioSource != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);  // 轻微随机音调
                audioSource.PlayOneShot(shootSound, shootVolume);
            }

            Debug.Log($"{gameObject.name} fired at {currentTarget.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in Shoot() for {gameObject.name}: {e.Message}");
        }
    }

    // 设置子弹属性
    private void SetupBullet(GameObject bullet, Vector3 direction)
    {
        // 基本Bullet脚本（兼容旧版本）
        Bullet basicBullet = bullet.GetComponent<Bullet>();
        if (basicBullet != null)
        {
            basicBullet.SetDamage(bulletDamage);
            basicBullet.speed = bulletSpeed;
        }

        // 高级Projectile脚本（新版本）
        Projectile projectile = bullet.GetComponent<Projectile>();
        if (projectile != null)
        {
            if (currentWeapon != null)
            {
                // 使用武器数据设置子弹属性
                projectile.ApplyWeaponData(currentWeapon);
            }
            else
            {
                // 使用默认设置
                projectile.SetDamage(bulletDamage);
                projectile.speed = bulletSpeed;
            }
        }

        // 如果没有刚体，添加初始速度
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.velocity = direction * bulletSpeed;
        }
    }

    public void SetBulletPrefab(GameObject prefab)
    {
        if (prefab != null)
        {
            bulletPrefab = prefab;
            isInitialized = true;
            enabled = true;
            Debug.Log($"Bullet prefab set on {gameObject.name}");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}