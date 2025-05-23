using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FireEventButton : MonoBehaviour
{
    public S_GameEvent gameEvent;
    public string arg;
    void Awake()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (arg != null)
                S_GameFlowController.Instance.FireEvent(gameEvent, arg);
            else
                S_GameFlowController.Instance.FireEvent(gameEvent);
        });
    }
}