using UnityEngine;

public class RestaurantGrid : GridController
{
    [Header("饭店基础信息")]
    public string restaurantName = "餐馆"; // 饭店名称
    public string foodName = "米饭";        // 食物名称

    [Header("饭店归属与状态")]
    public PlayerController owner = null;
    public int level = 0;

    [Header("数值配置")]
    public int buildCost = 100; // 0级建1级花费
    public int[] upgradeCosts = new int[] { 200, 350, 0 }; // 升到2级, 升到3级
    public int[] tolls = new int[] { 20, 30, 50 };         // 停泊过路费
    public int[] consumeCosts = new int[] { 50, 80, 120 }; // 吃饭花费
    public int[] staminaRecovers = new int[] { 2, 3, 4 };  // 吃饭恢复体力
    [Range(0f, 1f)] public float ownerDiscount = 0.5f; // 默认5折

    [Header("格子视觉表现")]
    public MeshRenderer modelRenderer;    // 子物体的 MeshRenderer
    public Material emptyMaterial;        // 空白格材质
    public Material levelMaterial;     // 饭店格材质

    [Header("建筑、食物模型与点位配置")]
    public GameObject buildingPrefab;     // 饭店建筑模型/预制体
    public GameObject foodPrefab;         // 食物模型/预制体
    public Transform[] spawnPoints;       // 4个候选生成点位 (前后左右)
    [Tooltip("选择哪个点位生成建筑")]
    public int selectedSpawnIndex = 0;
    [Tooltip("食物模型在生成点位上方的高度偏移")]
    public float foodHeightOffset = 2.0f;

    // 内部引用的已实例化物体
    private GameObject spawnedBuilding;
    private GameObject spawnedFood;

    private void Awake()
    {
        // 实例化模型
        InstantiateModels();
    }

    private void Start()
    {
        // 开局初始化材质
        UpdateVisual();
    }

    /// <summary>
    /// 初始化时生成建筑和食物模型
    /// </summary>
    private void InstantiateModels()
    {
        if (spawnPoints == null || spawnPoints.Length == 0 || selectedSpawnIndex >= spawnPoints.Length)
        {
            Debug.LogWarning($"{gameObject.name} 的模型生成点位配置有误！");
            return;
        }

        Transform spawnPoint = spawnPoints[selectedSpawnIndex];

        // 实例化建筑
        if (buildingPrefab != null)
        {
            spawnedBuilding = Instantiate(buildingPrefab, spawnPoint.position, spawnPoint.rotation, this.transform);
            spawnedBuilding.SetActive(level > 0);
        }

        // 实例化食物
        if (foodPrefab != null)
        {
            Vector3 foodPos = spawnPoint.position + Vector3.up * foodHeightOffset;
            spawnedFood = Instantiate(foodPrefab, foodPos, spawnPoint.rotation, this.transform);
            spawnedFood.SetActive(level > 0);
        }
    }

    public override void OnArrived(PlayerController player)
    {
        if (level == 0)
        {
            Debug.Log($"【空地】{player.playerName} 停在了无主空地上。");
        }
        else if (owner == null)
        {
            // 中立饭店逻辑
            Debug.Log($"【中立饭店】{player.playerName} 停在了中立的 {level} 级饭店，可花钱消费。");
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
        // 获取该玩家的实际需付金额
        int actualToll = player.GetActualCost(toll);

        if (player.SpendMoney(actualToll))
        {
            // 正常交费
            OwnerMakeMoney(actualToll);
            PopupManager.Instance.ShowPopup("交纳过路费", $" {player.playerName} 到达了 {owner.playerName} 经营的 {restaurantName} ！\n支付了 {actualToll} 金币过路费。", null, null, "知道了");
        }
        else
        {
            // 钱不够，破产挖煤逻辑
            int allRemaining = player.GetMoney();
            OwnerMakeMoney(allRemaining);
            player.SetMoney(0);
            PopupManager.Instance.ShowPopup("破产警告", $"{player.playerName}到达了 {owner.playerName} 经营的 {restaurantName} ！\n但 {player.playerName} 钱不够交过路费（差 {actualToll - allRemaining}）。\n {player.playerName} 已破产，将被送去挖煤！", null, null, "去挖煤吧");
            player.EnterMiningState();
        }
    }

    // 供外部调用的升级/建造方法
    public void UpgradeOrBuild(PlayerController newOwner)
    {
        this.TriggerEvent(EventName.PlaySFX, new SFXEventArgs { sfxType = SoundEffectType.Upgrade }); // 升级音效
        owner = newOwner;
        level++;
        UpdateVisual();
    }

    // 店主赚钱
    public void OwnerMakeMoney(int money)
    {
        if (owner == null) return;
        owner.SetMoney(owner.GetMoney() + money);
    }

    // 更新视觉表现
    private void UpdateVisual()
    {
        // 更新格子材质
        if (modelRenderer != null)
        {
            if (level == 0)
                modelRenderer.material = emptyMaterial;
            else
                modelRenderer.material = levelMaterial;
        }

        // 更新建筑和食物模型的显隐
        bool shouldShowModels = (level > 0);

        if (spawnedBuilding != null)
        {
            spawnedBuilding.SetActive(shouldShowModels);
        }

        if (spawnedFood != null)
        {
            spawnedFood.SetActive(shouldShowModels);
        }
    }
}
