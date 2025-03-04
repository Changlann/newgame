using UnityEngine;

public class SoldierShooter : MonoBehaviour
{
    [Header("��������")]
    public GameObject bulletPrefab;
    public float fireRate = 1f;
    public float detectionRange = 15f;
    public int bulletDamage = 15;

    [Header("�߼��������")]
    public float bulletSpeed = 20f;
    public int bulletsPerShot = 1;  // ÿ�����������ӵ�����
    public float spreadAngle = 5f;  // ɢ��Ƕ�

    [Header("����ϵͳ")]
    public WeaponData currentWeapon;  // ��ǰʹ�õ���������
    public Transform shootPoint;  // �����

    [Header("��Ч����")]
    public AudioClip shootSound;  // �����Ч
    [Range(0f, 1f)]
    public float shootVolume = 0.5f;  // �������

    [Header("�Ӿ�Ч��")]
    public GameObject muzzleFlashPrefab;  // ǹ������Ч��

    private float nextFireTime;
    private Transform currentTarget;
    private bool isInitialized = false;
    private AudioSource audioSource;
    private WeaponHolder weaponHolder;  // �����������������

    void Start()
    {
        // ��ʼ����ƵԴ
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D��Ч
            audioSource.maxDistance = 20f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }

        // ��ȡ�������������
        weaponHolder = GetComponent<WeaponHolder>();

        // ����ӵ�Ԥ����
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

        // ȷ���������
        if (shootPoint == null)
        {
            shootPoint = transform;
        }

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        // ������������ݣ�ʹ���������ݸ��²���
        if (weaponHolder != null && weaponHolder.HasWeapon())
        {
            currentWeapon = weaponHolder.GetWeaponData();

            if (currentWeapon != null)
            {
                // �����������
                fireRate = currentWeapon.fireRate;
                bulletDamage = currentWeapon.damage;
                detectionRange = currentWeapon.range;
                bulletSpeed = currentWeapon.bulletSpeed;
                bulletsPerShot = currentWeapon.bulletsPerShot;
                spreadAngle = currentWeapon.spreadAngle;

                // ������Ч
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
            nextFireTime = Time.time + 1f / fireRate;  // ʹ�û���������ǹ̶�ʱ��
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
            // ����������򣨳���Ŀ�꣩
            Vector3 shootDirection = (currentTarget.position - shootPoint.position).normalized;

            // �������ӵ���ɢ�䣩
            for (int i = 0; i < bulletsPerShot; i++)
            {
                // ����ɢ��Ƕ�
                Vector3 spreadDirection = shootDirection;
                if (spreadAngle > 0 && bulletsPerShot > 1)
                {
                    // �������ɢ��ƫ��
                    float randomSpread = Random.Range(-spreadAngle, spreadAngle);
                    Vector3 randomDir = Random.insideUnitSphere;
                    Quaternion spreadRot = Quaternion.AngleAxis(randomSpread, Vector3.Cross(shootDirection, randomDir).normalized);
                    spreadDirection = spreadRot * shootDirection;
                }

                // �ӵ�����λ�ã����������΢ƫ�ƣ�������ײ���⣩
                Vector3 spawnPosition = shootPoint.position + (spreadDirection * 0.5f);

                // ʵ�����ӵ�
                GameObject bullet = Instantiate(
                    currentWeapon != null && currentWeapon.projectilePrefab != null ?
                    currentWeapon.projectilePrefab : bulletPrefab,
                    spawnPosition,
                    Quaternion.LookRotation(spreadDirection)
                );

                // �����ӵ�����
                SetupBullet(bullet, spreadDirection);
            }

            // ����ǹ������
            if (muzzleFlashPrefab != null || (currentWeapon != null && currentWeapon.muzzleFlashPrefab != null))
            {
                GameObject flashPrefab = currentWeapon != null && currentWeapon.muzzleFlashPrefab != null ?
                                         currentWeapon.muzzleFlashPrefab : muzzleFlashPrefab;

                GameObject flash = Instantiate(flashPrefab, shootPoint.position, Quaternion.LookRotation(shootDirection));
                Destroy(flash, 0.1f);  // ������ʾ������
            }

            // ���������Ч
            if (shootSound != null && audioSource != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);  // ��΢�������
                audioSource.PlayOneShot(shootSound, shootVolume);
            }

            Debug.Log($"{gameObject.name} fired at {currentTarget.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in Shoot() for {gameObject.name}: {e.Message}");
        }
    }

    // �����ӵ�����
    private void SetupBullet(GameObject bullet, Vector3 direction)
    {
        // ����Bullet�ű������ݾɰ汾��
        Bullet basicBullet = bullet.GetComponent<Bullet>();
        if (basicBullet != null)
        {
            basicBullet.SetDamage(bulletDamage);
            basicBullet.speed = bulletSpeed;
        }

        // �߼�Projectile�ű����°汾��
        Projectile projectile = bullet.GetComponent<Projectile>();
        if (projectile != null)
        {
            if (currentWeapon != null)
            {
                // ʹ���������������ӵ�����
                projectile.ApplyWeaponData(currentWeapon);
            }
            else
            {
                // ʹ��Ĭ������
                projectile.SetDamage(bulletDamage);
                projectile.speed = bulletSpeed;
            }
        }

        // ���û�и��壬��ӳ�ʼ�ٶ�
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