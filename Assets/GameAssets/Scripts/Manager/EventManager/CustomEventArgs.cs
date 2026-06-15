using System;
using UnityEngine;

// ======== 储存事件参数类 ========

public class TestEventArgs : EventArgs
{
    public string testString;
}

// 格子交互事件参数
public class GridInteractionEventArgs : EventArgs
{
    public int gridIndex;
    public PlayerController player;
}

// 音乐播放事件参数
public class MusicEventArgs : EventArgs
{
    public MusicType musicType;
}

// 音效播放事件参数
public class SFXEventArgs : EventArgs
{
    public SoundEffectType sfxType;
}

// 摄像机跟踪目标切换事件参数
public class CameraTargetEventArgs : EventArgs
{
    public Transform targetTransform;
}

// 骰子动画事件参数
public class DiceAnimationEventArgs : EventArgs
{
    public int result; // 投出的具体点数
}
