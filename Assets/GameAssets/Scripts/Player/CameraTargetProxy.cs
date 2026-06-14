using System;
using UnityEngine;

public class CameraTargetProxy : MonoBehaviour
{
    [Header("高度配置")]
    [Tooltip("代理物体所在的固定高度（世界坐标Y值）")]
    public float fixedY = 0f;

    private Transform currentTarget;

    private void OnEnable()
    {
        EventManager.Instance.AddListener(EventName.OnCameraChangeTarget, OnChangeTarget);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener(EventName.OnCameraChangeTarget, OnChangeTarget);
    }

    private void OnChangeTarget(object sender, EventArgs e)
    {
        if (e is CameraTargetEventArgs args && args.targetTransform != null)
        {
            currentTarget = args.targetTransform;
        }
    }

    private void LateUpdate()
    {
        if (currentTarget != null)
        {
            // 核心逻辑：X和Z跟随玩家，Y始终保持 fixedY
            transform.position = new Vector3(currentTarget.position.x, fixedY, currentTarget.position.z);
        }
    }
}
