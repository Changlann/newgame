// CubeSpawner.cs
using UnityEngine;
using System.Collections;

public class CubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public int maxCubes = 20;
    public float spawnRadius = 50f;
    public float spawnInterval = 2f;

    void Start()
    {
        StartCoroutine(SpawnCubes());
    }

    IEnumerator SpawnCubes()
    {
        while (true)
        {
            if (GameObject.FindGameObjectsWithTag("Collectable").Length < maxCubes)
            {
                Vector3 spawnPos = GetRandomPosition();
                Instantiate(cubePrefab, spawnPos, Quaternion.identity);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    Vector3 GetRandomPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = new Vector3(
            transform.position.x + randomCircle.x,
            0.5f, // 确保生成在地面上方
            transform.position.z + randomCircle.y
        );

        // 防止生成在玩家起始位置
        while (Vector3.Distance(pos, Vector3.zero) < 5f)
        {
            randomCircle = Random.insideUnitCircle * spawnRadius;
            pos = new Vector3(
                transform.position.x + randomCircle.x,
                0.5f,
                transform.position.z + randomCircle.y
            );
        }
        return pos;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}