using System;
using System.Diagnostics;
using UnityEngine;

public class SystemController : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            ShutdownWindows();
        }
    }

    public void ShutdownWindows()
    {
#if UNITY_STANDALONE_WIN
        // /s 关机, /t 0 立即执行
        Process.Start("shutdown", "/s /t 0");
#endif
    }
}