using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地图管理器
/// </summary>
public class MapManager : MonoBehaviour
{
    // 单例
    public static MapManager Instance { get; private set; }

    // 存储环形地图所有格子的列表
    [Header("地图数据")]
    public List<GridController> gridList = new List<GridController>();

    private void Awake()
    {
        // 确保场景中只有一个实例
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // 注册监听器
        EventManager.Instance.AddListener(EventName.OnPlayerPassedGrid, OnPlayerPassedGridHandler);
        EventManager.Instance.AddListener(EventName.OnPlayerArrivedGrid, OnPlayerArrivedGridHandler);
    }

    private void OnDestroy()
    {
        // 移除监听器
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener(EventName.OnPlayerPassedGrid, OnPlayerPassedGridHandler);
            EventManager.Instance.RemoveListener(EventName.OnPlayerArrivedGrid, OnPlayerArrivedGridHandler);
        }
    }

    #region 事件处理逻辑
    private void OnPlayerPassedGridHandler(object sender, EventArgs e)
    {
        var args = e as GridInteractionEventArgs;
        if (args != null && args.gridIndex < gridList.Count)
        {
            // 调用对应格子的经过方法
            gridList[args.gridIndex].OnPassed(args.player);
        }
    }

    private void OnPlayerArrivedGridHandler(object sender, EventArgs e)
    {
        var args = e as GridInteractionEventArgs;
        if (args != null && args.gridIndex < gridList.Count)
        {
            // 调用对应格子的到达方法
            gridList[args.gridIndex].OnArrived(args.player);
        }
    }
    #endregion

    /// <summary>
    /// 获取移动后的目标格子索引
    /// </summary>
    public int GetTargetGridIndex(int currentIndex, int steps)
    {
        if (gridList.Count == 0)
        {
            Debug.LogError("MapManager 的 gridList 为空！请在面板中配置格子！");
            return 0;
        }
        // 走到末尾自动绕回起点
        return (currentIndex + steps) % gridList.Count;
    }

    /// <summary>
    /// 获取指定索引的格子坐标
    /// </summary>
    public Vector3 GetGridPosition(int index)
    {
        if (index >= 0 && index < gridList.Count)
        {
            return gridList[index].transform.position;
        }
        Debug.LogError($"获取格子坐标失败，索引越界: {index}");
        return Vector3.zero;
    }

    /// <summary>
    /// 获得特定格子的索引
    /// </summary>
    public int GetGridIndex(GridController grid)
    {
        return gridList.IndexOf(grid);
    }
}
