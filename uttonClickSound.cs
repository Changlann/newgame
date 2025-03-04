using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonClickSound : MonoBehaviour, IPointerClickHandler
{
    [Header("��Ч����")]
    [Tooltip("�����ťʱ���ŵ���Ч")]
    public AudioClip clickSound;

    [Range(0f, 1f)]
    [Tooltip("��Ч����")]
    public float volume = 0.7f;

    [Range(0.5f, 2f)]
    [Tooltip("��Ч�����ٶ�")]
    public float pitch = 1.0f;

    [Tooltip("�Ƿ����ЧӦ����������仯")]
    public bool randomizePitch = false;

    [Range(0f, 0.3f)]
    [Tooltip("��������仯��Χ")]
    public float randomPitchRange = 0.1f;

    // ��ƵԴ����
    private AudioSource audioSource;

    void Start()
    {
        // ���Ի�ȡ���������е���ƵԴ
        audioSource = GetComponent<AudioSource>();

        // ���û���ҵ���ƵԴ������һ򴴽�һ��
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

        // Ϊ��ť��onClick�¼������Ч���ź�������Ϊ���÷�����
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    // ʵ��������ӿ�
    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClickSound();
    }

    // ���ŵ����Ч
    public void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
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
            audioSource.PlayOneShot(clickSound);
        }
    }
}