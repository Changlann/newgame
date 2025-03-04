using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingController : MonoBehaviour
{
    [Header("后处理配置")]
    public PostProcessVolume postProcessVolume;

    [Header("RGB分离设置")]
    public float chromaticAberrationIntensity = 0.5f;

    [Header("暗角设置")]
    public float vignetteIntensity = 0.4f;
    public Color vignetteColor = Color.black;

    [Header("噪点设置")]
    public float grainIntensity = 0.5f;
    public float grainSize = 1.0f;

    private ChromaticAberration chromaticAberration;
    private Vignette vignette;
    private Grain grain;

    void Start()
    {
        // 获取后处理效果组件
        postProcessVolume.profile.TryGetSettings(out chromaticAberration);
        postProcessVolume.profile.TryGetSettings(out vignette);
        postProcessVolume.profile.TryGetSettings(out grain);

        // 初始化RGB分离
        if (chromaticAberration != null)
        {
            chromaticAberration.enabled.value = true;
            chromaticAberration.intensity.value = chromaticAberrationIntensity;
        }

        // 初始化暗角
        if (vignette != null)
        {
            vignette.enabled.value = true;
            vignette.intensity.value = vignetteIntensity;
            vignette.color.value = vignetteColor;
        }

        // 初始化噪点
        if (grain != null)
        {
            grain.enabled.value = true;
            grain.intensity.value = grainIntensity;
            grain.size.value = grainSize;
        }
    }

    // 动态调整RGB分离强度
    public void SetChromaticAberrationIntensity(float intensity)
    {
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = intensity;
        }
    }

    // 动态调整暗角强度
    public void SetVignetteIntensity(float intensity)
    {
        if (vignette != null)
        {
            vignette.intensity.value = intensity;
        }
    }

    // 动态调整噪点强度
    public void SetGrainIntensity(float intensity)
    {
        if (grain != null)
        {
            grain.intensity.value = intensity;
        }
    }
}