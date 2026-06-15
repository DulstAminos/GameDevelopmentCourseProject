/// <summary>
/// 储存事件名称
/// </summary>
public static class EventName
{
    public const string Test = nameof(Test);

    // 玩家资源变化事件
    public const string OnStaminaChanged = nameof(OnStaminaChanged);
    public const string OnMoneyChanged = nameof(OnMoneyChanged);

    // 格子交互事件
    public const string OnPlayerPassedGrid = nameof(OnPlayerPassedGrid);
    public const string OnPlayerArrivedGrid = nameof(OnPlayerArrivedGrid);

    // UI刷新事件
    public const string OnRoundOrTurnChanged = nameof(OnRoundOrTurnChanged);
    public const string OnPlayerStateChanged = nameof(OnPlayerStateChanged); // 玩家状态改变时

    // 音频控制事件
    public const string PlayMusic = nameof(PlayMusic);
    public const string PlaySFX = nameof(PlaySFX);
    public const string StopMoveSFX = nameof(StopMoveSFX); // 专门用于停止玩家移动音效

    // 摄像机切换跟随目标事件
    public const string OnCameraChangeTarget = nameof(OnCameraChangeTarget);

    // 触发骰子动画
    public const string ShowDiceAnimation = nameof(ShowDiceAnimation);
}
