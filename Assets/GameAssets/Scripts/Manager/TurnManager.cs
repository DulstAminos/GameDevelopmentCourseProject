using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public List<PlayerController> players;

    private int currentPlayerIndex = 0;
    private int currentRound = 1;
    public int MaxRound = 40; // 40回合上限

    private bool isWaitingForPlayerInput = false; // 临时变量，用于Day 1监听空格键

    void Start()
    {
        // 游戏开始，启动流水线
        StartCoroutine(GameLoopCoroutine());
    }

    void Update()
    {
        // 临时测试输入：当需要玩家掷骰子或确认时，按空格推进
        if (isWaitingForPlayerInput && Input.GetKeyDown(KeyCode.Space))
        {
            isWaitingForPlayerInput = false;
        }
    }

    /// <summary>
    /// 游戏总循环
    /// </summary>
    IEnumerator GameLoopCoroutine()
    {
        Debug.Log("======== 游戏开始 ========");

        while (currentRound <= MaxRound && GetAlivePlayerCount() > 1)
        {
            PlayerController currentPlayer = players[currentPlayerIndex];

            // 如果玩家还活着，执行他的回合
            if (currentPlayer.state != PlayerState.Dead)
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
        }

        // 游戏结束结算
        Debug.Log("======== 游戏结束 ========");
        if (GetAlivePlayerCount() == 1)
        {
            // 结束方式1：只剩一个人
            foreach (var p in players)
            {
                if (p.state != PlayerState.Dead)
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
        // 【第一步：回合开始】
        if (player.state == PlayerState.Mining)
        {
            // 挖煤状态处理逻辑
            Debug.Log($"{player.playerName} 正在挖煤禁足中...");
        }
        else
        {
            // 正常状态扣体力
            player.stamina -= 1;
            Debug.Log($"{player.playerName} 消耗了 1 点体力。剩余体力：{player.stamina}");

            // 玩家死亡判定
            if (player.stamina < 0)
            {
                Debug.Log($"【死亡】{player.playerName} 体力耗尽，被淘汰！");
                player.state = PlayerState.Dead;
                player.Die();
                yield break; // 立即结束当前协程
            }
        }

        // 【第二步：等待行动（掷骰子）】
        player.isActionDone = false;
        Debug.Log($"等待 {player.playerName} 掷骰子... (按空格模拟掷骰)");

        // 模拟等待玩家点击UI
        if (!player.isAI)
        {
            isWaitingForPlayerInput = true;
            yield return new WaitUntil(() => !isWaitingForPlayerInput);
        }

        int diceResult = Random.Range(1, 7);
        Debug.Log($"{player.playerName} 掷出了 {diceResult} 点！");


        // 【第三步：角色移动】
        player.isActionDone = false;
        player.MoveSteps(diceResult);
        yield return new WaitUntil(() => player.isActionDone);


        // 【第四步：格子结算与操作】
        player.isActionDone = false;
        Debug.Log($"触发格子 [{player.currentGridIndex}] 的事件... 等待玩家操作 (按空格模拟操作完毕)");

        // 模拟等待玩家点击"吃饭/升级/取消"
        if (!player.isAI)
        {
            isWaitingForPlayerInput = true;
            yield return new WaitUntil(() => !isWaitingForPlayerInput);
        }

        player.FinishGridAction();
        yield return new WaitUntil(() => player.isActionDone);


        // 【第五步：回合结束】
        // TODO: 可在此处发出 UI 更新事件
        // this.TriggerEvent(EventName.TurnEnded); 
        Debug.Log($"{player.playerName} 的回合结束。");
    }

    /// <summary>
    /// 获取存活玩家数量
    /// </summary>
    private int GetAlivePlayerCount()
    {
        int count = 0;
        foreach (var p in players)
        {
            if (p.state != PlayerState.Dead) count++;
        }
        return count;
    }
}
