using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private Dictionary<string, AudioSource> soundSources = new Dictionary<string, AudioSource>();
    private AudioSource bgmSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(string soundName, AudioClip clip, bool isLoop = false, float volume = 1f)
    {
        // 如果已存在这个音效的AudioSource，就重用它
        if (soundSources.TryGetValue(soundName, out AudioSource source))
        {
            source.clip = clip;
            source.volume = volume;
            source.loop = isLoop;
            source.Play();
            return;
        }

        // 否则创建新的AudioSource
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = clip;
        newSource.volume = volume;
        newSource.loop = isLoop;
        newSource.Play();

        soundSources[soundName] = newSource;
    }

    public void PlayBGM(AudioClip clip, float volume = 1f)
    {
        // 如果BGM源不存在，创建一个
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }

        // 如果是同一个BGM，不重新播放
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.Play();
    }

    public void StopSound(string soundName)
    {
        if (soundSources.TryGetValue(soundName, out AudioSource source))
        {
            source.Stop();
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void SetVolume(string soundName, float volume)
    {
        if (soundSources.TryGetValue(soundName, out AudioSource source))
        {
            source.volume = volume;
        }
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = volume;
        }
    }

    // 可选：清理不再使用的AudioSource
    public void ClearUnusedSources()
    {
        List<string> keysToRemove = new List<string>();
        
        foreach (var pair in soundSources)
        {
            if (!pair.Value.isPlaying && !pair.Value.loop)
            {
                keysToRemove.Add(pair.Key);
                Destroy(pair.Value);
            }
        }

        foreach (string key in keysToRemove)
        {
            soundSources.Remove(key);
        }
    }

    public void PlaySoundFromResources(string resourcePath, string soundName, bool isLoop = false, float volume = 1f)
    {
        AudioClip clip = Resources.Load<AudioClip>(resourcePath);
        if (clip != null)
        {
            PlaySound(soundName, clip, isLoop, volume);
        }
        else
        {
            Debug.LogWarning($"找不到音频文件: {resourcePath}");
        }
    }

    public void PlayBGMFromResources(string resourcePath, float volume = 1f)
    {
        AudioClip clip = Resources.Load<AudioClip>(resourcePath);
        if (clip != null)
        {
            PlayBGM(clip, volume);
        }
        else
        {
            Debug.LogWarning($"找不到BGM文件: {resourcePath}");
        }
    }
}