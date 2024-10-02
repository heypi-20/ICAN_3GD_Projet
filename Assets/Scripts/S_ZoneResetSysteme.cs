using System;
using UnityEngine;
using TMPro;  // 引入TextMeshPro命名空间

public class S_ZoneResetSysteme : MonoBehaviour
{
    // 定义一个事件，供其他物体订阅
    public static event Action OnZoneReset;
    // TextMeshPro UI 组件，用于显示重置次数
    public TextMeshProUGUI resetCountText;
    // 可以自行设定的按键，用于触发区域重置
    [SerializeField] private KeyCode resetKey = KeyCode.R;

    // 重置事件触发计数
    private int resetCount = 0;

    

    void Update()
    {
        // 当按下设定的键时，触发重置事件
        if (Input.GetKeyDown(resetKey))
        {
            TriggerZoneReset();
        }
    }

    // 触发区域重置事件的方法
    private void TriggerZoneReset()
    {
        Debug.Log("Zone reset triggered!");

        // 增加重置计数
        resetCount++;

        // 更新 UI 文本显示
        UpdateResetCountText();

        // 触发事件，如果有订阅者
        OnZoneReset?.Invoke();
    }

    // 更新 TextMeshPro 文本的方法
    private void UpdateResetCountText()
    {
        if (resetCountText != null)
        {
            resetCountText.text = $"Zone Reset Count: {resetCount}";
        }
        else
        {
            Debug.LogWarning("TextMeshProUGUI component is not assigned.");
        }
    }
}
