using UnityEngine;

[ExecuteInEditMode]
public class MoireFisheyeEffect : MonoBehaviour
{
    [Header("����Ч������")]
    [Range(0f, 1f)]
    public float fisheyeStrength = 0.5f;
    [Range(0f, 1f)]
    public float barrelDistortion = 0.5f;
    [Range(0f, 1f)]
    public float zoom = 1f;

    [Header("Ħ���Ʋ���")]
    [Range(1f, 100f)]
    public float moireScale = 50f;        // Ħ���Ƶ��ܶ�
    [Range(0f, 1f)]
    public float moireStrength = 0.1f;    // Ħ���Ƶ�ǿ��
    [Range(0f, 2f)]
    public float moireSpeed = 0.5f;       // Ħ���ƵĶ����ٶ�

    private Material effectMaterial;
    private Shader effectShader;

    void Start()
    {
        CreateEffectShader();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (effectMaterial == null)
        {
            CreateEffectShader();
        }

        // ��������shader����
        effectMaterial.SetFloat("_FisheyeStrength", fisheyeStrength);
        effectMaterial.SetFloat("_BarrelDistortion", barrelDistortion);
        effectMaterial.SetFloat("_Zoom", zoom);
        effectMaterial.SetFloat("_MoireScale", moireScale);
        effectMaterial.SetFloat("_MoireStrength", moireStrength);
        effectMaterial.SetFloat("_MoireSpeed", moireSpeed);

        Graphics.Blit(source, destination, effectMaterial);
    }

    void CreateEffectShader()
    {
        if (effectShader == null)
        {
            effectShader = Shader.Find("Hidden/MoireFisheyeShader");
        }
        if (effectShader != null && effectMaterial == null)
        {
            effectMaterial = new Material(effectShader);
            effectMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    void OnDisable()
    {
        if (effectMaterial != null)
        {
            DestroyImmediate(effectMaterial);
        }
    }
}