using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("游戏配置")]
    public List<PlayerController> players;
    public int MaxRound = 40; // 40回合上限

    [Header("AI 行为配置")]
    public int aiReserveMoney = 50;           // AI 预留防身金钱
    public int aiStaminaDangerLevel = 4;      // AI 认为危险的体力阈值

    [Header("结算 UI 组件")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;
    public Button returnMenuBtn;

    private int currentPlayerIndex = 0;
    private int currentRound = 1;

    private bool isDiceRolled = false;
    private int diceResult = 0;

    // 当前状态的查询接口
    public int CurrentPlayerIndex => currentPlayerIndex;
    public int CurrentRound => currentRound;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 触发播放游玩音乐
        this.TriggerEvent(EventName.PlayMusic, new MusicEventArgs { musicType = MusicType.Gameplay });
        // 根据GameData的数据，设置谁是玩家，谁是AI
        for (int i = 0; i < players.Count; i++)
        {
            // 如果索引匹配，说明是玩家选的，isAI 设为 false；其余全是 true
            players[i].isAI = (i != GameData.SelectedPlayerIndex);
        }

        // 绑定返回主菜单按钮
        if (returnMenuBtn != null)
        {
            returnMenuBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("MainMenu");
            });
        }

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
        PlayerController winner = GetWinner();
        ShowGameOverUI(winner);
    }

    /// <summary>
    /// 单个玩家的回合流程
    /// </summary>
    IEnumerator PlayerTurnRoutine(PlayerController player)
    {
        // 通知摄像机目标切换为当前玩家
        this.TriggerEvent(EventName.OnCameraChangeTarget, new CameraTargetEventArgs { targetTransform = player.transform });

        // 停顿一会，防止切换太快
        yield return new WaitForSeconds(0.8f);

        // 【第一步：回合开始】
        // 判定是否跳过回合
        if (player.skipNextTurn)
        {
            player.skipNextTurn = false;
            PopupManager.Instance.ShowPopup("养伤中", $"【{player.playerName}】正在骨科医院躺着，跳过本回合。", null, null, "同情");
            yield return new WaitForSeconds(1.5f); // 停顿一下让弹窗显示
            PopupManager.Instance.popupPanel.SetActive(false);
            yield break;
        }

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

        // 【第二步：掷骰子】
        player.isActionDone = false;
        isDiceRolled = false;

        // 模拟等待玩家点击UI
        if (player.isAI)
        {
            // AI 自动掷骰子
            yield return new WaitForSeconds(0.5f);
            RollDice(Random.Range(1, 7));
        }
        else
        {
            // 等待玩家点击投骰子按钮
            BindDiceEvent();
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
            player.FinishGridAction();
        }
        else
        {
            RestaurantGrid rGrid = currentGrid as RestaurantGrid;
            // 触发随机事件
            if (rGrid != null && rGrid.level == 0)
            {
                bool isEventFinished = false;
                // 触发随机事件，挂起协程直到玩家关掉事件弹窗
                RandomEventManager.Instance.TriggerRandomEvent(player, () =>
                {
                    isEventFinished = true;
                });
                yield return new WaitUntil(() => isEventFinished);
            }

            // 事件结束后，如果在事件中没死没挖煤，才允许操作
            if (player.GetState() != PlayerState.Dead && player.GetState() != PlayerState.Mining)
            {
                if (player.isAI)
                {
                    yield return new WaitForSeconds(0.5f);
                    DoAIAction(player, currentGrid);
                }
                else
                {
                    PlayUIManager.Instance.EnablePlayerActions(player, currentGrid);
                }
            }
            else
            {
                player.FinishGridAction(); // 在事件里死了，直接结束操作环节
            }
        }

        yield return new WaitUntil(() => player.isActionDone);


        // 【第五步：回合结束】
        Debug.Log($"{player.playerName} 的回合结束。");
    }

    #region 掷骰子相关逻辑
    private void BindDiceEvent()
    {
        PlayUIManager.Instance.diceBtn.interactable = true;

        UnityEngine.Events.UnityAction btnAction = null;
        btnAction = () =>
        {
            PlayUIManager.Instance.diceBtn.interactable = false;
            PlayUIManager.Instance.diceBtn.onClick.RemoveListener(btnAction);
            RollDice(Random.Range(1, 7));
        };
        PlayUIManager.Instance.diceBtn.onClick.AddListener(btnAction);
    }


    private void RollDice(int result)
    {
        this.TriggerEvent(EventName.PlaySFX, new SFXEventArgs { sfxType = SoundEffectType.RollDice });
        diceResult = result;
        isDiceRolled = true; // 释放第二步的卡点
    }
    #endregion

    // AI行为
    private void DoAIAction(PlayerController player, GridController grid)
    {
        RestaurantGrid rGrid = grid as RestaurantGrid;
        if (rGrid != null)
        {
            // 操作阶段 1：如果在任何饭店且体力低，优先补充体力
            while (rGrid.level > 0 && player.GetStamina() <= aiStaminaDangerLevel && player.GetStamina() < player.maxStamina)
            {
                bool isMy = (rGrid.owner == player);
                int baseCost = rGrid.consumeCosts[rGrid.level - 1];
                int cost = isMy ? Mathf.FloorToInt(baseCost * rGrid.ownerDiscount) : baseCost;

                if (player.GetMoney() >= cost + aiReserveMoney)
                {
                    player.SpendMoney(cost);
                    if (!isMy) rGrid.OwnerMakeMoney(cost);
                    player.RecoverStamina(rGrid.staminaRecovers[rGrid.level - 1]);
                }
                else break; // 没钱吃饭了，退出循环
            }

            // 操作阶段 2：投资建造/升级
            if (rGrid.level == 0 && player.GetMoney() >= rGrid.buildCost + aiReserveMoney)
            {
                if (player.SpendMoney(rGrid.buildCost)) rGrid.UpgradeOrBuild(player);
            }
            else if (rGrid.owner == player && rGrid.level > 0 && rGrid.level < 3)
            {
                int upCost = rGrid.upgradeCosts[rGrid.level - 1];
                if (player.GetMoney() >= upCost + aiReserveMoney)
                {
                    if (player.SpendMoney(upCost)) rGrid.UpgradeOrBuild(player);
                }
            }
        }
        player.FinishGridAction(); // AI操作结束
    }

    // 获取存活玩家数量
    private int GetAlivePlayerCount()
    {
        int count = 0;
        foreach (var p in players)
        {
            if (p.GetState() != PlayerState.Dead) count++;
        }
        return count;
    }

    /// <summary>
    /// 计算获胜者逻辑
    /// </summary>
    private PlayerController GetWinner()
    {
        PlayerController bestPlayer = null;

        if (GetAlivePlayerCount() == 1)
        {
            // 独自生还的情况
            foreach (var p in players)
            {
                if (p.GetState() != PlayerState.Dead) return p;
            }
        }
        else
        {
            // 回合上限到达，根据总资产（现金+剩余体力*20）计算排名
            int maxAssets = -1;
            foreach (var p in players)
            {
                if (p.GetState() == PlayerState.Dead) continue;

                int totalAssets = p.GetMoney() + p.GetStamina() * 20;

                if (totalAssets > maxAssets)
                {
                    maxAssets = totalAssets;
                    bestPlayer = p;
                }
            }
        }
        return bestPlayer;
    }

    /// <summary>
    /// 显示游戏结束UI
    /// </summary>
    private void ShowGameOverUI(PlayerController winner)
    {
        if (gameOverPanel != null)
        {
            this.TriggerEvent(EventName.PlaySFX, new SFXEventArgs { sfxType = SoundEffectType.GameOver }); // 结算音效
            gameOverPanel.SetActive(true);
            if (winner != null)
            {
                winnerText.text = $"游戏结束！\n最终大赢家是：【{winner.playerName}】";
            }
            else
            {
                winnerText.text = "游戏结束！\n平局或全部破产！";
            }
        }
        else
        {
            Debug.LogError("未绑定 GameOverPanel");
        }
    }
}
