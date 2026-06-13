using System;

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
