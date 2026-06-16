using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI弹窗管理器
/// </summary>
public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [Header("UI 引用")]
    public GameObject popupPanel; // 弹窗父节点
    public TextMeshProUGUI titleText;        // 标题文本
    public TextMeshProUGUI contentText;      // 内容文本
    public Button confirmBtn;     // 确认按钮
    public Button cancelBtn;      // 取消按钮
    public TextMeshProUGUI confirmBtnText;   // 确认按钮上的文本
    public TextMeshProUGUI cancelBtnText;    // 取消按钮上的文本

    private Action onConfirmAction;
    private Action onCancelAction;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 绑定按钮事件
        confirmBtn.onClick.AddListener(OnConfirmClicked);
        cancelBtn.onClick.AddListener(OnCancelClicked);

        // 默认隐藏
        popupPanel.SetActive(false);
    }

    /// <summary>
    /// 显示弹窗 (支持仅一个按钮或两个按钮)
    /// </summary>
    public void ShowPopup(string title, string content, Action onConfirm, Action onCancel = null, string confirmStr = "确认", string cancelStr = "取消")
    {
        titleText.text = title;
        contentText.text = content;

        onConfirmAction = onConfirm;
        onCancelAction = onCancel;

        confirmBtnText.text = confirmStr;

        if (onCancel != null)
        {
            cancelBtn.gameObject.SetActive(true);
            cancelBtnText.text = cancelStr;
        }
        else
        {
            // 如果没有取消回调，说明是通知类弹窗，隐藏取消按钮
            cancelBtn.gameObject.SetActive(false);
        }

        popupPanel.SetActive(true);
    }

    private void OnConfirmClicked()
    {
        popupPanel.SetActive(false);
        onConfirmAction?.Invoke();
    }

    private void OnCancelClicked()
    {
        popupPanel.SetActive(false);
        onCancelAction?.Invoke();
    }
}
