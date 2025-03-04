using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 主动技能管理器 - 管理技能的获取、升级和使用
/// </summary>
public class ActiveSkillManager : MonoBehaviour
{
    // 单例
    public static ActiveSkillManager Instance { get; private set; }

    [Header("技能设置")]
    [Tooltip("可用技能预制体列表")]
    public List<GameObject> skillPrefabs = new List<GameObject>();

    [Tooltip("技能获取物品预制体")]
    public GameObject skillPickupPrefab;

    [Tooltip("拾取物品后的音效")]
    public AudioClip pickupSound;

    [Tooltip("最多可以同时拥有的技能数量")]
    public int maxSkillSlots = 3;

    // 已获得的技能
    [HideInInspector]
    public List<ActiveSkill> acquiredSkills = new List<ActiveSkill>();

    // 音频源
    private AudioSource audioSource;

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        // 获取或添加AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        // 初始化技能UI
        ActiveSkillUI.Instance?.InitializeSkillUI(maxSkillSlots);
    }

    /// <summary>
    /// 创建技能拾取物
    /// </summary>
    /// <param name="position">生成位置</param>
    public void SpawnSkillPickup(Vector3 position)
    {
        if (skillPickupPrefab == null || skillPrefabs.Count == 0) return;

        // 随机选择一个技能
        int randomSkillIndex = Random.Range(0, skillPrefabs.Count);
        GameObject randomSkillPrefab = skillPrefabs[randomSkillIndex];

        // 创建拾取物
        GameObject pickup = Instantiate(skillPickupPrefab, position, Quaternion.identity);

        // 设置拾取物属性
        SkillPickup pickupComponent = pickup.GetComponent<SkillPickup>();
        if (pickupComponent != null)
        {
            // 获取技能信息
            ActiveSkill skillInfo = randomSkillPrefab.GetComponent<ActiveSkill>();
            if (skillInfo != null)
            {
                pickupComponent.skillPrefab = randomSkillPrefab;
                pickupComponent.skillName = skillInfo.skillName;
                pickupComponent.skillIcon = skillInfo.icon;
                pickupComponent.skillDescription = skillInfo.description;
            }
        }

        Debug.Log($"[ActiveSkillManager] 在 {position} 生成技能拾取物");
    }

    /// <summary>
    /// 获取技能
    /// </summary>
    /// <param name="skillPrefab">技能预制体</param>
    public void AcquireSkill(GameObject skillPrefab)
    {
        if (skillPrefab == null) return;

        // 播放拾取音效
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        // 获取技能类型
        ActiveSkill newSkillPrototype = skillPrefab.GetComponent<ActiveSkill>();
        if (newSkillPrototype == null)
        {
            Debug.LogError("[ActiveSkillManager] 技能预制体不包含ActiveSkill组件");
            return;
        }

        // 检查是否已有相同技能
        ActiveSkill existingSkill = null;
        foreach (ActiveSkill skill in acquiredSkills)
        {
            if (skill.GetType() == newSkillPrototype.GetType())
            {
                existingSkill = skill;
                break;
            }
        }

        // 如果已有相同技能，则升级
        if (existingSkill != null)
        {
            existingSkill.LevelUp();
            Debug.Log($"[ActiveSkillManager] 升级技能 {existingSkill.skillName} 到 {existingSkill.level} 级");

            // 更新UI
            ActiveSkillUI.Instance?.UpdateSkillUI();
        }
        // 否则添加新技能
        else
        {
            // 检查技能槽是否已满
            if (acquiredSkills.Count >= maxSkillSlots)
            {
                Debug.Log("[ActiveSkillManager] 技能槽已满，无法获取新技能");
                return;
            }

            // 实例化技能
            GameObject skillObj = Instantiate(skillPrefab, transform);
            ActiveSkill newSkill = skillObj.GetComponent<ActiveSkill>();

            if (newSkill != null)
            {
                // 添加到已获得技能列表
                acquiredSkills.Add(newSkill);
                Debug.Log($"[ActiveSkillManager] 获得新技能: {newSkill.skillName}");

                // 更新UI
                ActiveSkillUI.Instance?.AddSkillToUI(newSkill);
            }
        }
    }

    /// <summary>
    /// 移除技能
    /// </summary>
    /// <param name="skill">要移除的技能</param>
    public void RemoveSkill(ActiveSkill skill)
    {
        if (skill == null) return;

        // 从列表中移除
        acquiredSkills.Remove(skill);

        // 移除技能对象
        Destroy(skill.gameObject);

        // 更新UI
        ActiveSkillUI.Instance?.UpdateSkillUI();

        Debug.Log($"[ActiveSkillManager] 移除技能: {skill.skillName}");
    }

    /// <summary>
    /// 生成技能拾取物（用于测试）
    /// </summary>
    public void SpawnRandomSkillPickupNearby()
    {
        if (skillPickupPrefab == null || skillPrefabs.Count == 0) return;

        // 在玩家附近随机位置生成
        Vector3 randomOffset = new Vector3(
            Random.Range(-5f, 5f),
            0.5f,
            Random.Range(-5f, 5f)
        );

        Vector3 spawnPosition = transform.position + randomOffset;
        SpawnSkillPickup(spawnPosition);
    }

    /// <summary>
    /// 直接获取随机技能（用于测试）
    /// </summary>
    public void AcquireRandomSkill()
    {
        if (skillPrefabs.Count == 0) return;

        int randomIndex = Random.Range(0, skillPrefabs.Count);
        AcquireSkill(skillPrefabs[randomIndex]);
    }
}