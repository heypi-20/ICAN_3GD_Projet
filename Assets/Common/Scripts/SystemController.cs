using System;
using System.Diagnostics;
using UnityEngine;

public class SystemController : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            //ShutdownWindows();
        }
    }

    public void ShutdownWindows()
    {
#if UNITY_STANDALONE_WIN
        Process.Start("shutdown", "/s /t 0");
#endif
    }
}