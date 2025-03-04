using TMPro;
using UnityEngine;

public class OutlineSetter : MonoBehaviour
{
    public TextMeshProUGUI tmpText;  // Ҳ�ɻ��� TextMeshPro

    void Start()
    {
        // 1. ��ȡ�ı���ʹ�õĲ���
        Material mat = tmpText.fontMaterial;

        // 2. ������߿�ȣ���ֵ������Ҫ������
        mat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.2f);

        // 3. ���������ɫ
        mat.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
    }
}
