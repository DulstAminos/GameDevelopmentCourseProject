using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayUIManager : MonoBehaviour
{
    public static PlayUIManager Instance { get; private set; }

    [Header("顶栏 UI")]
    public TextMeshProUGUI roundInfoText; // 显示当前回合

    [Header("左侧玩家列表 UI")]
    public List<TextMeshProUGUI> playerInfoTexts;

    [Header("底部操作栏 UI")]
    public Button buildBtn;    // 建造
    public Button upgradeBtn;  // 升级
    public Button consumeBtn;  // 消费(吃饭)
    public Button endActionBtn;// 结束

    [Header("右下角 UI")]
    public Button diceBtn;     // 掷骰子

    private PlayerController activePlayer; // 当前正在操作的玩家
    private RestaurantGrid activeGrid;     // 玩家当前脚下的格子是否是饭店格

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 监听底层数据变动事件
        EventManager.Instance.AddListener(EventName.OnMoneyChanged, RefreshPlayerUI);
        EventManager.Instance.AddListener(EventName.OnStaminaChanged, RefreshPlayerUI);
        EventManager.Instance.AddListener(EventName.OnPlayerStateChanged, RefreshPlayerUI);
        EventManager.Instance.AddListener(EventName.OnRoundOrTurnChanged, RefreshRoundUI);

        // 默认禁用所有交互按钮，等待流程控制器TurnManager激活
        SetActionButtonsActive(false);
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

    /// <summary>
    /// 刷新左侧所有玩家信息
    /// </summary>
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

            // 组装格式：[1] 你 张三 | 💰: 200 | 🍗: 5 | 状态: 正常
            playerInfoTexts[i].text = $"[{i + 1}] {typeStr} {p.playerName} | 金钱:{p.GetMoney()} | 体力:{p.GetStamina()} | 状态: {stateStr}";

            // 可以给当前回合的玩家文字加粗或换颜色标识
            if (i == tm.CurrentPlayerIndex)
            {
                playerInfoTexts[i].text = "<color=#FFFF00>▶ " + playerInfoTexts[i].text + "</color>";
            }
        }
    }

    private void RefreshRoundUI(object sender, System.EventArgs e)
    {
        TurnManager tm = FindObjectOfType<TurnManager>();
        if (tm != null)
        {
            roundInfoText.text = $"当前回合:\n{tm.CurrentRound} / 40";
        }
        RefreshPlayerUI(null, null); // 切换回合时，也要刷新玩家信息
    }

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
    /// 开关底部4个操作按钮的显示/隐藏状态
    /// </summary>
    public void SetActionButtonsActive(bool isActive)
    {
        buildBtn.gameObject.SetActive(isActive);
        upgradeBtn.gameObject.SetActive(isActive);
        consumeBtn.gameObject.SetActive(isActive);
        endActionBtn.gameObject.SetActive(isActive);
    }

    /// <summary>
    /// TurnManager会在第四步调用这个方法，开放操作权限
    /// </summary>
    public void EnablePlayerActions(PlayerController player, GridController currentGrid)
    {
        activePlayer = player;
        activeGrid = currentGrid as RestaurantGrid; // 尝试转型为饭店格

        SetActionButtonsActive(true);

        // 重置所有按钮可点击状态，后续根据逻辑判断是否禁用
        buildBtn.interactable = false;
        upgradeBtn.interactable = false;
        consumeBtn.interactable = false;
        endActionBtn.interactable = true; // 结束按钮永远可用

        if (activeGrid != null)
        {
            if (activeGrid.level == 0)
            {
                // 空地：可以建造 (造价写死100)
                buildBtn.interactable = activePlayer.GetMoney() >= 100;
            }
            else if (activeGrid.owner == activePlayer)
            {
                // 自己饭店：可以升级或消费
                // (根据GDD，1级升2级需200，2级升3级需350，吃饭费用暂定为对应等级的过路费+30)
                int upgradeCost = activeGrid.level == 1 ? 200 : (activeGrid.level == 2 ? 350 : 9999);
                if (activeGrid.level < 3) upgradeBtn.interactable = activePlayer.GetMoney() >= upgradeCost;

                int consumeCost = activeGrid.level == 1 ? 50 : (activeGrid.level == 2 ? 80 : 120);
                consumeBtn.interactable = activePlayer.GetMoney() >= consumeCost && activePlayer.GetStamina() < activePlayer.maxStamina;
            }
        }
    }

    #region 按钮点击响应逻辑
    private void OnBuildBtnClicked()
    {
        int cost = 100;
        // 弹出二次确认框
        PopupManager.Instance.ShowPopup("建造饭店", $"是否花费 {cost} 金币在此空地建造 1 级饭店？",
            onConfirm: () =>
            {
                if (activePlayer.SpendMoney(cost))
                {
                    activeGrid.UpgradeOrBuild(activePlayer);
                    Debug.Log("建造成功！");
                    // 操作完一次后，禁用该按钮防止连点，或直接强制结束回合操作
                    SetActionButtonsActive(false);
                    activePlayer.FinishGridAction(); // 关键：释放协程卡点！
                }
            },
            onCancel: () => { /* 取消不做任何事 */ }
        );
    }

    private void OnUpgradeBtnClicked()
    {
        int cost = activeGrid.level == 1 ? 200 : 350;
        PopupManager.Instance.ShowPopup("升级饭店", $"是否花费 {cost} 金币将饭店升至 {activeGrid.level + 1} 级？",
            onConfirm: () =>
            {
                if (activePlayer.SpendMoney(cost))
                {
                    activeGrid.UpgradeOrBuild(activePlayer);
                    Debug.Log("升级成功！");
                    SetActionButtonsActive(false);
                    activePlayer.FinishGridAction();
                }
            },
            onCancel: () => { }
        );
    }

    private void OnConsumeBtnClicked()
    {
        int cost = activeGrid.level == 1 ? 50 : (activeGrid.level == 2 ? 80 : 120);
        int staminaRecover = activeGrid.level == 1 ? 2 : (activeGrid.level == 2 ? 3 : 4);

        PopupManager.Instance.ShowPopup("吃大餐", $"是否花费 {cost} 金币吃顿好的？\n将恢复 {staminaRecover} 点体力。",
            onConfirm: () =>
            {
                if (activePlayer.SpendMoney(cost))
                {
                    activePlayer.SetStamina(activePlayer.GetStamina() + staminaRecover);
                    Debug.Log("消费成功，体力恢复！");
                    SetActionButtonsActive(false);
                    activePlayer.FinishGridAction();
                }
            },
            onCancel: () => { }
        );
    }

    private void OnEndActionBtnClicked()
    {
        // 直接结束该阶段
        SetActionButtonsActive(false);
        activePlayer.FinishGridAction();
    }
    #endregion
}
