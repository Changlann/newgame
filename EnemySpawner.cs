using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("��������")]
    public GameObject enemyPrefab;
    public int maxEnemies = 10;
    public float spawnRadius = 20f;
    public float spawnInterval = 5f;

    [Header("��������")]
    public int enemyDamage = 1;      // ���������Ƶ����˺�ֵ
    public int enemyHealth = 100;     // ���������Ƶ�������ֵ

    private bool isSpawning = false;
    private Coroutine spawnCoroutine;

    void Start()
    {
        // ���Ԥ�����Ƿ�����
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab not set on EnemySpawner!");
            enabled = false;
            return;
        }

        StartSpawning();
    }

    void OnEnable()
    {
        if (enemyPrefab != null && !isSpawning)
        {
            StartSpawning();
        }
    }

    void OnDisable()
    {
        StopSpawning();
    }

    void StartSpawning()
    {
        if (!isSpawning && enemyPrefab != null)
        {
            isSpawning = true;
            spawnCoroutine = StartCoroutine(SpawnEnemies());
        }
    }

    void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        isSpawning = false;
    }

    IEnumerator SpawnEnemies()
    {
        while (isSpawning && enemyPrefab != null)
        {
            try
            {
                // ����������
                GameObject[] existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
                if (existingEnemies.Length < maxEnemies)
                {
                    Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                    Vector3 spawnPos = new Vector3(
                        transform.position.x + randomCircle.x,
                        0,
                        transform.position.z + randomCircle.y
                    );

                    // �������λ���Ƿ����
                    bool isClear = true;
                    foreach (GameObject enemy in existingEnemies)
                    {
                        if (enemy != null && Vector3.Distance(enemy.transform.position, spawnPos) < 2f)
                        {
                            isClear = false;
                            break;
                        }
                    }

                    if (isClear)
                    {
                        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

                        // ���õ�������
                        EnemyAI enemyAI = newEnemy.GetComponent<EnemyAI>();
                        if (enemyAI != null)
                        {
                            enemyAI.SetDamageAmount(enemyDamage); // ʹ�����õ��˺�ֵ
                        }

                        Health health = newEnemy.GetComponent<Health>();
                        if (health != null)
                        {
                            health.maxHealth = enemyHealth;
                            health.currentHealth = enemyHealth;
                        }

                        newEnemy.tag = "Enemy";
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in EnemySpawner: {e.Message}");
                StopSpawning();
                break;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void OnValidate()
    {
        // ȷ�������ں���Χ��
        maxEnemies = Mathf.Max(1, maxEnemies);
        spawnRadius = Mathf.Max(1f, spawnRadius);
        spawnInterval = Mathf.Max(0.1f, spawnInterval);
        enemyDamage = Mathf.Max(1, enemyDamage);
        enemyHealth = Mathf.Max(1, enemyHealth);
    }

    void OnDrawGizmosSelected()
    {
        // ��Scene��ͼ����ʾ���ɷ�Χ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}