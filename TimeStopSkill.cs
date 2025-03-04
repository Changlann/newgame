using System.Collections;
using UnityEngine;
using UnityEngine.AI; // 添加此命名空间引用

/// <summary>
/// 时间停顿技能 - 暂停敌人移动并提高自身速度
/// </summary>
public class TimeStopSkill : ActiveSkill
{
    [Header("时间停顿设置")]
    [Tooltip("停顿持续时间（秒）")]
    public float stopDuration = 5f;

    [Tooltip("每级增加的持续时间（秒）")]
    public float durationPerLevel = 1f;

    [Tooltip("自身移动速度提升倍率")]
    public float speedBoostMultiplier = 1.5f;

    [Tooltip("每级增加的速度提升")]
    public float speedBoostPerLevel = 0.1f;

    [Tooltip("停顿特效预制体")]
    public GameObject timeStopEffectPrefab;

    [Tooltip("停顿状态下的颜色滤镜")]
    public Color timeStopFilterColor = new Color(0.8f, 0.8f, 1f, 0.3f);

    // 引用
    private HealerMovement playerMovement;
    private PostProcessingController postProcessing;

    // 状态变量
    private float originalMoveSpeed;
    private bool isTimeStopped = false;
    private GameObject timeStopEffect;

    protected override void Awake()
    {
        base.Awake();

        // 设置默认值
        skillName = "时间停顿";
        description = "短暂暂停所有敌人的移动，同时提高自身移动速度。";
        skillKey = KeyCode.E;

        // 获取必要组件
        playerMovement = GetComponent<HealerMovement>();
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<HealerMovement>();
        }

        postProcessing = FindObjectOfType<PostProcessingController>();
    }

    protected override void Execute()
    {
        if (isTimeStopped) return;

        StartCoroutine(PerformTimeStop());
    }

    /// <summary>
    /// 执行时间停顿
    /// </summary>
    private IEnumerator PerformTimeStop()
    {
        isTimeStopped = true;

        // 获取实际持续时间（基础 + 等级加成）
        float actualDuration = stopDuration + (level - 1) * durationPerLevel;

        // 获取实际速度提升（基础 + 等级加成）
        float actualSpeedBoost = speedBoostMultiplier + (level - 1) * speedBoostPerLevel;

        // 保存原始移动速度
        if (playerMovement != null)
        {
            originalMoveSpeed = playerMovement.moveSpeed;
            playerMovement.moveSpeed *= actualSpeedBoost;
            Debug.Log($"[TimeStopSkill] 提高移动速度到 {playerMovement.moveSpeed} （提升倍率：{actualSpeedBoost}）");
        }

        // 暂停所有敌人
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            if (enemy != null)
            {
                // 保存敌人组件状态
                NavMeshAgent navAgent = enemy.GetComponent<NavMeshAgent>();
                Rigidbody rb = enemy.GetComponent<Rigidbody>();

                if (navAgent != null)
                {
                    navAgent.isStopped = true;
                    navAgent.velocity = Vector3.zero;
                }

                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                // 禁用敌人脚本
                enemy.enabled = false;
            }
        }

        // 应用后处理效果
        if (postProcessing != null)
        {
            // 保存原始后处理设置
            float originalChromatic = 0;
            float originalVignette = 0;

            // 检查是否有色差组件
            if (postProcessing.GetComponent<UnityEngine.Rendering.PostProcessing.PostProcessVolume>()?.profile.TryGetSettings(out UnityEngine.Rendering.PostProcessing.ChromaticAberration chromaticAberration) ?? false)
            {
                originalChromatic = chromaticAberration.intensity.value;
                postProcessing.SetChromaticAberrationIntensity(0.8f);
            }

            // 检查是否有暗角组件
            if (postProcessing.GetComponent<UnityEngine.Rendering.PostProcessing.PostProcessVolume>()?.profile.TryGetSettings(out UnityEngine.Rendering.PostProcessing.Vignette vignette) ?? false)
            {
                originalVignette = vignette.intensity.value;
                postProcessing.SetVignetteIntensity(0.7f);
            }

            // 恢复后处理设置
            StartCoroutine(RestorePostProcessing(originalChromatic, originalVignette, actualDuration));
        }

        // 创建时间停顿特效
        if (timeStopEffectPrefab != null)
        {
            timeStopEffect = Instantiate(timeStopEffectPrefab, transform.position, Quaternion.identity);
            timeStopEffect.transform.SetParent(transform);
        }

        // 播放特效和音效
        Debug.Log($"[TimeStopSkill] 时间停顿激活，持续 {actualDuration} 秒");

        // 等待持续时间
        yield return new WaitForSeconds(actualDuration);

        // 恢复所有状态
        RestoreTimeFlow();
    }

    /// <summary>
    /// 恢复后处理效果
    /// </summary>
    private IEnumerator RestorePostProcessing(float originalChromatic, float originalVignette, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (postProcessing != null)
        {
            postProcessing.SetChromaticAberrationIntensity(originalChromatic);
            postProcessing.SetVignetteIntensity(originalVignette);
        }
    }

    /// <summary>
    /// 恢复时间流动（结束停顿状态）
    /// </summary>
    private void RestoreTimeFlow()
    {
        // 恢复玩家移动速度
        if (playerMovement != null)
        {
            playerMovement.moveSpeed = originalMoveSpeed;
            Debug.Log($"[TimeStopSkill] 恢复移动速度为 {originalMoveSpeed}");
        }

        // 恢复所有敌人
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            if (enemy != null)
            {
                // 恢复敌人组件状态
                NavMeshAgent navAgent = enemy.GetComponent<NavMeshAgent>();
                Rigidbody rb = enemy.GetComponent<Rigidbody>();

                if (navAgent != null)
                {
                    navAgent.isStopped = false;
                }

                if (rb != null)
                {
                    rb.isKinematic = false;
                }

                // 启用敌人脚本
                enemy.enabled = true;
            }
        }

        // 销毁特效
        if (timeStopEffect != null)
        {
            Destroy(timeStopEffect);
        }

        isTimeStopped = false;
        Debug.Log("[TimeStopSkill] 时间停顿结束");
    }

    /// <summary>
    /// 当脚本被禁用或对象被销毁时确保恢复时间流动
    /// </summary>
    private void OnDisable()
    {
        if (isTimeStopped)
        {
            RestoreTimeFlow();
        }
    }
}