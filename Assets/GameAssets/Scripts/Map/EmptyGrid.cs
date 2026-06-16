using UnityEngine;

/// <summary>
/// 空白格控制器
/// </summary>
public class EmptyGrid : GridController
{
    public override void OnArrived(PlayerController player)
    {
        Debug.Log($"【空白格】{player.playerName} 停在白地。");
    }
}
