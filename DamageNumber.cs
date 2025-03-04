using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    public float lifetime = 1f;    // 持续时间（秒）
    public float moveSpeed = 1f;  // 上升速度
    private TextMeshProUGUI textMesh;
    private float timer;
    private Color textColor;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textColor = textMesh.color;  // 获取初始颜色
            timer = lifetime;            // 设置计时器
        }
        else
        {
            Debug.LogError("伤害数字预制体缺少TextMeshProUGUI组件！");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // 向上移动
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 处理淡出
        timer -= Time.deltaTime;
        float alpha = timer / lifetime;  // 计算透明度
        textColor.a = alpha;             // 设置透明度

        if (textMesh != null)
        {
            textMesh.color = textColor;  // 应用颜色
        }

        // 时间到了销毁
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetDamageText(int damage)
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshProUGUI>();
        }

        if (textMesh != null)
        {
            textMesh.text = damage.ToString();  // 设置显示的数字
        }
        else
        {
            Debug.LogError("无法设置伤害数字，缺少TextMeshProUGUI组件！");
        }
    }
}