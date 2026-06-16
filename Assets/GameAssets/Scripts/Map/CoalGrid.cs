using UnityEngine;

/// <summary>
/// 煤矿格控制器
/// </summary>
public class CoalGrid : GridController
{
    public override void OnArrived(PlayerController player)
    {
        Debug.Log($"【煤矿格】{player.playerName} 踩中煤矿！");
        player.EnterMiningState();
    }
}
