using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public List<PlayerController> players;

    private int currentPlayerIndex = 0;
    private int currentRound = 1;
    public int MaxRound = 40; // 40回合上限

    private bool isDiceRolled = false;
    private int diceResult = 0;

    // 当前状态的查询接口
    public int CurrentPlayerIndex => currentPlayerIndex;
    public int CurrentRound => currentRound;

    void Start()
    {
        // 游戏开始，启动流水线
        StartCoroutine(GameLoopCoroutine());
    }

    /// <summary>
    /// 游戏总循环
    /// </summary>
    IEnumerator GameLoopCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("======== 游戏开始 ========");

        // 开局刷新一次UI
        this.TriggerEvent(EventName.OnRoundOrTurnChanged);

        while (currentRound <= MaxRound && GetAlivePlayerCount() > 1)
        {
            PlayerController currentPlayer = players[currentPlayerIndex];

            // 如果玩家还活着，执行他的回合
            if (currentPlayer.GetState() != PlayerState.Dead)
            {
                Debug.Log($"\n--- 第 {currentRound} 回合：轮到 {currentPlayer.playerName} ---");
                yield return StartCoroutine(PlayerTurnRoutine(currentPlayer));
            }

            // 换人逻辑
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            // 如果又轮到第一个玩家，说明一圈结束，回合数+1
            if (currentPlayerIndex == 0)
            {
                currentRound++;
            }

            // 每次换人或换回合，通知UI刷新
            this.TriggerEvent(EventName.OnRoundOrTurnChanged);
        }

        // 游戏结束结算
        Debug.Log("======== 游戏结束 ========");
        if (GetAlivePlayerCount() == 1)
        {
            // 结束方式1：只剩一个人
            foreach (var p in players)
            {
                if (p.GetState() != PlayerState.Dead)
                    Debug.Log($"恭喜【{p.playerName}】熬死了所有人，获得最终胜利！");
            }
        }
        else
        {
            // 结束方式2：回合数达到上限
            Debug.Log($"{MaxRound}回合已到！根据总资产结算排名（待开发...）");
        }
    }

    /// <summary>
    /// 单个玩家的回合流程
    /// </summary>
    IEnumerator PlayerTurnRoutine(PlayerController player)
    {
        // 停顿一会，防止切换太快
        yield return new WaitForSeconds(0.8f);

        // 【第一步：回合开始】
        if (player.GetState() == PlayerState.Mining)
        {
            // 挖煤状态处理逻辑
            player.ProcessMiningTurn();
            Debug.Log($"{player.playerName} 正在挖煤禁足中...");
            yield break; // 强制结束该玩家本回合
        }
        else
        {
            // 玩家死亡判定
            if (player.GetStamina() <= 0)
            {
                PopupManager.Instance.ShowPopup("玩家淘汰", $"【{player.playerName}】因体力透支，饿死在街头！", null, null, "太惨了");
                player.Die();
                yield break; // 立即结束当前协程
            }
            // 正常状态扣体力
            player.DecreaseStamina();
            Debug.Log($"{player.playerName} 消耗了 1 点体力。剩余体力：{player.GetStamina()}");
        }

        // 【第二步：等待行动（掷骰子）】
        player.isActionDone = false;
        isDiceRolled = false;
        Debug.Log($"等待 {player.playerName} 掷骰子... ");

        // 模拟等待玩家点击UI
        if (player.isAI)
        {
            // AI 自动掷骰子
            yield return new WaitForSeconds(0.5f);
            RollDice(Random.Range(1, 7));
        }
        else
        {
            // 真人玩家：激活骰子按钮，挂起协程直到玩家点击
            PlayUIManager.Instance.diceBtn.interactable = true;

            // 给骰子按钮临时绑定一个匿名方法
            UnityEngine.Events.UnityAction btnAction = null;
            btnAction = () =>
            {
                PlayUIManager.Instance.diceBtn.interactable = false; // 点完禁用
                PlayUIManager.Instance.diceBtn.onClick.RemoveListener(btnAction); // 移除监听防重复
                RollDice(Random.Range(1, 7)); // 随机摇点并改变标记位
            };
            PlayUIManager.Instance.diceBtn.onClick.AddListener(btnAction);
        }
        // 挂起协程，等待 RollDice 方法被调用，isDiceRolled 变成 true
        yield return new WaitUntil(() => isDiceRolled);

        // 【第三步：角色移动】
        player.isActionDone = false;
        player.MoveSteps(diceResult);
        yield return new WaitUntil(() => player.isActionDone);


        // 【第四步：格子结算与操作】
        player.isActionDone = false;

        // 获取当前脚下的格子引用
        int currentIdx = player.GetCurrentGridIndex();
        GridController currentGrid = MapManager.Instance.gridList[currentIdx];

        // 如果玩家因为交不起过路费破产变成了挖煤状态，此时直接跳过操作阶段
        if (player.GetState() != PlayerState.Normal)
        {
            Debug.Log("玩家已破产或死亡，跳过自选操作阶段。");
            player.FinishGridAction();
        }
        else
        {
            if (player.isAI)
            {
                // AI 简易逻辑
                yield return new WaitForSeconds(0.5f);
                DoAIAction(player, currentGrid);
            }
            else
            {
                // PlayUIManager弹出底部操作按钮，让玩家进行操作
                PlayUIManager.Instance.EnablePlayerActions(player, currentGrid);
            }
        }

        yield return new WaitUntil(() => player.isActionDone);


        // 【第五步：回合结束】
        Debug.Log($"{player.playerName} 的回合结束。");
    }

    private void RollDice(int result)
    {
        diceResult = result;
        isDiceRolled = true; // 释放第二步的卡点
    }

    private void DoAIAction(PlayerController player, GridController grid)
    {
        RestaurantGrid rGrid = grid as RestaurantGrid;
        if (rGrid != null)
        {
            // 极简 AI 行为规则
            if (rGrid.level == 0 && player.GetMoney() >= 150)
            {
                if (player.SpendMoney(100)) rGrid.UpgradeOrBuild(player);
            }
            else if (rGrid.owner == player)
            {
                if (player.GetStamina() <= 4 && player.GetMoney() >= 50)
                {
                    if (player.SpendMoney(50)) player.SetStamina(player.GetStamina() + 2);
                }
                else if (player.GetMoney() >= 200 && rGrid.level < 3)
                {
                    if (player.SpendMoney(200)) rGrid.UpgradeOrBuild(player);
                }
            }
        }
        player.FinishGridAction(); // AI操作结束
    }

    /// <summary>
    /// 获取存活玩家数量
    /// </summary>
    private int GetAlivePlayerCount()
    {
        int count = 0;
        foreach (var p in players)
        {
            if (p.GetState() != PlayerState.Dead) count++;
        }
        return count;
    }
}
