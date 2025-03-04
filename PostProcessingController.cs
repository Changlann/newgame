using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingController : MonoBehaviour
{
    [Header("��������")]
    public PostProcessVolume postProcessVolume;

    [Header("RGB��������")]
    public float chromaticAberrationIntensity = 0.5f;

    [Header("��������")]
    public float vignetteIntensity = 0.4f;
    public Color vignetteColor = Color.black;

    [Header("�������")]
    public float grainIntensity = 0.5f;
    public float grainSize = 1.0f;

    private ChromaticAberration chromaticAberration;
    private Vignette vignette;
    private Grain grain;

    void Start()
    {
        // ��ȡ����Ч�����
        postProcessVolume.profile.TryGetSettings(out chromaticAberration);
        postProcessVolume.profile.TryGetSettings(out vignette);
        postProcessVolume.profile.TryGetSettings(out grain);

        // ��ʼ��RGB����
        if (chromaticAberration != null)
        {
            chromaticAberration.enabled.value = true;
            chromaticAberration.intensity.value = chromaticAberrationIntensity;
        }

        // ��ʼ������
        if (vignette != null)
        {
            vignette.enabled.value = true;
            vignette.intensity.value = vignetteIntensity;
            vignette.color.value = vignetteColor;
        }

        // ��ʼ�����
        if (grain != null)
        {
            grain.enabled.value = true;
            grain.intensity.value = grainIntensity;
            grain.size.value = grainSize;
        }
    }

    // ��̬����RGB����ǿ��
    public void SetChromaticAberrationIntensity(float intensity)
    {
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = intensity;
        }
    }

    // ��̬��������ǿ��
    public void SetVignetteIntensity(float intensity)
    {
        if (vignette != null)
        {
            vignette.intensity.value = intensity;
        }
    }

    // ��̬�������ǿ��
    public void SetGrainIntensity(float intensity)
    {
        if (grain != null)
        {
            grain.intensity.value = intensity;
        }
    }
}