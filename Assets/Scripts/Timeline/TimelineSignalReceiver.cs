using UnityEngine;
using UnityEngine.Timeline;

public class TimelineSignalReceiver : MonoBehaviour
{
    private enum SoundSequence
    {
        DoorBreak,
        EndBob,
        UfoAway,
        None
    }

    private SoundSequence currentSound = SoundSequence.None;
    private string currentSoundName = "";

    // 这个方法会在Timeline中被Signal调用
    public void OnPlaySound(string soundName)
    {
        Debug.Log($"Playing sound: {soundName}"); // 用于调试
        SoundManager.Instance?.PlaySoundFromResources($"Sound/{soundName}", soundName, false, 1.0f);
    }

    public void OnStopSound(string soundName)
    {
        Debug.Log($"Stopping sound: {soundName}"); // 用于调试
        SoundManager.Instance?.StopSound(soundName);
    }

    // 可以添加更多方法来处理不同的音频控制需求
    public void OnSetVolume(string soundName, float volume)
    {
        SoundManager.Instance?.SetVolume(soundName, volume);
    }

    // 按顺序播放下一个音效
    public void PlayNextSound()
    {
        // 如果当前没有播放音效，从头开始
        if (currentSound == SoundSequence.None)
        {
            currentSound = SoundSequence.DoorBreak;
        }

        switch (currentSound)
        {
            case SoundSequence.DoorBreak:
                PlayDoorBreakSound();
                currentSound = SoundSequence.EndBob;
                currentSoundName = "4Door-break";
                break;

            case SoundSequence.EndBob:
                PlayEndBobSound();
                currentSound = SoundSequence.UfoAway;
                currentSoundName = "5End3-Bob2";
                break;

            case SoundSequence.UfoAway:
                PlayUfoAwaySound();
                currentSound = SoundSequence.None;  // 循环结束
                currentSoundName = "5UFO-away";
                break;
        }

        Debug.Log($"Playing sound sequence: {currentSoundName}");
    }

    // 停止当前播放的音效
    public void StopCurrentSound()
    {
        if (!string.IsNullOrEmpty(currentSoundName))
        {
            SoundManager.Instance.StopSound(currentSoundName);
            Debug.Log($"Stopping current sound: {currentSoundName}");
            currentSound = GetNextSequence(currentSound);  // 移动到下一个音效
            currentSoundName = "";
        }
    }

    // 获取序列中的下一个状态
    private SoundSequence GetNextSequence(SoundSequence current)
    {
        switch (current)
        {
            case SoundSequence.DoorBreak:
                return SoundSequence.EndBob;
            case SoundSequence.EndBob:
                return SoundSequence.UfoAway;
            case SoundSequence.UfoAway:
            default:
                return SoundSequence.None;
        }
    }

    // BGM 控制
    public void PlayBGM()
    {
        SoundManager.Instance.PlaySoundFromResources("Sound/RealEndingBGM", "RealEndingBGM", false, 1.0f);
        Debug.Log("Playing BGM: RealEndingBGM");
    }

    public void StopBGM()
    {
        SoundManager.Instance.StopSound("RealEndingBGM");
        Debug.Log("Stopping BGM: RealEndingBGM");
    }

    // 原有的独立控制方法
    public void PlayDoorBreakSound()
    {
        SoundManager.Instance.PlaySoundFromResources("Sound/4Door-break", "4Door-break", false, 1.0f);
    }

    public void StopDoorBreakSound()
    {
        SoundManager.Instance.StopSound("4Door-break");
    }

    public void PlayEndBobSound()
    {
        SoundManager.Instance.PlaySoundFromResources("Sound/5End3-Bob2", "5End3-Bob2", false, 1.0f);
    }

    public void StopEndBobSound()
    {
        SoundManager.Instance.StopSound("5End3-Bob2");
    }

    public void PlayUfoAwaySound()
    {
        SoundManager.Instance.PlaySoundFromResources("Sound/5UFO-away", "5UFO-away", false, 1.0f);
    }

    public void StopUfoAwaySound()
    {
        SoundManager.Instance.StopSound("5UFO-away");
    }

    public void PlayRealEndingBGMSound()
    {
        SoundManager.Instance.PlaySoundFromResources("Sound/RealEndingBGM", "RealEndingBGM", false, 1.0f);
    }

    public void StopRealEndingBGMSound()
    {
        SoundManager.Instance.StopSound("RealEndingBGM");
    }
}