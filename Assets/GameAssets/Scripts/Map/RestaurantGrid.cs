using UnityEngine;

public class RestaurantGrid : GridController
{
    [Header("饭店数据")]
    public PlayerController owner = null;
    public int level = 0;
    public int[] tolls = new int[3] { 20, 30, 50 };

    [Header("视觉表现")]
    public MeshRenderer modelRenderer;    // 子物体的 MeshRenderer
    public Material emptyMaterial;        // 空白格材质
    public Material levelMaterial;     // 饭店材质

    private void Start()
    {
        UpdateVisual(); // 开局初始化材质
    }

    public override void OnArrived(PlayerController player)
    {
        if (owner == null)
        {
            Debug.Log($"【空地】{player.playerName} 停在了无主空地上。");
            // 提示：你可以在这儿抛出事件触发随机事件，按照GDD我们放到以后做
        }
        else if (owner != player)
        {
            Debug.Log($"【别人饭店】{player.playerName} 停在了 {owner.playerName} 的 {level} 级饭店。");
            PayToll(player);
        }
        else
        {
            Debug.Log($"【自己饭店】{player.playerName} 停在了自己的 {level} 级饭店。");
        }
    }

    private void PayToll(PlayerController player)
    {
        if (level < 1 || level > 3) return;

        // 需要交的过路费
        int toll = tolls[level - 1];

        Debug.Log($" {player.playerName} 需要交纳过路费: {toll}");

        if (player.SpendMoney(toll))
        {
            // 正常交费
            owner.SetMoney(owner.GetMoney() + toll);
            // 弹窗通知被扣钱（不需要玩家确认，只要点知道了就行）
            PopupManager.Instance.ShowPopup("交纳过路费", $"{player.playerName}到达了 {owner.playerName} 的饭店！\n支付了 {toll} 金币过路费。", null, null, "知道了");
        }
        else
        {
            // 钱不够，破产挖煤逻辑
            int allRemaining = player.GetMoney();
            owner.SetMoney(owner.GetMoney() + allRemaining); // 把最后的钱给房主
            player.SetMoney(0);
            PopupManager.Instance.ShowPopup("破产警告", $"你踩中了 {owner.playerName} 的饭店！\n但你钱不够交过路费（差 {toll - allRemaining}）。\n你已破产，将被送去挖煤！", null, null, "去挖煤吧");
            player.EnterMiningState();
        }
    }

    // 供外部调用的升级/建造方法
    public void UpgradeOrBuild(PlayerController newOwner)
    {
        owner = newOwner;
        level++;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (modelRenderer == null) return;

        if (level == 0)
        {
            modelRenderer.material = emptyMaterial;
        }
        else
        {
            modelRenderer.material = levelMaterial;
        }
    }
}
