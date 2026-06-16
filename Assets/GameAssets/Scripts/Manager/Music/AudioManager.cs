using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音乐枚举
/// </summary>
public enum MusicType
{
    MainMenu,   // 主菜单音乐
    Gameplay    // 游玩音乐
}

/// <summary>
/// 音效枚举
/// </summary>
public enum SoundEffectType
{
    RollDice,       // 投骰子
    ButtonClick,    // 按钮点击
    GameOver,       // 结算
    PassStart,      // 经过起点格
    Consume,        // 消费
    BadEvent,       // 坏事件
    GoodEvent,      // 好事件
    Death,          // 死亡
    PlayerMove,     // 玩家移动 (需要循环播放)
    Upgrade,        // 升级
    Mining          // 挖煤
}

/// <summary>
/// 音乐与音效管理器
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource;       // 播放背景音乐
    public AudioSource sfxSource;       // 播放单次音效 (允许重叠重放)
    public AudioSource moveSfxSource;   // 专门播放移动音效 (循环)

    [Serializable]
    public struct MusicConfig { public MusicType type; public AudioClip clip; }
    [Serializable]
    public struct SFXConfig { public SoundEffectType type; public AudioClip clip; }

    [Header("Audio Clips")]
    public List<MusicConfig> musicConfigs;
    public List<SFXConfig> sfxConfigs;

    // 内部转为字典方便快速查找
    private Dictionary<MusicType, AudioClip> musicDict = new Dictionary<MusicType, AudioClip>();
    private Dictionary<SoundEffectType, AudioClip> sfxDict = new Dictionary<SoundEffectType, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 保证切场景时不被销毁
            InitializeAudioDicts();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioDicts()
    {
        foreach (var m in musicConfigs) musicDict[m.type] = m.clip;
        foreach (var s in sfxConfigs) sfxDict[s.type] = s.clip;
    }

    private void OnEnable()
    {
        // 注册事件监听
        EventManager.Instance.AddListener(EventName.PlayMusic, OnPlayMusic);
        EventManager.Instance.AddListener(EventName.PlaySFX, OnPlaySFX);
        EventManager.Instance.AddListener(EventName.StopMoveSFX, OnStopMoveSFX);
    }

    private void OnDisable()
    {
        // 注销事件监听
        EventManager.Instance.RemoveListener(EventName.PlayMusic, OnPlayMusic);
        EventManager.Instance.RemoveListener(EventName.PlaySFX, OnPlaySFX);
        EventManager.Instance.RemoveListener(EventName.StopMoveSFX, OnStopMoveSFX);
    }

    #region 事件响应方法
    private void OnPlayMusic(object sender, EventArgs e)
    {
        var args = e as MusicEventArgs;
        if (args != null && musicDict.TryGetValue(args.musicType, out AudioClip clip))
        {
            if (bgmSource.clip == clip && bgmSource.isPlaying) return; // 已经在播这个BGM就不重新开始了
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    private void OnPlaySFX(object sender, EventArgs e)
    {
        var args = e as SFXEventArgs;
        if (args != null && sfxDict.TryGetValue(args.sfxType, out AudioClip clip))
        {
            if (args.sfxType == SoundEffectType.PlayerMove)
            {
                // 特殊处理移动音效：使用专用声道循环播放
                if (!moveSfxSource.isPlaying)
                {
                    moveSfxSource.clip = clip;
                    moveSfxSource.loop = true;
                    moveSfxSource.Play();
                }
            }
            else
            {
                // 普通音效：用 PlayOneShot 允许多个声音重叠
                sfxSource.PlayOneShot(clip);
            }
        }
    }

    private void OnStopMoveSFX(object sender, EventArgs e)
    {
        if (moveSfxSource.isPlaying)
        {
            moveSfxSource.Stop();
        }
    }
    #endregion
}
