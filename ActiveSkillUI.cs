using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // ��Ӵ������ռ����ý��EventTrigger��ش���

/// <summary>
/// ��������UI������ - ������ͼ�ꡢ��ȴ��ʾ��UIԪ��
/// </summary>
public class ActiveSkillUI : MonoBehaviour
{
    // ����
    public static ActiveSkillUI Instance { get; private set; }

    [Header("UI����")]
    [Tooltip("���ܲ�UIԤ����")]
    public GameObject skillSlotPrefab;

    [Tooltip("���ܲ۵ĸ�����")]
    public Transform skillSlotsContainer;

    [Tooltip("������ʾ���")]
    public GameObject skillTooltipPanel;

    [Tooltip("������ʾ����е������ı�")]
    public TextMeshProUGUI tooltipNameText;

    [Tooltip("������ʾ����е������ı�")]
    public TextMeshProUGUI tooltipDescriptionText;

    [Tooltip("������ʾ����е���ȴ�ı�")]
    public TextMeshProUGUI tooltipCooldownText;

    [Tooltip("������ʾ����еĵȼ��ı�")]
    public TextMeshProUGUI tooltipLevelText;

    // ���ܲ��б�
    private List<SkillSlot> skillSlots = new List<SkillSlot>();

    // ��ǰ��ʾ��ʾ�ļ��ܲ�
    private SkillSlot currentTooltipSlot;

    /// <summary>
    /// ���ܲ��� - ���浥�����ܲ۵�UIԪ������
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
        // ����ģʽ
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        // ��ʼ���ؼ�����ʾ���
        if (skillTooltipPanel != null)
        {
            skillTooltipPanel.SetActive(false);
        }
    }

    /// <summary>
    /// ��ʼ������UI
    /// </summary>
    /// <param name="maxSlots">����ܲ�����</param>
    public void InitializeSkillUI(int maxSlots)
    {
        // ������м��ܲ�
        foreach (SkillSlot slot in skillSlots)
        {
            if (slot.slotObject != null)
            {
                Destroy(slot.slotObject);
            }
        }
        skillSlots.Clear();

        // ���û����������Canvas�´���һ��
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

                // ���ˮƽ�������
                HorizontalLayoutGroup layout = containerObj.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 10;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;

                skillSlotsContainer = rt;
            }
            else
            {
                Debug.LogError("[ActiveSkillUI] ������û��Canvas��");
                return;
            }
        }

        // �������ܲ�
        for (int i = 0; i < maxSlots; i++)
        {
            CreateSkillSlot(GetKeyCodeForIndex(i));
        }
    }

    /// <summary>
    /// �����������ܲ�
    /// </summary>
    /// <param name="keyCode">�󶨵İ���</param>
    private void CreateSkillSlot(KeyCode keyCode)
    {
        if (skillSlotPrefab == null || skillSlotsContainer == null) return;

        // ʵ�������ܲ�
        GameObject slotObj = Instantiate(skillSlotPrefab, skillSlotsContainer);

        // ��ȡ��Ҫ���
        Image iconImage = slotObj.transform.Find("Icon")?.GetComponent<Image>();
        Image cooldownOverlay = slotObj.transform.Find("CooldownOverlay")?.GetComponent<Image>();
        TextMeshProUGUI keyText = slotObj.transform.Find("KeyText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI levelText = slotObj.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();

        if (iconImage == null || cooldownOverlay == null)
        {
            Debug.LogError("[ActiveSkillUI] ���ܲ�Ԥ����ȱ�ٱ�Ҫ���!");
            Destroy(slotObj);
            return;
        }

        // ���ó�ʼ״̬
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

        // �������ܲ۶�����ӵ��б�
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

        // ����¼�����
        Button button = slotObj.GetComponent<Button>();
        if (button != null)
        {
            int slotIndex = skillSlots.Count - 1;

            // �����ͣ��ʾ��ʾ
            EventTrigger trigger = slotObj.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = slotObj.AddComponent<EventTrigger>();
            }

            // ����������¼�
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { ShowTooltip(slotIndex); });
            trigger.triggers.Add(enterEntry);

            // �������˳��¼�
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { HideTooltip(); });
            trigger.triggers.Add(exitEntry);
        }
    }

    /// <summary>
    /// ��ʾ������ʾ
    /// </summary>
    /// <param name="slotIndex">���ܲ�����</param>
    private void ShowTooltip(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Count) return;

        SkillSlot slot = skillSlots[slotIndex];
        if (slot.linkedSkill == null) return;

        currentTooltipSlot = slot;

        if (skillTooltipPanel != null)
        {
            // ������ʾ����
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
                tooltipCooldownText.text = $"��ȴʱ��: {slot.linkedSkill.GetActualCooldown():F1} ��";
            }

            if (tooltipLevelText != null)
            {
                tooltipLevelText.text = $"�ȼ�: {slot.linkedSkill.level}";
            }

            // ����λ�ò���ʾ
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
    /// ���ؼ�����ʾ
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
    /// ��Ӽ��ܵ�UI
    /// </summary>
    /// <param name="skill">���ܶ���</param>
    public void AddSkillToUI(ActiveSkill skill)
    {
        if (skill == null) return;

        // ���ҿ��в�λ
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
            Debug.LogWarning("[ActiveSkillUI] û�п��õļ��ܲ�!");
            return;
        }

        // ���Ӽ��ܵ���λ
        targetSlot.linkedSkill = skill;
        skill.skillKey = targetSlot.keyCode;

        // ����UI
        targetSlot.iconImage.sprite = skill.icon;
        targetSlot.iconImage.enabled = true;

        if (targetSlot.levelText != null)
        {
            targetSlot.levelText.text = skill.level.ToString();
            targetSlot.levelText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// ��UI���Ƴ�����
    /// </summary>
    /// <param name="skill">���ܶ���</param>
    public void RemoveSkillFromUI(ActiveSkill skill)
    {
        if (skill == null) return;

        // ���Ұ����ü��ܵĲ�λ
        foreach (SkillSlot slot in skillSlots)
        {
            if (slot.linkedSkill == skill)
            {
                // ����ò�λ
                slot.linkedSkill = null;
                slot.iconImage.sprite = null;
                slot.iconImage.enabled = false;
                slot.cooldownOverlay.fillAmount = 0;

                if (slot.levelText != null)
                {
                    slot.levelText.text = "";
                    slot.levelText.gameObject.SetActive(false);
                }

                // �����ǰ������ʾ�ü��ܵ���ʾ��������
                if (currentTooltipSlot == slot)
                {
                    HideTooltip();
                }

                break;
            }
        }
    }

    /// <summary>
    /// ���¼���UI
    /// </summary>
    public void UpdateSkillUI()
    {
        // ������в�λ
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

        // ����������м���
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
        // ������ȴ��ʾ
        foreach (SkillSlot slot in skillSlots)
        {
            if (slot.linkedSkill != null)
            {
                float cooldownProgress = slot.linkedSkill.GetCooldownProgress();
                slot.cooldownOverlay.fillAmount = cooldownProgress;

                // ������ʾ����е���ȴʱ�䣨���������ʾ��
                if (currentTooltipSlot == slot && tooltipCooldownText != null)
                {
                    if (cooldownProgress > 0)
                    {
                        tooltipCooldownText.text = $"��ȴʱ��: {slot.linkedSkill.currentCooldown:F1} ��";
                    }
                    else
                    {
                        tooltipCooldownText.text = $"��ȴʱ��: {slot.linkedSkill.GetActualCooldown():F1} ��";
                    }
                }
            }
        }
    }

    /// <summary>
    /// ����������ȡ������
    /// </summary>
    /// <param name="index">��λ����</param>
    /// <returns>��Ӧ�İ�����</returns>
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