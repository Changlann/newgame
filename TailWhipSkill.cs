using System.Collections;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 蛇尾甩动攻击技能
/// </summary>
public class TailWhipSkill : ActiveSkill
{
    [Header("甩尾设置")]
    [Tooltip("甩尾攻击持续时间（秒）")]
    public float whipDuration = 1.5f;

    [Tooltip("甩尾无敌状态持续时间（秒）")]
    public float invincibleDuration = 2f;

    [Tooltip("基础伤害值")]
    public int baseDamage = 30;

    [Tooltip("每级增加的伤害")]
    public int damagePerLevel = 10;

    [Tooltip("速度伤害系数（伤害 = 基础伤害 + 移动速度 * 系数）")]
    public float speedDamageMultiplier = 5f;

    [Tooltip("攻击判定半径")]
    public float attackRadius = 2.5f;

    [Tooltip("攻击特效预制体")]
    public GameObject whipEffectPrefab;

    [Tooltip("蛇尾部分变色（攻击状态）")]
    public Color whipActiveColor = new Color(1f, 0.5f, 0f);

    // 引用
    private SnakeBody snakeBody;
    private List<Health> affectedEnemies = new List<Health>();

    // 状态
    private bool isWhipping = false;

    protected override void Awake()
    {
        base.Awake();

        // 设置默认值
        skillName = "蛇尾甩动";
        description = "使蛇尾进入短暂的无敌状态，并对接触到的敌人造成伤害。伤害与蛇尾移动速度有关。";
        skillKey = KeyCode.Q;

        // 获取SnakeBody组件
        snakeBody = GetComponent<SnakeBody>();
        if (snakeBody == null)
        {
            snakeBody = FindObjectOfType<SnakeBody>();
        }

        if (snakeBody == null)
        {
            Debug.LogError("[TailWhipSkill] 无法找到SnakeBody组件！");
            enabled = false;
        }
    }

    protected override void Execute()
    {
        if (isWhipping || snakeBody == null) return;

        StartCoroutine(PerformTailWhip());
    }

    /// <summary>
    /// 执行蛇尾甩动攻击
    /// </summary>
    private IEnumerator PerformTailWhip()
    {
        isWhipping = true;
        affectedEnemies.Clear();

        // 获取所有蛇身部分
        Transform[] bodyParts = snakeBody.GetBodyPartTransforms();
        if (bodyParts.Length == 0)
        {
            Debug.LogWarning("[TailWhipSkill] 没有蛇身部分可用于甩尾攻击");
            isWhipping = false;
            yield break;
        }

        // 设置蛇尾无敌状态
        foreach (Transform bodyPart in bodyParts)
        {
            Health health = bodyPart.GetComponent<Health>();
            if (health != null)
            {
                health.SetInvincible(invincibleDuration);
            }

            // 更改蛇尾颜色以指示激活状态
            MeshRenderer renderer = bodyPart.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                // 保存原始颜色
                Color originalColor = renderer.material.color;

                // 设置攻击激活颜色
                renderer.material.color = whipActiveColor;

                // 在协程结束时恢复原始颜色
                StartCoroutine(RestoreColor(renderer, originalColor, whipDuration));
            }
        }

        // 创建攻击特效（如果有）
        if (whipEffectPrefab != null && bodyParts.Length > 0)
        {
            Instantiate(whipEffectPrefab, bodyParts[bodyParts.Length - 1].position, Quaternion.identity);
        }

        // 在甩尾持续时间内检测碰撞
        float elapsed = 0f;
        while (elapsed < whipDuration)
        {
            elapsed += Time.deltaTime;

            // 遍历所有蛇身部分检测敌人
            foreach (Transform bodyPart in bodyParts)
            {
                // 计算这一帧的移动速度
                Vector3 currentPos = bodyPart.position;
                float deltaTime = Time.deltaTime;
                yield return null; // 等待下一帧
                Vector3 newPos = bodyPart.position;
                float speed = Vector3.Distance(currentPos, newPos) / deltaTime;

                // 检测周围的敌人
                Collider[] colliders = Physics.OverlapSphere(bodyPart.position, attackRadius);
                foreach (Collider collider in colliders)
                {
                    if (collider.CompareTag("Enemy"))
                    {
                        Health enemyHealth = collider.GetComponent<Health>();
                        if (enemyHealth != null && !affectedEnemies.Contains(enemyHealth))
                        {
                            // 计算伤害（基础伤害 + 等级加成 + 速度加成）
                            int damage = baseDamage + (level - 1) * damagePerLevel + Mathf.RoundToInt(speed * speedDamageMultiplier);

                            // 对敌人造成伤害
                            enemyHealth.TakeDamage(damage, true);

                            // 添加到已影响列表，防止重复伤害
                            affectedEnemies.Add(enemyHealth);

                            Debug.Log($"[TailWhipSkill] 甩尾攻击命中敌人，造成 {damage} 点伤害（速度：{speed:F2}）");
                        }
                    }
                }
            }

            yield return null;
        }

        isWhipping = false;
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
    /// 在场景视图中显示攻击范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        SnakeBody snakeBody = GetComponent<SnakeBody>();
        if (snakeBody == null) return;

        Transform[] bodyParts = snakeBody.GetBodyPartTransforms();
        if (bodyParts == null || bodyParts.Length == 0) return;

        Gizmos.color = Color.red;
        foreach (Transform bodyPart in bodyParts)
        {
            Gizmos.DrawWireSphere(bodyPart.position, attackRadius);
        }
    }
}