using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// �˽ű������ڳ�������������ʰȡ��
public class WeaponSpawner : MonoBehaviour
{
    [Header("��������")]
    [Tooltip("�������������б�")]
    public List<WeaponData> availableWeapons = new List<WeaponData>(); // ������������
    [Tooltip("����ʰȡ��Ԥ����")]
    public GameObject weaponPickupPrefab; // ����ʰȡ��Ԥ����
    [Tooltip("�����������������")]
    public int maxWeapons = 5; // �����������������
    [Tooltip("���ɼ��ʱ��(��)")]
    public float spawnInterval = 10f; // ���ɼ��
    [Tooltip("��ʼ�ӳ�ʱ��(��)")]
    public float initialDelay = 5f; // ��ʼ�ӳ�

    [Header("��������")]
    [Tooltip("���ɰ뾶��Χ")]
    public float spawnRadius = 30f; // ���ɰ뾶
    [Tooltip("������ҵ���С����")]
    public float minDistanceFromPlayer = 8f; // ������ҵ���С����
    [Tooltip("����֮�����С����")]
    public float minDistanceFromWeapons = 5f; // ����֮�����С����

    [Header("����")]
    [Tooltip("��ʾ������Ϣ")]
    public bool showDebugInfo = true;

    private Transform playerTransform;
    private List<GameObject> activeWeapons = new List<GameObject>();
    private bool isSpawning = false;

    void Start()
    {
        // �������
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("[WeaponSpawner] �Ҳ�����Ҷ�������λ�ÿ��ܲ����룡");
        }

        // �������
        if (availableWeapons.Count == 0)
        {
            Debug.LogError("[WeaponSpawner] û�����ÿ�������������Inspector������������ݣ�");
            enabled = false;
            return;
        }

        if (weaponPickupPrefab == null)
        {
            Debug.LogError("[WeaponSpawner] δ��������ʰȡ��Ԥ���壡");
            enabled = false;
            return;
        }

        // ��ʼ����ѭ��
        StartCoroutine(SpawnRoutine());
    }

    // ��������ѭ��
    private IEnumerator SpawnRoutine()
    {
        isSpawning = true;

        // ��ʼ�ӳ�
        yield return new WaitForSeconds(initialDelay);

        while (isSpawning)
        {
            // ������Ч����������
            activeWeapons.RemoveAll(weapon => weapon == null);

            // �����������δ�����ޣ���������
            if (activeWeapons.Count < maxWeapons)
            {
                SpawnWeapon();
            }

            // �ȴ���һ������
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // ����һ������
    private void SpawnWeapon()
    {
        // ���ѡ��һ����������
        WeaponData selectedWeapon = availableWeapons[Random.Range(0, availableWeapons.Count)];
        if (selectedWeapon == null)
        {
            Debug.LogWarning("[WeaponSpawner] ѡ�е���������Ϊ�գ�");
            return;
        }

        // ��ȡ����λ��
        Vector3 spawnPosition = GetSpawnPosition();
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("[WeaponSpawner] �޷��ҵ����ʵ�����λ�ã������������ɣ�");
            return;
        }

        // ʵ��������ʰȡ��
        GameObject weaponInstance = Instantiate(weaponPickupPrefab, spawnPosition, Quaternion.identity);

        // ������������
        WeaponPickup pickup = weaponInstance.GetComponent<WeaponPickup>();
        if (pickup != null)
        {
            pickup.weaponData = selectedWeapon;
        }
        else
        {
            Debug.LogError("[WeaponSpawner] ����ʰȡ��Ԥ����û��WeaponPickup�����");
            Destroy(weaponInstance);
            return;
        }

        // ��ӵ�������б�
        activeWeapons.Add(weaponInstance);

        if (showDebugInfo)
        {
            Debug.Log($"[WeaponSpawner] ��������: {selectedWeapon.weaponName} ��λ��: {spawnPosition}");
        }
    }

    // ��ȡ���ʵ�����λ��
    private Vector3 GetSpawnPosition()
    {
        // �����ҵ�����λ�õ�������
        int maxAttempts = 30;
        Vector3 position = Vector3.zero;

        for (int i = 0; i < maxAttempts; i++)
        {
            // �������Χ��Բ���������һ����
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 potentialPosition;

            if (playerTransform != null)
            {
                // �����Ϊ����
                potentialPosition = new Vector3(
                    playerTransform.position.x + randomCircle.x,
                    0.5f, // �߶�����Ϊ�Ը��ڵ���
                    playerTransform.position.z + randomCircle.y
                );
            }
            else
            {
                // ��������Ϊ����
                potentialPosition = new Vector3(
                    transform.position.x + randomCircle.x,
                    0.5f,
                    transform.position.z + randomCircle.y
                );
            }

            // ����Ƿ���Ͼ�������
            if (IsValidSpawnPosition(potentialPosition))
            {
                position = potentialPosition;
                break;
            }
        }

        return position;
    }

    // �������λ���Ƿ���Ч
    private bool IsValidSpawnPosition(Vector3 position)
    {
        // �������ҵľ���
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(position, playerTransform.position);
            if (distanceToPlayer < minDistanceFromPlayer)
            {
                return false;
            }
        }

        // ��������������ľ���
        foreach (GameObject weapon in activeWeapons)
        {
            if (weapon == null) continue;

            float distanceToWeapon = Vector3.Distance(position, weapon.transform.position);
            if (distanceToWeapon < minDistanceFromWeapons)
            {
                return false;
            }
        }

        // �򻯵ĵ����� - ��ʹ�ñ�ǩ��飬ֻҪ���߻����κ���ײ�����Ϊ����Чλ��
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 5f, Vector3.down, out hit, 10f))
        {
            // �κ���ײ����Ϊ��Ч����
            position.y = hit.point.y + 0.5f;
            return true;
        }

        return false;
    }

    // ��Scene��ͼ����ʾ��������
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

    // ֹͣ����
    public void StopSpawning()
    {
        isSpawning = false;
    }

    // �ָ�����
    public void ResumeSpawning()
    {
        if (!isSpawning)
        {
            StartCoroutine(SpawnRoutine());
        }
    }
}