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

public class MusicEventArgs : EventArgs
{
    public MusicType musicType;
}

public class SFXEventArgs : EventArgs
{
    public SoundEffectType sfxType;
}

public class CameraTargetEventArgs : EventArgs
{
    public Transform targetTransform;
}
