using UnityEngine;
using TMPro;

/// <summary>
/// 技能拾取物 - 用于玩家拾取获得技能
/// </summary>
public class SkillPickup : MonoBehaviour
{
    [Header("技能信息")]
    [Tooltip("技能预制体引用")]
    public GameObject skillPrefab;

    [Tooltip("技能名称")]
    public string skillName = "未知技能";

    [Tooltip("技能图标")]
    public Sprite skillIcon;

    [Tooltip("技能描述")]
    [TextArea(3, 5)]
    public string skillDescription = "技能描述";

    [Header("外观设置")]
    [Tooltip("旋转速度")]
    public float rotationSpeed = 50f;

    [Tooltip("悬浮高度")]
    public float hoverHeight = 0.5f;

    [Tooltip("悬浮速度")]
    public float hoverSpeed = 1f;

    [Tooltip("拾取检测范围")]
    public float pickupRadius = 2f;

    [Tooltip("提示文本预制体")]
    public GameObject tooltipPrefab;

    // 私有变量
    private float initialY;
    private TextMeshProUGUI tooltipText;
    private GameObject tooltipInstance;
    private Transform player;
    private bool isPlayerNearby = false;

    void Start()
    {
        initialY = transform.position.y;

        // 查找玩家
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 创建提示文本
        if (tooltipPrefab != null)
        {
            tooltipInstance = Instantiate(tooltipPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            tooltipInstance.transform.SetParent(transform);

            tooltipText = tooltipInstance.GetComponent<TextMeshProUGUI>();
            if (tooltipText != null)
            {
                tooltipText.text = $"按 F 获取技能: {skillName}";
                tooltipInstance.SetActive(false);
            }
        }

        // 添加发光效果
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", Color.cyan * 2f);
        }
    }

    void Update()
    {
        // 旋转效果
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 悬浮效果
        float newY = initialY + Mathf.Sin(Time.time * hoverSpeed) * 0.2f + hoverHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 检测玩家是否在范围内
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= pickupRadius)
            {
                if (!isPlayerNearby)
                {
                    isPlayerNearby = true;
                    if (tooltipInstance != null)
                    {
                        tooltipInstance.SetActive(true);
                    }
                }

                // 检测按键拾取
                if (Input.GetKeyDown(KeyCode.F))
                {
                    PickupSkill();
                }
            }
            else if (isPlayerNearby)
            {
                isPlayerNearby = false;
                if (tooltipInstance != null)
                {
                    tooltipInstance.SetActive(false);
                }
            }

            // 使提示文本始终面向玩家
            if (tooltipInstance != null && tooltipInstance.activeSelf)
            {
                tooltipInstance.transform.LookAt(Camera.main.transform);
                tooltipInstance.transform.Rotate(0, 180, 0);
            }
        }
    }

    /// <summary>
    /// 拾取技能
    /// </summary>
    private void PickupSkill()
    {
        if (skillPrefab != null && ActiveSkillManager.Instance != null)
        {
            ActiveSkillManager.Instance.AcquireSkill(skillPrefab);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 碰撞检测（备用方法）
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (tooltipInstance != null)
            {
                tooltipInstance.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 碰撞结束检测
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (tooltipInstance != null)
            {
                tooltipInstance.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 在场景视图中显示拾取范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}