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
}
