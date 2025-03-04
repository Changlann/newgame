using System.Collections;
using UnityEngine;

/// <summary>
/// 群体治疗技能 - 治疗所有蛇身部分并提供临时护盾
/// </summary>
public class MassHealSkill : ActiveSkill
{
    [Header("治疗设置")]
    [Tooltip("基础治疗量（百分比最大生命值）")]
    [Range(0.1f, 1.0f)]
    public float baseHealPercent = 0.3f;

    [Tooltip("每级增加的治疗量")]
    [Range(0.05f, 0.2f)]
    public float healPercentPerLevel = 0.1f;

    [Tooltip("治疗范围")]
    public float healRadius = 15f;

    [Tooltip("临时护盾持续时间（秒）")]
    public float shieldDuration = 5f;

    [Tooltip("每级增加的护盾持续时间")]
    public float shieldDurationPerLevel = 1f;

    [Tooltip("护盾减伤比例")]
    [Range(0.1f, 0.9f)]
    public float damageReduction = 0.5f;

    [Tooltip("治疗特效预制体")]
    public GameObject healEffectPrefab;

    [Tooltip("护盾特效预制体")]
    public GameObject shieldEffectPrefab;

    [Tooltip("护盾状态颜色")]
    public Color shieldColor = new Color(0.5f, 1f, 0.5f, 0.7f);

    // 引用
    private SnakeBody snakeBody;

    protected override void Awake()
    {
        base.Awake();

        // 设置默认值
        skillName = "群体治疗";
        description = "治疗所有蛇身部分一定百分比的生命值，并提供临时伤害减免护盾。";
        skillKey = KeyCode.R; // 可以根据需要修改

        // 获取SnakeBody组件
        snakeBody = GetComponent<SnakeBody>();
        if (snakeBody == null)
        {
            snakeBody = FindObjectOfType<SnakeBody>();
        }

        if (snakeBody == null)
        {
            Debug.LogError("[MassHealSkill] 无法找到SnakeBody组件！");
            enabled = false;
        }
    }

    protected override void Execute()
    {
        if (snakeBody == null) return;

        StartCoroutine(PerformMassHeal());
    }

    /// <summary>
    /// 执行群体治疗
    /// </summary>
    private IEnumerator PerformMassHeal()
    {
        // 计算实际治疗量和护盾持续时间
        float actualHealPercent = baseHealPercent + (level - 1) * healPercentPerLevel;
        float actualShieldDuration = shieldDuration + (level - 1) * shieldDurationPerLevel;

        // 获取所有蛇身部分
        Transform[] bodyParts = snakeBody.GetBodyPartTransforms();

        if (bodyParts.Length == 0)
        {
            Debug.LogWarning("[MassHealSkill] 没有蛇身部分可以治疗");
            yield break;
        }

        // 在玩家位置创建治疗特效
        if (healEffectPrefab != null)
        {
            GameObject healEffect = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
            Destroy(healEffect, 3f);
        }

        // 为每个身体部分应用治疗和护盾
        foreach (Transform bodyPart in bodyParts)
        {
            Health health = bodyPart.GetComponent<Health>();
            if (health != null)
            {
                // 计算治疗量
                float healAmount = health.maxHealth * actualHealPercent;

                // 应用治疗
                health.Heal(healAmount);

                // 应用护盾效果（使用无敌状态模拟）
                health.SetInvincible(actualShieldDuration);

                // 为每个部分创建护盾特效
                if (shieldEffectPrefab != null)
                {
                    GameObject shieldEffect = Instantiate(shieldEffectPrefab, bodyPart.position, Quaternion.identity);
                    shieldEffect.transform.SetParent(bodyPart);
                    Destroy(shieldEffect, actualShieldDuration);
                }

                // 更改颜色以指示护盾状态
                MeshRenderer renderer = bodyPart.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    // 保存原始颜色
                    Color originalColor = renderer.material.color;

                    // 设置护盾颜色
                    renderer.material.color = Color.Lerp(originalColor, shieldColor, 0.5f);

                    // 在护盾结束时恢复原始颜色
                    StartCoroutine(RestoreColor(renderer, originalColor, actualShieldDuration));
                }
            }
        }

        Debug.Log($"[MassHealSkill] 群体治疗已施放，治疗 {(actualHealPercent * 100):F0}% 最大生命值，护盾持续 {actualShieldDuration} 秒");

        yield return null;
    }

    /// <summary>
    /// 恢复渲染器的原始颜色
    /// </summary>
    private IEnumerator RestoreColor(MeshRenderer renderer, Color originalColor, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (renderer != null)
        {
            renderer.material.color = originalColor;
        }
    }

    /// <summary>
    /// 在场景视图中显示治疗范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRadius);
    }
}