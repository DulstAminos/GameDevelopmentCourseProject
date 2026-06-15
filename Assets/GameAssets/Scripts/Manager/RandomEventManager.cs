using System;
using System.Collections.Generic;
using UnityEngine;

public enum RandomEventType
{
    None,           // 0.无事发生
    Treasure,       // 1.天降宝藏 (+100金)
    Thief,          // 2.遇到小偷 (-50金)
    Resident,       // 3.热心居民 (+1体力)
    Fall,           // 4.掉落山崖 (-2体力)
    Overtime,       // 5.临时加班 (+50金, -1体)
    BrokenLeg,      // 6.摔断腿 (跳过下回合)
    Disaster,       // 7.天降横祸 (传送到最近煤矿)
    Windfall        // 8.大风刮来 (抢夺其他人各20金)
}

[Serializable]
public class EventCardWeight
{
    public RandomEventType eventType;
    public int countInDeck; // 在牌库中的数量（控制概率）
}

public class RandomEventManager : MonoBehaviour
{
    public static RandomEventManager Instance { get; private set; }

    [Header("好坏事件划分")]
    public List<RandomEventType> GoodEvents;
    public List<RandomEventType> BadEvents;

    [Header("随机事件牌库配置")]
    public List<EventCardWeight> deckConfig;
    private List<RandomEventType> drawPool = new List<RandomEventType>(); // 实际用来抽卡的牌堆

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 初始化牌库
        foreach (var config in deckConfig)
        {
            for (int i = 0; i < config.countInDeck; i++)
            {
                drawPool.Add(config.eventType);
            }
        }
    }

    /// <summary>
    /// 触发随机事件
    /// </summary>
    public void TriggerRandomEvent(PlayerController player, Action onComplete)
    {
        if (drawPool.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // 随机抽一张
        int randIndex = UnityEngine.Random.Range(0, drawPool.Count);
        RandomEventType drawnEvent = drawPool[randIndex];

        // 触发事件音效
        if (GoodEvents.Contains(drawnEvent))
        {
            this.TriggerEvent(EventName.PlaySFX, new SFXEventArgs { sfxType = SoundEffectType.GoodEvent });
        }
        else if (BadEvents.Contains(drawnEvent))
        {
            this.TriggerEvent(EventName.PlaySFX, new SFXEventArgs { sfxType = SoundEffectType.BadEvent });
        }

        ExecuteEvent(player, drawnEvent, onComplete);
    }

    private void ExecuteEvent(PlayerController player, RandomEventType type, Action onComplete)
    {
        string title = "随机事件";
        string content = "";

        switch (type)
        {
            case RandomEventType.None:
                content = "一阵微风吹过，什么事都没有发生。";
                PopupManager.Instance.ShowPopup(title, content, onComplete, null, "何意味");
                break;

            case RandomEventType.Treasure:
                player.SetMoney(player.GetMoney() + 100);
                content = $"{player.playerName} 在路边捡到了一个钱包！\n立刻获得 100 金币！";
                PopupManager.Instance.ShowPopup(title, content, onComplete, null, "这么强");
                break;

            case RandomEventType.Thief:
                int lost = Mathf.Min(player.GetMoney(), 50); // 最多扣到0
                player.SetMoney(player.GetMoney() - lost);
                content = $"{player.playerName} 遇到了一群强盗！\n失去了 {lost} 金币。";
                PopupManager.Instance.ShowPopup(title, content, onComplete, null, "太倒霉了");
                break;

            case RandomEventType.Resident:
                player.RecoverStamina(1);
                content = $"热心大妈给了 {player.playerName} 一个包子！\n恢复 1 点体力。";
                PopupManager.Instance.ShowPopup(title, content, onComplete, null, "不赖");
                break;

            case RandomEventType.Fall:
                player.SetStamina(player.GetStamina() - 2);
                content = $"{player.playerName} 不小心掉进了下水道！\n失去 2 点体力。";
                PopupManager.Instance.ShowPopup(title, content, () =>
                {
                    if (player.GetStamina() <= 0) player.Die();
                    onComplete?.Invoke();
                }, null, "惨");
                break;

            case RandomEventType.Overtime:
                player.SetMoney(player.GetMoney() + 50);
                player.SetStamina(player.GetStamina() - 1);
                content = $"老板强制 {player.playerName} 回去加了会儿班！\n获得 50 金币，但失去 1 点体力。";
                PopupManager.Instance.ShowPopup(title, content, () =>
                {
                    if (player.GetStamina() <= 0) player.Die();
                    onComplete?.Invoke();
                }, null, "Go Work!");
                break;

            case RandomEventType.BrokenLeg:
                player.skipNextTurn = true;
                content = $"{player.playerName} 走路玩手机撞树上了！\n强制跳过下一个回合。";
                PopupManager.Instance.ShowPopup(title, content, onComplete, null, "那很坏了");
                break;

            case RandomEventType.Windfall:
                int totalStolen = 0;
                TurnManager tm = FindObjectOfType<TurnManager>();
                foreach (var p in tm.players)
                {
                    if (p != player && p.GetState() != PlayerState.Dead)
                    {
                        int stealAmount = Mathf.Min(p.GetMoney(), 20);
                        p.SetMoney(p.GetMoney() - stealAmount);
                        totalStolen += stealAmount;
                    }
                }
                player.SetMoney(player.GetMoney() + totalStolen);
                content = $"一阵大风刮来了别人的私房钱！\n其他人各损失最多 20 金币，{player.playerName} 总共获得了 {totalStolen} 金币！";
                PopupManager.Instance.ShowPopup(title, content, onComplete, null, "开了？");
                break;

            case RandomEventType.Disaster:
                content = $"一辆失控的泥头车把 {player.playerName} 撞飞到了最近的煤矿！";
                PopupManager.Instance.ShowPopup(title, content, () =>
                {
                    // 传送到最近煤矿的逻辑
                    int nearestCoal = FindNearestCoalIndex(player.GetCurrentGridIndex());
                    player.TeleportToGrid(nearestCoal);
                    onComplete?.Invoke();
                }, null, "撞大运了");
                break;
        }
    }

    private int FindNearestCoalIndex(int currentIndex)
    {
        // 简易遍历查找最近的 CoalGrid
        int minDistance = 999;
        int targetIndex = currentIndex;
        MapManager map = MapManager.Instance;

        for (int i = 0; i < map.gridList.Count; i++)
        {
            if (map.gridList[i] is CoalGrid)
            {
                // 计算正反向距离取最小
                int dist = Mathf.Abs(i - currentIndex);
                dist = Mathf.Min(dist, map.gridList.Count - dist);

                if (dist < minDistance)
                {
                    minDistance = dist;
                    targetIndex = i;
                }
            }
        }
        return targetIndex;
    }
}
