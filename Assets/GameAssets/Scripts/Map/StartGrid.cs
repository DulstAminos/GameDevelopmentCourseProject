using UnityEngine;

public class StartGrid : GridController
{
    [Header("经过奖励")]
    public int reward = 30;
    public override void OnPassed(PlayerController player)
    {
        this.TriggerEvent(EventName.PlaySFX, new SFXEventArgs { sfxType = SoundEffectType.PassStart }); // 奖励音效
        player.SetMoney(player.GetMoney() + reward);
        Debug.Log($"【起点格】{player.playerName} 经过起点，获得 {reward} 金钱！");
    }
}
