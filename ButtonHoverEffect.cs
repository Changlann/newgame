using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("�������")]
    public float outlineWidth = 0.1f;
    public Color outlineColor = Color.white;

    [Header("��������")]
    public float animationSpeed = 5f;
    public float hoverScale = 1.1f;

    // �������
    private TextMeshProUGUI textComponent;
    private Vector3 originalScale;
    private bool isHovering = false;

    void Start()
    {
        // ��ȡTextMeshPro���
        textComponent = GetComponentInChildren<TextMeshProUGUI>();

        // ����ԭʼ��С
        originalScale = transform.localScale;

        // ��ʼ���ı�(ȷ��û�����)
        if (textComponent != null)
        {
            textComponent.outlineWidth = 0;
            textComponent.outlineColor = outlineColor;
        }
    }

    void Update()
    {
        // ƽ�����Ŷ���
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

    // ������
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    // ����뿪
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }
}