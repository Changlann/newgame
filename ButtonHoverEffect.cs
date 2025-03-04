using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("描边设置")]
    public float outlineWidth = 0.1f;
    public Color outlineColor = Color.white;

    [Header("动画设置")]
    public float animationSpeed = 5f;
    public float hoverScale = 1.1f;

    // 组件引用
    private TextMeshProUGUI textComponent;
    private Vector3 originalScale;
    private bool isHovering = false;

    void Start()
    {
        // 获取TextMeshPro组件
        textComponent = GetComponentInChildren<TextMeshProUGUI>();

        // 保存原始大小
        originalScale = transform.localScale;

        // 初始化文本(确保没有描边)
        if (textComponent != null)
        {
            textComponent.outlineWidth = 0;
            textComponent.outlineColor = outlineColor;
        }
    }

    void Update()
    {
        // 平滑缩放动画
        if (isHovering)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale * hoverScale, Time.deltaTime * animationSpeed);

            if (textComponent != null)
            {
                textComponent.outlineWidth = Mathf.Lerp(textComponent.outlineWidth, outlineWidth, Time.deltaTime * animationSpeed);
            }
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * animationSpeed);

            if (textComponent != null)
            {
                textComponent.outlineWidth = Mathf.Lerp(textComponent.outlineWidth, 0, Time.deltaTime * animationSpeed);
            }
        }
    }

    // 鼠标进入
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    // 鼠标离开
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }
}