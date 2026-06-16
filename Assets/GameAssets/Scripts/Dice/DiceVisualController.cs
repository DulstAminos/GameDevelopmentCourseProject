using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 骰子动画控制器
/// </summary>
public class DiceVisualController : MonoBehaviour
{
    [Header("模型引用")]
    public GameObject diceModel; // 骰子的模型子物体

    [Header("显示配置")]
    public float distanceFromCamera = 5f; // 距离摄像机多远
    public Vector3 offset = Vector3.zero; // 屏幕中心偏移量

    [Header("动画时间配置")]
    public float randomSpinDuration = 1.0f; // 随机旋转的时间
    public float settleDuration = 0.5f;     // 停靠到目标角度的时间
    public float showResultDuration = 0.8f; // 结果展示停留时间
    public Vector3 randomSpinSpeed = new Vector3(1080, 720, 1440); // 乱转的速度

    [Serializable]
    public struct DiceFaceMarker
    {
        public int number;
        public Transform markerTransform; // 该面对应的空物体锚点
    }

    [Header("点数锚点配置")]
    public List<DiceFaceMarker> faceMarkers;

    private Dictionary<int, Transform> markerDict = new Dictionary<int, Transform>();

    private void Awake()
    {
        // 初始化字典方便查找
        foreach (var face in faceMarkers)
        {
            if (face.markerTransform != null)
                markerDict[face.number] = face.markerTransform;
        }

        // 初始隐藏模型
        diceModel.SetActive(false);
    }

    private void OnEnable()
    {
        EventManager.Instance.AddListener(EventName.ShowDiceAnimation, OnShowDiceAnimation);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener(EventName.ShowDiceAnimation, OnShowDiceAnimation);
    }

    private void OnShowDiceAnimation(object sender, EventArgs e)
    {
        var args = e as DiceAnimationEventArgs;
        var turnManager = sender as TurnManager;

        if (args != null && turnManager != null)
        {
            StartCoroutine(AnimateDiceCoroutine(args.result, turnManager));
        }
    }

    private IEnumerator AnimateDiceCoroutine(int targetNumber, TurnManager turnManager)
    {
        diceModel.SetActive(true);

        Transform camTransform = Camera.main.transform;

        // 阶段一：随机旋转
        float timer = 0f;
        while (timer < randomSpinDuration)
        {
            // 时刻跟随摄像机前方
            UpdatePositionToCamera(camTransform);

            // 疯狂自转
            transform.Rotate(randomSpinSpeed * Time.deltaTime, Space.Self);

            timer += Time.deltaTime;
            yield return null;
        }

        // 阶段二：计算目标旋转并平滑过渡
        // 1. 获取目标面对应的锚点
        Transform targetMarker = markerDict.ContainsKey(targetNumber) ? markerDict[targetNumber] : null;

        if (targetMarker != null)
        {
            Quaternion startRot = transform.rotation;

            // 2. 计算当前面向量 (从骰子中心指向锚点)
            Vector3 currentFaceDir = (targetMarker.position - transform.position).normalized;

            // 3. 计算目标向量 (指向摄像机的反方向，即摄像机正对面)
            Vector3 targetDir = -camTransform.forward;

            // 4. 计算这两个向量之间的旋转差
            Quaternion rotationDelta = Quaternion.FromToRotation(currentFaceDir, targetDir);

            // 5. 最终目标旋转 = 旋转差 * 当前旋转
            Quaternion targetRot = rotationDelta * startRot;

            timer = 0f;
            while (timer < settleDuration)
            {
                UpdatePositionToCamera(camTransform);
                timer += Time.deltaTime;

                // 缓动函数
                float t = timer / settleDuration;
                t = Mathf.Sin(t * Mathf.PI * 0.5f);

                transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }

            // 确保最后绝对对齐
            transform.rotation = targetRot;
        }
        else
        {
            Debug.LogError($"未配置点数 {targetNumber} 的锚点！");
        }

        // 【阶段三：展示结果并清理】
        yield return new WaitForSeconds(showResultDuration);

        diceModel.SetActive(false);

        // 通知 TurnManager 解除卡点，继续下一步
        turnManager.SetDiceRolled();
    }

    // 辅助方法：让骰子始终悬浮在摄像机正前方
    private void UpdatePositionToCamera(Transform camTransform)
    {
        transform.position = camTransform.position + camTransform.forward * distanceFromCamera + camTransform.TransformDirection(offset);
    }
}
