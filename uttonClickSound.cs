using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonClickSound : MonoBehaviour, IPointerClickHandler
{
    [Header("音效设置")]
    [Tooltip("点击按钮时播放的音效")]
    public AudioClip clickSound;

    [Range(0f, 1f)]
    [Tooltip("音效音量")]
    public float volume = 0.7f;

    [Range(0.5f, 2f)]
    [Tooltip("音效播放速度")]
    public float pitch = 1.0f;

    [Tooltip("是否对音效应用随机音调变化")]
    public bool randomizePitch = false;

    [Range(0f, 0.3f)]
    [Tooltip("随机音调变化范围")]
    public float randomPitchRange = 0.1f;

    // 音频源引用
    private AudioSource audioSource;

    void Start()
    {
        // 尝试获取场景中已有的音频源
        audioSource = GetComponent<AudioSource>();

        // 如果没有找到音频源，则查找或创建一个
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

        // 为按钮的onClick事件添加音效播放函数（作为备用方法）
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    // 实现鼠标点击接口
    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClickSound();
    }

    // 播放点击音效
    public void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
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
            audioSource.PlayOneShot(clickSound);
        }
    }
}