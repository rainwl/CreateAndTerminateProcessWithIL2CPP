using System.Diagnostics;
using UnityEngine;

public class Main : MonoBehaviour
{
    private void Awake()
    {
        //StartProcess("E:\\FleX-1.2.0\\", "NvFlexDemoReleaseCUDA_x64.exe", "-surgeryType=0");
        //StartExternalProcess.Start("E:\\FleX-1.2.0\\NvFlexDemoReleaseCUDA_x64.exe","-surgeryType=0" );
        InternalServerProcess.Start();
    }

    // Conflict with IL2CPP
    private static void StartProcess(string path, string applicationPath, string arguments = "")
    {
        var process = new Process();
        process.StartInfo.WorkingDirectory = path;
        process.StartInfo.FileName = applicationPath;
        if (string.IsNullOrEmpty(arguments)) return;
        process.StartInfo.Arguments = arguments;
        process.Start();
    }
}