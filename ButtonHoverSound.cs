using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    [Header("音效设置")]
    [Tooltip("鼠标悬停时播放的音效")]
    public AudioClip hoverSound;

    [Range(0f, 1f)]
    [Tooltip("音效音量")]
    public float volume = 0.5f;

    [Range(0.5f, 2f)]
    [Tooltip("音效播放速度")]
    public float pitch = 1.0f;

    [Tooltip("是否对音效应用随机音调变化")]
    public bool randomizePitch = false;

    [Range(0f, 0.3f)]
    [Tooltip("随机音调变化范围")]
    public float randomPitchRange = 0.1f;

    // 音频源引用，可以是场景中已有的，也可以由脚本创建
    private AudioSource audioSource;

    void Start()
    {
        // 尝试获取场景中已有的音频源
        audioSource = GetComponent<AudioSource>();

        // 如果没有找到音频源，则创建一个
        if (audioSource == null)
        {
            // 检查是否有全局音频控制器
            GameObject soundManager = GameObject.Find("SoundManager");
            if (soundManager != null)
            {
                audioSource = soundManager.GetComponent<AudioSource>();
            }

            // 如果仍然没有找到，则添加到当前物体
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
            }
        }
    }

    // 鼠标悬停时触发
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHoverSound();
    }

    // 播放悬停音效
    public void PlayHoverSound()
    {
        if (hoverSound != null && audioSource != null)
        {
            // 应用音量设置
            audioSource.volume = volume;

            // 应用音调设置，可以添加随机变化
            if (randomizePitch)
            {
                float randomPitch = pitch + Random.Range(-randomPitchRange, randomPitchRange);
                audioSource.pitch = randomPitch;
            }
            else
            {
                audioSource.pitch = pitch;
            }

            // 播放音效
            audioSource.PlayOneShot(hoverSound);
        }
    }
}