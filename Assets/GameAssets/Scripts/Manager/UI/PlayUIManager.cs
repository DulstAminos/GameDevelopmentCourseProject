using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游玩UI管理器
/// </summary>
public class PlayUIManager : MonoBehaviour
{
    public static PlayUIManager Instance { get; private set; }

    [Header("顶栏 UI")]
    public TextMeshProUGUI roundInfoText; // 显示当前回合

    [Header("左侧玩家列表 UI")]
    public List<TextMeshProUGUI> playerInfoTexts;

    [Header("底部操作栏 UI")]
    public GameObject OperationPanel; // 操作面板
    public Button buildBtn;    // 建造
    public Button upgradeBtn;  // 升级
    public Button consumeBtn;  // 消费(吃饭)
    public Button endActionBtn;// 结束

    [Header("右下角 UI")]
    public Button diceBtn;     // 掷骰子

    private PlayerController activePlayer; // 当前正在操作的玩家
    private RestaurantGrid activeGrid;     // 玩家当前脚下的格子是否是饭店格
    private bool hasBuiltOrUpgradedThisTurn = false; // 当前回合是否已建造或升级

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 监听底层数据变动事件
        EventManager.Instance.AddListener(EventName.OnMoneyChanged, RefreshPlayerUI);
        EventManager.Instance.AddListener(EventName.OnStaminaChanged, RefreshPlayerUI);
        EventManager.Instance.AddListener(EventName.OnPlayerStateChanged, RefreshPlayerUI);
        EventManager.Instance.AddListener(EventName.OnRoundOrTurnChanged, RefreshRoundUI);

        // 默认禁用所有交互，等待流程控制器TurnManager激活
        SetOperationActive(false);
        diceBtn.interactable = false;
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener(EventName.OnMoneyChanged, RefreshPlayerUI);
            EventManager.Instance.RemoveListener(EventName.OnStaminaChanged, RefreshPlayerUI);
            EventManager.Instance.RemoveListener(EventName.OnPlayerStateChanged, RefreshPlayerUI);
            EventManager.Instance.RemoveListener(EventName.OnRoundOrTurnChanged, RefreshRoundUI);
        }
    }

    private void Start()
    {
        // 绑定底部四个操作按钮
        buildBtn.onClick.AddListener(OnBuildBtnClicked);
        upgradeBtn.onClick.AddListener(OnUpgradeBtnClicked);
        consumeBtn.onClick.AddListener(OnConsumeBtnClicked);
        endActionBtn.onClick.AddListener(OnEndActionBtnClicked);
    }

    // 刷新回合显示UI
    private void RefreshRoundUI(object sender, System.EventArgs e)
    {
        TurnManager tm = FindObjectOfType<TurnManager>();
        if (tm != null)
        {
            roundInfoText.text = $"{tm.CurrentRound} / 40";
        }
        RefreshPlayerUI(null, null); // 切换回合时，也要刷新玩家信息
    }

    // 刷新左侧所有玩家信息
    private void RefreshPlayerUI(object sender, System.EventArgs e)
    {
        // 通过 TurnManager 获取玩家数据
        TurnManager tm = FindObjectOfType<TurnManager>();
        if (tm == null || tm.players == null) return;

        for (int i = 0; i < tm.players.Count; i++)
        {
            if (i >= playerInfoTexts.Count) break;

            PlayerController p = tm.players[i];
            string typeStr = p.isAI ? "AI" : "你";
            string stateStr = GetStateString(p);

            playerInfoTexts[i].text = $"[{i + 1}] {typeStr} {p.playerName}\n金钱:{p.GetMoney()} 体力:{p.GetStamina()}/{p.maxStamina}\n 状态: {stateStr}";

            // 可以给当前回合的玩家文字加粗或换颜色标识
            if (i == tm.CurrentPlayerIndex)
            {
                playerInfoTexts[i].text = "<color=#FFFF00>=> " + playerInfoTexts[i].text + "</color>";
            }
        }
    }

    // 辅助函数：获取玩家状态
    private string GetStateString(PlayerController p)
    {
        switch (p.GetState())
        {
            case PlayerState.Normal: return "正常";
            case PlayerState.Mining: return $"挖煤({p.GetMiningTurnsLeft()}回合)";
            case PlayerState.Dead: return "<color=red>死亡</color>";
            default: return "未知";
        }
    }

    /// <summary>
    /// 开关操作面板的显示/隐藏状态
    /// </summary>
    public void SetOperationActive(bool isActive)
    {
        OperationPanel.gameObject.SetActive(isActive);
    }

    /// <summary>
    /// 开放操作权限
    /// </summary>
    /// <param name="player">当前玩家</param>
    /// <param name="currentGrid">当前格</param>
    public void EnablePlayerActions(PlayerController player, GridController currentGrid)
    {
        activePlayer = player;
        activeGrid = currentGrid as RestaurantGrid; // 尝试转型为饭店格

        hasBuiltOrUpgradedThisTurn = false; // 新回合开始，重置建造限制标记

        SetOperationActive(true);

        RefreshActionButtons();
    }

    // 统一评估四个按钮当前是否可点击，每次操作后都应调用
    private void RefreshActionButtons()
    {
        buildBtn.interactable = false;
        upgradeBtn.interactable = false;
        consumeBtn.interactable = false;
        endActionBtn.interactable = true; // 结束按钮始终可用

        if (activeGrid != null)
        {
            // 1. 建造判断
            if (activeGrid.level == 0 && !hasBuiltOrUpgradedThisTurn)
            {
                buildBtn.interactable = activePlayer.GetMoney() >= activePlayer.GetActualCost(activeGrid.buildCost);
            }
            // 2. 升级判断 (只能升自己的)
            else if (activeGrid.owner == activePlayer && activeGrid.level > 0 && activeGrid.level < 3 && !hasBuiltOrUpgradedThisTurn)
            {
                int upCost = activeGrid.upgradeCosts[activeGrid.level - 1];
                upgradeBtn.interactable = activePlayer.GetMoney() >= activePlayer.GetActualCost(upCost);
            }

            // 3. 消费判断 (所有饭店都能吃)
            if (activeGrid.level > 0 && activePlayer.GetStamina() < activePlayer.maxStamina)
            {
                int baseCost = activeGrid.consumeCosts[activeGrid.level - 1];
                int finalCost = activeGrid.owner == activePlayer ? Mathf.FloorToInt(baseCost * activeGrid.ownerDiscount) : baseCost;

                consumeBtn.interactable = activePlayer.GetMoney() >= activePlayer.GetActualCost(finalCost);
            }
        }
    }

    #region 按钮点击响应逻辑
    private void OnBuildBtnClicked()
    {
        int cost = activePlayer.GetActualCost(activeGrid.buildCost);
        PopupManager.Instance.ShowPopup("建造饭店", $"是否花费 {cost} 金币买下这块地，并建造 1 级 {activeGrid.restaurantName} ？",
            onConfirm: () =>
            {
                if (activePlayer.SpendMoney(cost))
                {
                    activeGrid.UpgradeOrBuild(activePlayer);
                    hasBuiltOrUpgradedThisTurn = true; // 标记本回合已建造
                    RefreshActionButtons(); // 刷新按钮状态
                }
            },
            onCancel: () => { }
        );
    }

    private void OnUpgradeBtnClicked()
    {
        int cost = activePlayer.GetActualCost(activeGrid.upgradeCosts[activeGrid.level - 1]);
        PopupManager.Instance.ShowPopup("升级饭店", $"是否花费 {cost} 金币将 {activeGrid.restaurantName} 升至 {activeGrid.level + 1} 级？",
            onConfirm: () =>
            {
                if (activePlayer.SpendMoney(cost))
                {
                    activeGrid.UpgradeOrBuild(activePlayer);
                    hasBuiltOrUpgradedThisTurn = true; // 标记本回合已升级
                    RefreshActionButtons();
                }
            },
            onCancel: () => { }
        );
    }

    private void OnConsumeBtnClicked()
    {
        bool isMyRestaurant = (activeGrid.owner == activePlayer);
        int baseCost = activeGrid.consumeCosts[activeGrid.level - 1];
        int finalCost = activePlayer.GetActualCost(isMyRestaurant ? Mathf.FloorToInt(baseCost * activeGrid.ownerDiscount) : baseCost);
        int staminaRecover = activeGrid.staminaRecovers[activeGrid.level - 1];

        string ownerStr = activeGrid.owner == null ? "中立的" : (isMyRestaurant ? "你自己的" : $"{activeGrid.owner.playerName} 的");

        string contentMsg = $"是否在 {ownerStr} {activeGrid.restaurantName} 花费 {finalCost} 金币购买 {activeGrid.foodName} ？\n(恢复 {staminaRecover} 点体力)";

        if (isMyRestaurant)
        {
            contentMsg += $"\n<color=#32CD32> 房主特权：享受 {activeGrid.ownerDiscount * 10} 折优惠！(原价 {baseCost})</color>";
        }

        PopupManager.Instance.ShowPopup("购买食物", contentMsg,
            onConfirm: () =>
            {
                if (activePlayer.SpendMoney(finalCost))
                {
                    if (!isMyRestaurant) activeGrid.OwnerMakeMoney(finalCost);
                    activePlayer.RecoverStamina(staminaRecover);
                    RefreshActionButtons();
                }
            },
            onCancel: () => { }
        );
    }

    private void OnEndActionBtnClicked()
    {
        // 直接结束该阶段
        SetOperationActive(false);
        activePlayer.FinishGridAction();
    }
    #endregion
}
