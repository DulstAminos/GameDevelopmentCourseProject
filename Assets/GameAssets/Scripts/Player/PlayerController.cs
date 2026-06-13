using DG.Tweening;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("基础属性")]
    public string playerName;
    public bool isAI;
    public int startGridIndex = 0; // 起点索引

    [Header("初始数值")]
    public int initialStamina = 5;
    public int maxStamina = 7;
    public int initialMoney = 200;
    public PlayerState initialState = PlayerState.Normal;
    public int miningTotalTurns = 2; // 挖煤所需总回合

    [Header("移动配置")]
    public float jumpPower = 1.5f;
    public float jumpDuration = 0.3f;

    // 内部数据
    private int currentStamina;
    private int currentMoney;
    private PlayerState currentState;
    private int currentGridIndex;
    private int currentMiningTurnsLeft;

    // 核心控制标志位
    [HideInInspector] public bool isActionDone = false; // 用于告诉主协程某阶段动作做完了

    private void Start()
    {
        // 初始化
        currentStamina = initialStamina;
        currentMoney = initialMoney;
        currentState = initialState;
        currentMiningTurnsLeft = 0;
        TeleportToGrid(startGridIndex);
    }

    #region 数据查询与修改基础接口
    public int GetStamina() => currentStamina;
    public int GetMoney() => currentMoney;
    public PlayerState GetState() => currentState;
    public int GetCurrentGridIndex() => currentGridIndex;
    public int GetMiningTurnsLeft() => currentMiningTurnsLeft;

    public void SetStamina(int value)
    {
        // 限制体力在 0 到 maxStamina 之间
        currentStamina = Mathf.Clamp(value, 0, maxStamina);
        // 触发体力改变事件
        this.TriggerEvent(EventName.OnStaminaChanged);
    }

    public void SetMoney(int value)
    {
        // 限制金币最小为0
        currentMoney = Mathf.Max(0, value);
        // 触发金币改变事件
        this.TriggerEvent(EventName.OnMoneyChanged);
    }

    public void SetState(PlayerState newState)
    {
        currentState = newState;
        // 触发状态更新事件
        this.TriggerEvent(EventName.OnPlayerStateChanged);
    }
    #endregion

    #region 资源消耗逻辑拓展
    /// <summary>
    /// 回合开始时的体力消耗
    /// </summary>
    public void DecreaseStamina()
    {
        SetStamina(currentStamina - 1);
    }

    /// <summary>
    /// 自主消费
    /// </summary>
    /// <returns>true 表示购买成功，false 表示钱不够</returns>
    public bool SpendMoney(int amount)
    {
        // TODO:可在此处根据角色类型（如年轻女孩）修改 amount 的值
        int finalCost = amount;

        if (currentMoney >= finalCost)
        {
            SetMoney(currentMoney - finalCost);
            return true;
        }
        return false;
    }
    #endregion

    #region 核心惩罚机制：挖煤逻辑
    /// <summary>
    /// 触发进入挖煤状态
    /// </summary>
    public void EnterMiningState()
    {
        SetState(PlayerState.Mining);
        currentMiningTurnsLeft = miningTotalTurns;

        // 进入瞬间扣1点体力
        DecreaseStamina();

        Debug.Log($"【{playerName}】进入挖煤状态，禁足 {currentMiningTurnsLeft} 回合。");

        if (GetStamina() < 0) Die(); // 体力小于0直接死亡
    }

    /// <summary>
    /// 处理挖煤期间的回合消耗与解禁
    /// </summary>
    public void ProcessMiningTurn()
    {
        if (currentState != PlayerState.Mining) return;

        currentMiningTurnsLeft--;
        Debug.Log($"【{playerName}】正在努力挖煤... 剩余禁足回合：{currentMiningTurnsLeft}");

        // 禁足倒计时归零，解除状态
        if (currentMiningTurnsLeft <= 0)
        {
            SetState(PlayerState.Normal);
            SetMoney(currentMoney + 50); // 获得50金币工资
            Debug.Log($"【{playerName}】刑满释放！状态恢复正常，获得 50 金币挖煤工资！");
        }
    }
    #endregion

    #region 移动与行为控制
    /// <summary>
    /// 瞬间传送到指定格子
    /// </summary>
    public void TeleportToGrid(int index)
    {
        currentGridIndex = index;
        transform.position = MapManager.Instance.GetGridPosition(index);
        Debug.Log($"【{playerName}】瞬移到了格子 [{index}]");

        // 瞬间移动完成后，触发“到达”事件
        var args = new GridInteractionEventArgs { gridIndex = currentGridIndex, player = this };
        this.TriggerEvent(EventName.OnPlayerArrivedGrid, args);
    }

    /// <summary>
    /// 向前移动
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

            // 每跳一步，触发“经过”事件
            var passArgs = new GridInteractionEventArgs { gridIndex = currentGridIndex, player = this };
            this.TriggerEvent(EventName.OnPlayerPassedGrid, passArgs);
        }

        Debug.Log($"【{playerName}】移动结束，停在了格子 [{currentGridIndex}]");

        // 全部走完，触发“到达”事件
        var arriveArgs = new GridInteractionEventArgs { gridIndex = currentGridIndex, player = this };
        this.TriggerEvent(EventName.OnPlayerArrivedGrid, arriveArgs);

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
        SetState(PlayerState.Dead);
        // 极简表现：隐藏模型
        gameObject.SetActive(false);
        Debug.Log($"【死亡】{playerName} 体力耗尽，已从棋盘移除！");
    }
    #endregion
}
