using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 按钮点击音效脚本
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            // 按钮点击时触发音效事件
            this.TriggerEvent(EventName.PlaySFX, new SFXEventArgs { sfxType = SoundEffectType.ButtonClick });
        });
    }
}
