using DG.Tweening;
using System.Collections;
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

    [Header("移动配置")]
    public float jumpPower = 1.5f;
    public float jumpDuration = 0.3f;

    // 核心控制标志位
    [HideInInspector] public bool isActionDone = false; // 用于告诉主协程某阶段动作做完了

    /// <summary>
    /// 触发移动
    /// </summary>
    public void MoveSteps(int steps)
    {
        StartCoroutine(MoveStepByStepCoroutine(steps));
    }

    /// <summary>
    /// 协程：控制角色一格一格地跳跃
    /// </summary>
    private IEnumerator MoveStepByStepCoroutine(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            // 计算下一格的索引
            int nextIndex = MapManager.Instance.GetTargetGridIndex(currentGridIndex, 1);
            Vector3 targetPos = MapManager.Instance.GetGridPosition(nextIndex);

            // 执行跳跃动画 (目标点，跳跃力度，跳跃次数，持续时间)
            bool isSingleJumpDone = false;
            transform.DOJump(targetPos, jumpPower, numJumps: 1, jumpDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    isSingleJumpDone = true; // 动画播完的回调
                });

            // 等待这一小步跳完，再跳下一步
            yield return new WaitUntil(() => isSingleJumpDone);

            // 更新当前所在格子索引
            currentGridIndex = nextIndex;
        }

        Debug.Log($"【{playerName}】移动结束，停在了格子 [{currentGridIndex}]");
        isActionDone = true; // 全部跳完，释放主流程卡点
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

    /// <summary>
    /// 死亡表现
    /// </summary>
    public void Die()
    {
        state = PlayerState.Dead;
        // 极简表现：直接隐藏模型
        gameObject.SetActive(false);
        Debug.Log($"【死亡】{playerName} 体力耗尽，已从棋盘移除！");
    }
}
