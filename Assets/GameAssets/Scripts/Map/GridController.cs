using UnityEngine;

/// <summary>
/// 格子控制器
/// </summary>
public class GridController : MonoBehaviour
{
    public int GridIndex => MapManager.Instance.GetGridIndex(this);// 格子在地图列表中的序号

    /// <summary>
    /// 玩家经过该格子时触发
    /// </summary>
    public virtual void OnPassed(PlayerController player)
    {
        // 基类默认无操作，留给子类重写
    }

    /// <summary>
    /// 玩家最终停在该格子时触发
    /// </summary>
    public virtual void OnArrived(PlayerController player)
    {
        // 基类默认无操作，留给子类重写
    }
}
