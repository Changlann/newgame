using UnityEngine;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    [Header("音乐设置")]
    public AudioClip[] backgroundMusics;  // BGM数组
    public float switchInterval = 180f;    // 切换间隔，默认180秒（3分钟）
    public float fadeTime = 2.0f;         // 淡入淡出时间
    public float initialDelay = 240f;     // 初始延迟时间（4分钟）

    [Header("音量设置")]
    [Range(0, 1)]
    public float musicVolume = 0.5f;      // 音乐音量

    private AudioSource[] audioSources;    // 使用两个AudioSource来实现淡入淡出
    private int currentSource = 0;         // 当前正在播放的AudioSource索引
    private int currentMusicIndex = -1;    // 当前播放的音乐索引
    private bool isSwitching = false;      // 是否正在切换音乐
    private bool hasStartedPlaying = false; // 是否已经开始播放音乐

    void Start()
    {
        // 检查是否有音乐文件
        if (backgroundMusics == null || backgroundMusics.Length == 0)
        {
            Debug.LogError("没有设置背景音乐！请在Inspector中设置音乐文件！");
            enabled = false;
            return;
        }

        // 创建两个AudioSource组件用于淡入淡出
        audioSources = new AudioSource[2];
        for (int i = 0; i < 2; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].loop = false;  // 不循环，由管理器控制播放
            audioSources[i].playOnAwake = false;
            audioSources[i].volume = 0;
        }

        // 启动延迟播放协程
        StartCoroutine(DelayedStart());
    }

    // 延迟开始播放音乐的协程
    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(initialDelay);
        hasStartedPlaying = true;
        PlayRandomMusic();
        StartCoroutine(MusicSwitchTimer());
    }

    // 随机选择并播放音乐
    void PlayRandomMusic()
    {
        if (isSwitching) return;

        // 随机选择一首不同的音乐
        int newIndex;
        do
        {
            newIndex = Random.Range(0, backgroundMusics.Length);
        } while (newIndex == currentMusicIndex && backgroundMusics.Length > 1);

        currentMusicIndex = newIndex;
        StartCoroutine(SwitchMusic(backgroundMusics[currentMusicIndex]));
    }

    // 音乐切换协程
    IEnumerator SwitchMusic(AudioClip newClip)
    {
        isSwitching = true;

        // 获取当前和下一个AudioSource
        AudioSource currentAudio = audioSources[currentSource];
        AudioSource nextAudio = audioSources[(currentSource + 1) % 2];

        // 设置新音乐
        nextAudio.clip = newClip;
        nextAudio.volume = 0;
        nextAudio.Play();

        // 淡入淡出
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

        // 停止旧音乐
        if (currentAudio.isPlaying)
        {
            currentAudio.Stop();
        }

        // 切换当前源
        currentSource = (currentSource + 1) % 2;
        isSwitching = false;
    }

    // 定时切换音乐的协程
    IEnumerator MusicSwitchTimer()
    {
        while (true)
        {
            if (!hasStartedPlaying)
            {
                yield return null;
                continue;
            }

            // 等待当前音乐播放完毕或达到切换时间
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

    // 公共方法：设置音乐音量
    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        audioSources[currentSource].volume = musicVolume;
    }

    // 公共方法：暂停音乐
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

    // 公共方法：继续播放音乐
    public void ResumeMusic()
    {
        if (!hasStartedPlaying) return; // 如果还没到开始播放时间，不要恢复播放

        foreach (var source in audioSources)
        {
            if (!source.isPlaying)
            {
                source.UnPause();
            }
        }
    }
}