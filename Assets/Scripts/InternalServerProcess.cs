using System.IO;
using UnityEngine;

internal static class InternalServerProcess
{
    private static uint _ptr = 0;

    public static void Start()
    {
        //const string processPath = @"E:\FleX-1.2.0\NvFlexDemoReleaseCUDA_x64.exe";
        const string processPath = @"C:\Windows\System32\cmd.exe";
        Debug.Log($"ProcessPath:{processPath}");

        if (File.Exists(processPath))
        {
            Debug.Log($"File Exists");
            //var args = $"-surgeryType=0";
            string args = null;
            Debug.Log($"Args:{args}");
            if (_ptr != 0)
            {
                Debug.Log("Internal server process already exists");
            }
            else
            {
                _ptr = StartExternalProcess.Start(processPath, args);
                Debug.Log($"pid:{_ptr}");
            }
        }
        else
        {
            Debug.Log($"File doesnt exist");
        }
    }

    public static void Stop()
    {
        Debug.Log($"killing if running pid:{_ptr}");

        if (_ptr == 0) return;
        Debug.Log($"killing");
        StartExternalProcess.KillProcess(_ptr);
    }
}