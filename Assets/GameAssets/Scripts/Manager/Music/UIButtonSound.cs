using UnityEngine;
using UnityEngine.UI;

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
