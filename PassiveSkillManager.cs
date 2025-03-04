using UnityEngine;
using System.Collections.Generic;

public class PassiveSkillManager : MonoBehaviour
{
    [Header("技能解锁条件")]
    [Tooltip("技能A的解锁击杀数")]
    public int skillAUnlockKills = 1000;

    [Header("持久化数据")]
    [Tooltip("总击杀数（跨游戏存储）")]
    private int totalKills = 0;
    private string killCountKey = "TotalEnemyKills";

    [Header("调试信息")]
    [SerializeField] private bool isSkillAUnlocked = false;
    [SerializeField] private int currentSessionKills = 0;

    // 单例模式
    public static PassiveSkillManager Instance { get; private set; }

    void Awake()
    {
        // 单例实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadKillCount(); // 加载之前的击杀数
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 检查技能解锁状态
        CheckSkillUnlocks();
    }

    // 从PlayerPrefs加载击杀计数
    private void LoadKillCount()
    {
        totalKills = PlayerPrefs.GetInt(killCountKey, 0);
        Debug.Log($"[PassiveSkillManager] 加载总击杀数: {totalKills}");
    }

    // 保存击杀计数到PlayerPrefs
    private void SaveKillCount()
    {
        PlayerPrefs.SetInt(killCountKey, totalKills);
        PlayerPrefs.Save();
        Debug.Log($"[PassiveSkillManager] 保存总击杀数: {totalKills}");
    }

    public void ResetTotalKills()
    {
        totalKills = 0;
        SaveKillCount();
        CheckSkillUnlocks();
        Debug.Log("[PassiveSkillManager] 总击杀数已重置为0");
    }

    // 增加击杀计数
    public void AddKill()
    {
        totalKills++;
        currentSessionKills++;

        // 检查是否解锁了新技能
        CheckSkillUnlocks();

        // 每10次击杀保存一次数据，避免频繁写入
        if (totalKills % 10 == 0)
        {
            SaveKillCount();
        }
    }

    // 检查技能解锁
    private void CheckSkillUnlocks()
    {
        // 检查技能A的解锁状态
        if (totalKills >= skillAUnlockKills && !isSkillAUnlocked)
        {
            isSkillAUnlocked = true;
            Debug.Log("[PassiveSkillManager] 技能A已解锁：蛇尾自愈能力");
        }
    }

    // 获取技能A的解锁状态
    public bool IsSkillAUnlocked()
    {
        return isSkillAUnlocked;
    }

    // 在游戏结束时保存数据
    private void OnApplicationQuit()
    {
        SaveKillCount();
    }

    // 用于UI显示
    public int GetTotalKills()
    {
        return totalKills;
    }

    public int GetCurrentSessionKills()
    {
        return currentSessionKills;
    }

    // 重置当前会话击杀数（在游戏重新开始时调用）
    public void ResetSessionKills()
    {
        currentSessionKills = 0;
    }
}