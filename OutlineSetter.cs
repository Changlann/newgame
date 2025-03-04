using TMPro;
using UnityEngine;

public class OutlineSetter : MonoBehaviour
{
    public TextMeshProUGUI tmpText;  // 也可换成 TextMeshPro

    void Start()
    {
        // 1. 获取文本所使用的材质
        Material mat = tmpText.fontMaterial;

        // 2. 设置描边宽度（数值根据需要调整）
        mat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.2f);

        // 3. 设置描边颜色
        mat.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
    }
}
