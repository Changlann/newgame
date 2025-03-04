using UnityEngine;

/// <summary>
/// Boss掉落处理 - 处理Boss死亡后掉落技能
/// </summary>
public class BossDropHandler : MonoBehaviour
{
    [Header("掉落设置")]
    [Tooltip("是否是Boss")]
    public bool isBoss = true;

    [Tooltip("普通敌人掉落技能的概率")]
    [Range(0f, 1f)]
    public float dropChance = 0.05f;

    [Tooltip("掉落特效预制体")]
    public GameObject dropEffectPrefab;

    // 引用
    private Health health;

    void Start()
    {
        health = GetComponent<Health>();
        if (health == null)
        {
            Debug.LogError("[BossDropHandler] 没有Health组件！");
            enabled = false;
            return;
        }

        // 注册死亡事件
        health.onDeath.AddListener(OnEnemyDeath);
    }

    /// <summary>
    /// 敌人死亡处理
    /// </summary>
    private void OnEnemyDeath()
    {
        if (ActiveSkillManager.Instance == null) return;

        // Boss必定掉落技能，普通敌人按概率掉落
        bool shouldDrop = isBoss || (Random.value < dropChance);

        if (shouldDrop)
        {
            // 播放掉落特效
            if (dropEffectPrefab != null)
            {
                Instantiate(dropEffectPrefab, transform.position, Quaternion.identity);
            }

            // 生成技能拾取物
            ActiveSkillManager.Instance.SpawnSkillPickup(transform.position + Vector3.up * 0.5f);

            Debug.Log($"[BossDropHandler] {(isBoss ? "Boss" : "敌人")}掉落了技能!");
        }
    }
}