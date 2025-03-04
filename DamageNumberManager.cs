using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    public GameObject damageNumberPrefab;  // 伤害数字预制体
    public Canvas worldSpaceCanvas;        // 世界空间Canvas
    public Vector3 offset = new Vector3(0, 2f, 0);  // 偏移量，可在 Inspector 中调整

    private static DamageNumberManager instance;
    public static DamageNumberManager Instance { get { return instance; } }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDamageNumber(int damage, Vector3 position)
    {
        if (damageNumberPrefab == null || worldSpaceCanvas == null)
        {
            Debug.LogError("缺少伤害数字预制体或Canvas引用，请检查DamageNumberManager设置！");
            return;
        }

        // 使用 offset 计算生成位置
        Vector3 spawnPosition = position + offset;

        // 生成伤害数字，设置为Canvas的子对象
        GameObject numberObj = Instantiate(damageNumberPrefab, spawnPosition,
            Quaternion.identity, worldSpaceCanvas.transform);

        // 让文字始终面向摄像机
        numberObj.transform.forward = Camera.main.transform.forward;

        DamageNumber damageNumber = numberObj.GetComponent<DamageNumber>();
        if (damageNumber != null)
        {
            damageNumber.SetDamageText(damage);
        }
        else
        {
            Debug.LogError("伤害数字预制体缺少DamageNumber脚本！");
        }
    }
}