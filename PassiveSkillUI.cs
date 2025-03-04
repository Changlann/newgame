using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PassiveSkillUI : MonoBehaviour
{
    [Header("UI引用")]
    public TextMeshProUGUI totalKillsText;
    public TextMeshProUGUI sessionKillsText;
    public GameObject skillAPanel;
    public Image skillALockIcon;
    public TextMeshProUGUI skillAStatus;

    [Header("更新设置")]
    public float updateInterval = 0.5f; // 每0.5秒更新一次，避免每帧更新

    private float updateTimer = 0f;
    private PassiveSkillManager skillManager;

    void Start()
    {
        // 寻找技能管理器
        skillManager = PassiveSkillManager.Instance;

        if (skillManager == null)
        {
            Debug.LogWarning("[PassiveSkillUI] 找不到PassiveSkillManager实例！");
            enabled = false;
            return;
        }

        // 初始更新UI
        UpdateUI();
    }

    void Update()
    {
        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            UpdateUI();
            updateTimer = 0f;
        }
    }

    void UpdateUI()
    {
        if (skillManager == null) return;

        // 更新击杀计数
        if (totalKillsText != null)
        {
            totalKillsText.text = $"总击杀: {skillManager.GetTotalKills()}";
        }

        if (sessionKillsText != null)
        {
            sessionKillsText.text = $"本局击杀: {skillManager.GetCurrentSessionKills()}";
        }

        // 更新技能A状态
        bool skillAUnlocked = skillManager.IsSkillAUnlocked();

        if (skillAPanel != null)
        {
            if (skillALockIcon != null)
            {
                skillALockIcon.gameObject.SetActive(!skillAUnlocked);
            }

            if (skillAStatus != null)
            {
                if (skillAUnlocked)
                {
                    skillAStatus.text = "已解锁: 蛇尾自愈能力";
                    skillAStatus.color = Color.green;
                }
                else
                {
                    int requiredKills = skillManager.skillAUnlockKills;
                    int currentKills = skillManager.GetTotalKills();
                    skillAStatus.text = $"解锁进度: {currentKills}/{requiredKills}";
                    skillAStatus.color = Color.yellow;
                }
            }
        }
    }
}