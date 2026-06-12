using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地图管理器
/// </summary>
public class MapManager : MonoBehaviour
{
    // 单例
    public static MapManager Instance { get; private set; }

    private void Awake()
    {
        // 确保场景中只有一个实例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("场景中存在多个MapManager，已销毁多余的实例。");
            Destroy(gameObject);
        }
    }

    [Header("地图数据")]
    // 存储环形地图所有格子的列表
    public List<GridController> gridList = new List<GridController>();

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
        // 环形求余算法：走到末尾自动绕回起点
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
}
