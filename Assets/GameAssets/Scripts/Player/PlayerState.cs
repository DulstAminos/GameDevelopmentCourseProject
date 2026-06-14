/// <summary>
/// 玩家状态枚举
/// </summary>
public enum PlayerState
{
    Normal,     // 正常状态
    Mining,     // 挖煤状态
    Dead        // 死亡淘汰
}

public enum CharacterRole
{
    Boy,        // 年轻男孩 (初始/最大体力更高)
    Girl,       // 年轻女孩 (花费打折)
    FatMan,     // 大胃袋 (额外恢复)
    OldLady     // 老奶奶 (挖煤免疫)
}
