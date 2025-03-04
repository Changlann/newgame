using UnityEngine;
using System.Collections.Generic;

public class HealingCircle : MonoBehaviour
{
    [Header("治疗设置")]
    [Tooltip("治疗范围半径")]
    public float healRadius = 3f;
    [Tooltip("每秒治疗量")]
    public float healAmountPerSecond = 5f;
    [Tooltip("持续时间（秒）")]
    public float duration = 10f;
    [Tooltip("跟随的目标")]
    public Transform target;

    [Header("视觉效果")]
    [Tooltip("治疗光环颜色")]
    public Color healColor = new Color(0.2f, 1f, 0.2f, 0.4f);
    [Tooltip("光环脉动速度")]
    public float pulseSpeed = 1.5f;
    [Tooltip("脉动大小变化范围")]
    public float pulseRange = 0.2f;

    // 私有变量
    private float timer = 0f;
    private MeshRenderer circleRenderer;
    private List<Health> healedTargets = new List<Health>();

    void Start()
    {
        // 创建视觉效果
        CreateVisualEffect();

        // 如果没有设置目标，尝试使用父对象
        if (target == null && transform.parent != null)
        {
            target = transform.parent;
        }
    }

    void Update()
    {
        // 跟随目标
        if (target != null)
        {
            transform.position = target.position;
        }

        // 计时
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Destroy(gameObject);
            return;
        }

        // 视觉效果 - 脉动
        float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseRange;
        transform.localScale = new Vector3(scale, scale, scale) * healRadius;

        // 治疗效果
        HealNearbyAllies();
    }

    void CreateVisualEffect()
    {
        // 创建一个圆形平面作为治疗光环
        GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        circle.transform.SetParent(transform);
        circle.transform.localPosition = new Vector3(0, 0.1f, 0); // 略微升高，避免和地面重叠
        circle.transform.localScale = new Vector3(1, 0.02f, 1); // 扁平化

        // 移除碰撞体
        Destroy(circle.GetComponent<Collider>());

        // 设置材质
        circleRenderer = circle.GetComponent<MeshRenderer>();
        Material circleMaterial = new Material(Shader.Find("Standard"));
        circleMaterial.color = healColor;
        circleMaterial.EnableKeyword("_EMISSION");
        circleMaterial.SetColor("_EmissionColor", healColor * 0.5f);

        // 设置渲染模式为透明
        circleMaterial.SetFloat("_Mode", 3); // Transparent mode
        circleMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        circleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        circleMaterial.SetInt("_ZWrite", 0);
        circleMaterial.DisableKeyword("_ALPHATEST_ON");
        circleMaterial.EnableKeyword("_ALPHABLEND_ON");
        circleMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        circleMaterial.renderQueue = 3000;

        circleRenderer.material = circleMaterial;
    }

    void HealNearbyAllies()
    {
        // 清空上一次的治疗目标列表
        healedTargets.Clear();

        // 查找范围内的所有碰撞体
        Collider[] colliders = Physics.OverlapSphere(transform.position, healRadius);

        foreach (Collider collider in colliders)
        {
            // 只治疗蛇身
            if (collider.CompareTag("SnakeBody"))
            {
                Health health = collider.GetComponent<Health>();
                if (health != null && !healedTargets.Contains(health))
                {
                    // 每秒治疗指定量的生命值
                    health.Heal(healAmountPerSecond * Time.deltaTime);
                    healedTargets.Add(health);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        // 在Scene视图中显示治疗范围
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRadius);
    }
}