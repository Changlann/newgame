using UnityEngine;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    [Header("��������")]
    public AudioClip[] backgroundMusics;  // BGM����
    public float switchInterval = 180f;    // �л������Ĭ��180�루3���ӣ�
    public float fadeTime = 2.0f;         // ���뵭��ʱ��
    public float initialDelay = 240f;     // ��ʼ�ӳ�ʱ�䣨4���ӣ�

    [Header("��������")]
    [Range(0, 1)]
    public float musicVolume = 0.5f;      // ��������

    private AudioSource[] audioSources;    // ʹ������AudioSource��ʵ�ֵ��뵭��
    private int currentSource = 0;         // ��ǰ���ڲ��ŵ�AudioSource����
    private int currentMusicIndex = -1;    // ��ǰ���ŵ���������
    private bool isSwitching = false;      // �Ƿ������л�����
    private bool hasStartedPlaying = false; // �Ƿ��Ѿ���ʼ��������

    void Start()
    {
        // ����Ƿ��������ļ�
        if (backgroundMusics == null || backgroundMusics.Length == 0)
        {
            Debug.LogError("û�����ñ������֣�����Inspector�����������ļ���");
            enabled = false;
            return;
        }

        // ��������AudioSource������ڵ��뵭��
        audioSources = new AudioSource[2];
        for (int i = 0; i < 2; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].loop = false;  // ��ѭ�����ɹ��������Ʋ���
            audioSources[i].playOnAwake = false;
            audioSources[i].volume = 0;
        }

        // �����ӳٲ���Э��
        StartCoroutine(DelayedStart());
    }

    // �ӳٿ�ʼ�������ֵ�Э��
    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(initialDelay);
        hasStartedPlaying = true;
        PlayRandomMusic();
        StartCoroutine(MusicSwitchTimer());
    }

    // ���ѡ�񲢲�������
    void PlayRandomMusic()
    {
        if (isSwitching) return;

        // ���ѡ��һ�ײ�ͬ������
        int newIndex;
        do
        {
            newIndex = Random.Range(0, backgroundMusics.Length);
        } while (newIndex == currentMusicIndex && backgroundMusics.Length > 1);

        currentMusicIndex = newIndex;
        StartCoroutine(SwitchMusic(backgroundMusics[currentMusicIndex]));
    }

    // �����л�Э��
    IEnumerator SwitchMusic(AudioClip newClip)
    {
        isSwitching = true;

        // ��ȡ��ǰ����һ��AudioSource
        AudioSource currentAudio = audioSources[currentSource];
        AudioSource nextAudio = audioSources[(currentSource + 1) % 2];

        // ����������
        nextAudio.clip = newClip;
        nextAudio.volume = 0;
        nextAudio.Play();

        // ���뵭��
        float timer = 0;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float t = timer / fadeTime;

            nextAudio.volume = Mathf.Lerp(0, musicVolume, t);
            if (currentAudio.isPlaying)
            {
                currentAudio.volume = Mathf.Lerp(musicVolume, 0, t);
            }

            yield return null;
        }

        // ֹͣ������
        if (currentAudio.isPlaying)
        {
            currentAudio.Stop();
        }

        // �л���ǰԴ
        currentSource = (currentSource + 1) % 2;
        isSwitching = false;
    }

    // ��ʱ�л����ֵ�Э��
    IEnumerator MusicSwitchTimer()
    {
        while (true)
        {
            if (!hasStartedPlaying)
            {
                yield return null;
                continue;
            }

            // �ȴ���ǰ���ֲ�����ϻ�ﵽ�л�ʱ��
            float waitTime = switchInterval;
            if (audioSources[currentSource].clip != null)
            {
                waitTime = Mathf.Min(switchInterval, audioSources[currentSource].clip.length);
            }

            yield return new WaitForSeconds(waitTime);

            if (!isSwitching)
            {
                PlayRandomMusic();
            }
        }
    }

    // ����������������������
    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        audioSources[currentSource].volume = musicVolume;
    }

    // ������������ͣ����
    public void PauseMusic()
    {
        foreach (var source in audioSources)
        {
            if (source.isPlaying)
            {
                source.Pause();
            }
        }
    }

    // ����������������������
    public void ResumeMusic()
    {
        if (!hasStartedPlaying) return; // �����û����ʼ����ʱ�䣬��Ҫ�ָ�����

        foreach (var source in audioSources)
        {
            if (!source.isPlaying)
            {
                source.UnPause();
            }
        }
    }
}