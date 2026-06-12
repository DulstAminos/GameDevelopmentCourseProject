using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("基础属性")]
    public string playerName;
    public bool isAI;

    // 核心资源
    public int stamina = 5;
    public int maxStamina = 7;
    public int money = 200;

    [Header("状态与位置")]
    public PlayerState state = PlayerState.Normal;
    public int currentGridIndex = 0;
    public int miningTurnsLeft = 0; // 挖煤倒计时

    // 核心控制标志位
    [HideInInspector] public bool isActionDone = false; // 用于告诉主协程某阶段动作做完了

    /// <summary>
    /// 模拟移动
    /// </summary>
    public void MoveToGrid(int targetIndex)
    {
        currentGridIndex = targetIndex;
        // 暂时直接瞬移到目标位置
        transform.position = MapManager.Instance.GetGridPosition(targetIndex);

        Debug.Log($"【{playerName}】移动到了格子 [{targetIndex}]");

        // 移动结束，释放协程卡点
        isActionDone = true;
    }

    /// <summary>
    /// 模拟格子结算完成
    /// </summary>
    public void FinishGridAction()
    {
        Debug.Log($"【{playerName}】完成了格子结算。");
        // 结算结束，释放协程卡点
        isActionDone = true;
    }
}
