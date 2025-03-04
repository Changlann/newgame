using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    [Header("��Ч����")]
    [Tooltip("�����ͣʱ���ŵ���Ч")]
    public AudioClip hoverSound;

    [Range(0f, 1f)]
    [Tooltip("��Ч����")]
    public float volume = 0.5f;

    [Range(0.5f, 2f)]
    [Tooltip("��Ч�����ٶ�")]
    public float pitch = 1.0f;

    [Tooltip("�Ƿ����ЧӦ����������仯")]
    public bool randomizePitch = false;

    [Range(0f, 0.3f)]
    [Tooltip("��������仯��Χ")]
    public float randomPitchRange = 0.1f;

    // ��ƵԴ���ã������ǳ��������еģ�Ҳ�����ɽű�����
    private AudioSource audioSource;

    void Start()
    {
        // ���Ի�ȡ���������е���ƵԴ
        audioSource = GetComponent<AudioSource>();

        // ���û���ҵ���ƵԴ���򴴽�һ��
        if (audioSource == null)
        {
            // ����Ƿ���ȫ����Ƶ������
            GameObject soundManager = GameObject.Find("SoundManager");
            if (soundManager != null)
            {
                audioSource = soundManager.GetComponent<AudioSource>();
            }

            // �����Ȼû���ҵ�������ӵ���ǰ����
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
            }
        }
    }

    // �����ͣʱ����
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHoverSound();
    }

    // ������ͣ��Ч
    public void PlayHoverSound()
    {
        if (hoverSound != null && audioSource != null)
        {
            // Ӧ����������
            audioSource.volume = volume;

            // Ӧ���������ã������������仯
            if (randomizePitch)
            {
                float randomPitch = pitch + Random.Range(-randomPitchRange, randomPitchRange);
                audioSource.pitch = randomPitch;
            }
            else
            {
                audioSource.pitch = pitch;
            }

            // ������Ч
            audioSource.PlayOneShot(hoverSound);
        }
    }
}