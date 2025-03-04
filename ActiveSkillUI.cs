using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // 添加此命名空间引用解决EventTrigger相关错误

/// <summary>
/// 主动技能UI管理器 - 管理技能图标、冷却显示等UI元素
/// </summary>
public class ActiveSkillUI : MonoBehaviour
{
    // 单例
    public static ActiveSkillUI Instance { get; private set; }

    [Header("UI引用")]
    [Tooltip("技能槽UI预制体")]
    public GameObject skillSlotPrefab;

    [Tooltip("技能槽的父容器")]
    public Transform skillSlotsContainer;

    [Tooltip("技能提示面板")]
    public GameObject skillTooltipPanel;

    [Tooltip("技能提示面板中的名称文本")]
    public TextMeshProUGUI tooltipNameText;

    [Tooltip("技能提示面板中的描述文本")]
    public TextMeshProUGUI tooltipDescriptionText;

    [Tooltip("技能提示面板中的冷却文本")]
    public TextMeshProUGUI tooltipCooldownText;

    [Tooltip("技能提示面板中的等级文本")]
    public TextMeshProUGUI tooltipLevelText;

    // 技能槽列表
    private List<SkillSlot> skillSlots = new List<SkillSlot>();

    // 当前显示提示的技能槽
    private SkillSlot currentTooltipSlot;

    /// <summary>
    /// 技能槽类 - 保存单个技能槽的UI元素引用
    /// </summary>
    [System.Serializable]
    public class SkillSlot
    {
        public GameObject slotObject;
        public Image iconImage;
        public Image cooldownOverlay;
        public TextMeshProUGUI keyText;
        public TextMeshProUGUI levelText;
        public ActiveSkill linkedSkill;
        public KeyCode keyCode;
    }

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

        // 初始隐藏技能提示面板
        if (skillTooltipPanel != null)
        {
            skillTooltipPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 初始化技能UI
    /// </summary>
    /// <param name="maxSlots">最大技能槽数量</param>
    public void InitializeSkillUI(int maxSlots)
    {
        // 清除现有技能槽
        foreach (SkillSlot slot in skillSlots)
        {
            if (slot.slotObject != null)
            {
                Destroy(slot.slotObject);
            }
        }
        skillSlots.Clear();

        // 如果没有容器，在Canvas下创建一个
        if (skillSlotsContainer == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                GameObject containerObj = new GameObject("SkillSlotsContainer");
                containerObj.transform.SetParent(canvas.transform);

                RectTransform rt = containerObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0);
                rt.anchorMax = new Vector2(0.5f, 0);
                rt.pivot = new Vector2(0.5f, 0);
                rt.anchoredPosition = new Vector2(0, 100);
                rt.sizeDelta = new Vector2(400, 100);

                // 添加水平布局组件
                HorizontalLayoutGroup layout = containerObj.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 10;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;

                skillSlotsContainer = rt;
            }
            else
            {
                Debug.LogError("[ActiveSkillUI] 场景中没有Canvas！");
                return;
            }
        }

        // 创建技能槽
        for (int i = 0; i < maxSlots; i++)
        {
            CreateSkillSlot(GetKeyCodeForIndex(i));
        }
    }

    /// <summary>
    /// 创建单个技能槽
    /// </summary>
    /// <param name="keyCode">绑定的按键</param>
    private void CreateSkillSlot(KeyCode keyCode)
    {
        if (skillSlotPrefab == null || skillSlotsContainer == null) return;

        // 实例化技能槽
        GameObject slotObj = Instantiate(skillSlotPrefab, skillSlotsContainer);

        // 获取必要组件
        Image iconImage = slotObj.transform.Find("Icon")?.GetComponent<Image>();
        Image cooldownOverlay = slotObj.transform.Find("CooldownOverlay")?.GetComponent<Image>();
        TextMeshProUGUI keyText = slotObj.transform.Find("KeyText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI levelText = slotObj.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();

        if (iconImage == null || cooldownOverlay == null)
        {
            Debug.LogError("[ActiveSkillUI] 技能槽预制体缺少必要组件!");
            Destroy(slotObj);
            return;
        }

        // 设置初始状态
        iconImage.sprite = null;
        iconImage.enabled = false;
        cooldownOverlay.fillAmount = 0;

        if (keyText != null)
        {
            keyText.text = keyCode.ToString();
        }

        if (levelText != null)
        {
            levelText.text = "";
            levelText.gameObject.SetActive(false);
        }

        // 创建技能槽对象并添加到列表
        SkillSlot newSlot = new SkillSlot
        {
            slotObject = slotObj,
            iconImage = iconImage,
            cooldownOverlay = cooldownOverlay,
            keyText = keyText,
            levelText = levelText,
            linkedSkill = null,
            keyCode = keyCode
        };
        skillSlots.Add(newSlot);

        // 添加事件监听
        Button button = slotObj.GetComponent<Button>();
        if (button != null)
        {
            int slotIndex = skillSlots.Count - 1;

            // 鼠标悬停显示提示
            EventTrigger trigger = slotObj.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = slotObj.AddComponent<EventTrigger>();
            }

            // 添加鼠标进入事件
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { ShowTooltip(slotIndex); });
            trigger.triggers.Add(enterEntry);

            // 添加鼠标退出事件
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { HideTooltip(); });
            trigger.triggers.Add(exitEntry);
        }
    }

    /// <summary>
    /// 显示技能提示
    /// </summary>
    /// <param name="slotIndex">技能槽索引</param>
    private void ShowTooltip(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Count) return;

        SkillSlot slot = skillSlots[slotIndex];
        if (slot.linkedSkill == null) return;

        currentTooltipSlot = slot;

        if (skillTooltipPanel != null)
        {
            // 更新提示内容
            if (tooltipNameText != null)
            {
                tooltipNameText.text = slot.linkedSkill.skillName;
            }

            if (tooltipDescriptionText != null)
            {
                tooltipDescriptionText.text = slot.linkedSkill.description;
            }

            if (tooltipCooldownText != null)
            {
                tooltipCooldownText.text = $"冷却时间: {slot.linkedSkill.GetActualCooldown():F1} 秒";
            }

            if (tooltipLevelText != null)
            {
                tooltipLevelText.text = $"等级: {slot.linkedSkill.level}";
            }

            // 设置位置并显示
            RectTransform tooltipRT = skillTooltipPanel.GetComponent<RectTransform>();
            RectTransform slotRT = slot.slotObject.GetComponent<RectTransform>();

            if (tooltipRT != null && slotRT != null)
            {
                Vector3 slotPosition = slotRT.position;
                tooltipRT.position = new Vector3(slotPosition.x, slotPosition.y + 100, slotPosition.z);
            }

            skillTooltipPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏技能提示
    /// </summary>
    private void HideTooltip()
    {
        currentTooltipSlot = null;

        if (skillTooltipPanel != null)
        {
            skillTooltipPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 添加技能到UI
    /// </summary>
    /// <param name="skill">技能对象</param>
    public void AddSkillToUI(ActiveSkill skill)
    {
        if (skill == null) return;

        // 查找空闲槽位
        SkillSlot targetSlot = null;
        foreach (SkillSlot slot in skillSlots)
        {
            if (slot.linkedSkill == null)
            {
                targetSlot = slot;
                break;
            }
        }

        if (targetSlot == null)
        {
            Debug.LogWarning("[ActiveSkillUI] 没有可用的技能槽!");
            return;
        }

        // 链接技能到槽位
        targetSlot.linkedSkill = skill;
        skill.skillKey = targetSlot.keyCode;

        // 更新UI
        targetSlot.iconImage.sprite = skill.icon;
        targetSlot.iconImage.enabled = true;

        if (targetSlot.levelText != null)
        {
            targetSlot.levelText.text = skill.level.ToString();
            targetSlot.levelText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 从UI中移除技能
    /// </summary>
    /// <param name="skill">技能对象</param>
    public void RemoveSkillFromUI(ActiveSkill skill)
    {
        if (skill == null) return;

        // 查找包含该技能的槽位
        foreach (SkillSlot slot in skillSlots)
        {
            if (slot.linkedSkill == skill)
            {
                // 清除该槽位
                slot.linkedSkill = null;
                slot.iconImage.sprite = null;
                slot.iconImage.enabled = false;
                slot.cooldownOverlay.fillAmount = 0;

                if (slot.levelText != null)
                {
                    slot.levelText.text = "";
                    slot.levelText.gameObject.SetActive(false);
                }

                // 如果当前正在显示该技能的提示，则隐藏
                if (currentTooltipSlot == slot)
                {
                    HideTooltip();
                }

                break;
            }
        }
    }

    /// <summary>
    /// 更新技能UI
    /// </summary>
    public void UpdateSkillUI()
    {
        // 清除所有槽位
        foreach (SkillSlot slot in skillSlots)
        {
            slot.linkedSkill = null;
            slot.iconImage.sprite = null;
            slot.iconImage.enabled = false;
            slot.cooldownOverlay.fillAmount = 0;

            if (slot.levelText != null)
            {
                slot.levelText.text = "";
                slot.levelText.gameObject.SetActive(false);
            }
        }

        // 重新添加所有技能
        if (ActiveSkillManager.Instance != null)
        {
            for (int i = 0; i < ActiveSkillManager.Instance.acquiredSkills.Count; i++)
            {
                ActiveSkill skill = ActiveSkillManager.Instance.acquiredSkills[i];

                if (i < skillSlots.Count)
                {
                    SkillSlot slot = skillSlots[i];
                    slot.linkedSkill = skill;
                    skill.skillKey = slot.keyCode;

                    slot.iconImage.sprite = skill.icon;
                    slot.iconImage.enabled = true;

                    if (slot.levelText != null)
                    {
                        slot.levelText.text = skill.level.ToString();
                        slot.levelText.gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    void Update()
    {
        // 更新冷却显示
        foreach (SkillSlot slot in skillSlots)
        {
            if (slot.linkedSkill != null)
            {
                float cooldownProgress = slot.linkedSkill.GetCooldownProgress();
                slot.cooldownOverlay.fillAmount = cooldownProgress;

                // 更新提示面板中的冷却时间（如果正在显示）
                if (currentTooltipSlot == slot && tooltipCooldownText != null)
                {
                    if (cooldownProgress > 0)
                    {
                        tooltipCooldownText.text = $"冷却时间: {slot.linkedSkill.currentCooldown:F1} 秒";
                    }
                    else
                    {
                        tooltipCooldownText.text = $"冷却时间: {slot.linkedSkill.GetActualCooldown():F1} 秒";
                    }
                }
            }
        }
    }

    /// <summary>
    /// 根据索引获取按键码
    /// </summary>
    /// <param name="index">槽位索引</param>
    /// <returns>对应的按键码</returns>
    private KeyCode GetKeyCodeForIndex(int index)
    {
        switch (index)
        {
            case 0: return KeyCode.Q;
            case 1: return KeyCode.E;
            case 2: return KeyCode.R;
            case 3: return KeyCode.T;
            default: return KeyCode.Alpha1 + index;
        }
    }
}