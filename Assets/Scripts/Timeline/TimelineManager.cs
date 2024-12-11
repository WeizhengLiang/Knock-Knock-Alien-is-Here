using UnityEngine;
using UnityEngine.Playables;

public class TimelineManager : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    private bool hasSoundStarted = false;
    
    private void Awake()
    {
        if (director == null)
        {
            director = GetComponent<PlayableDirector>();
        }
        
        // 注册时间轴事件
        if (director != null)
        {
            director.played += Director_Played;
            director.stopped += Director_Stopped;
            director.paused += Director_Paused;
        }
    }
    
    private void Update()
    {
        if (director != null && director.state == PlayState.Playing)
        {
            // 检查时间点来触发音效
            double currentTime = director.time;
            
            // 例如在2秒时播放音效
            if (currentTime >= 2.0 && !hasSoundStarted)
            {
                SoundManager.Instance.PlaySoundFromResources("Sound/RealEnding", "RealEnding", false, 1.0f);
                hasSoundStarted = true;
            }
        }
    }
    
    private void Director_Played(PlayableDirector obj)
    {
        hasSoundStarted = false;
        // 可以在这里播放开始音效
        SoundManager.Instance.PlaySoundFromResources("Sound/StartSound", "StartSound", false, 1.0f);
    }
    
    private void Director_Stopped(PlayableDirector obj)
    {
        // 停止所有相关音效
        SoundManager.Instance.StopSound("RealEnding");
    }
    
    private void Director_Paused(PlayableDirector obj)
    {
        // 处理暂停逻辑
    }
    
    private void OnDestroy()
    {
        if (director != null)
        {
            director.played -= Director_Played;
            director.stopped -= Director_Stopped;
            director.paused -= Director_Paused;
        }
    }
}