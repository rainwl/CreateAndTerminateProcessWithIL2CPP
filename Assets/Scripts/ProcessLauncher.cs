using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Debug = UnityEngine.Debug;

public class ProcessLauncher : MonoBehaviour
{
    #region DLL Import

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CreateProcessW(
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        [In] ref StartUpInfo lpStartupInfo,
        out ProcessInformation lpProcessInformation
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

    #endregion

    #region Fields

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct StartUpInfo
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    private ProcessInformation _processInfo1;
    private ProcessInformation _processInfo2;

    private List<ProcessInformation> _processList = new List<ProcessInformation>();

    #endregion

    #region Process Methods

    private ProcessInformation StartExternalProcess(string programPath, string arguments)
    {
        // const string programPath = @"E:\FleX-1.2.0\NvFlexDemoReleaseCUDA_x64.exe";
        // const string arguments = "-surgeryType=0";

        var startupInfo = new StartUpInfo();
        startupInfo.cb = Marshal.SizeOf(startupInfo);

        var success = CreateProcessW(
            programPath,
            arguments,
            IntPtr.Zero,
            IntPtr.Zero,
            false,
            0,
            IntPtr.Zero,
            null,
            ref startupInfo,
            out var processInfo
        );

        if (success)
        {
            Debug.Log("Process started, process ID: " + processInfo.dwProcessId);
            _processList.Add(processInfo);
            return processInfo;
        }
        else
        {
            Debug.LogError("Unable to start process, error code: " + Marshal.GetLastWin32Error());
            return default;
        }
    }

    private static void TerminateExternalProcess(ProcessInformation processInfo)
    {
        // ReSharper disable once InvertIf  
        if (processInfo.hProcess != IntPtr.Zero)
        {
            var success = TerminateProcess(processInfo.hProcess, 0);
            if (success)
            {
                Debug.Log("The external process is shut down");
            }
            else
            {
                Debug.LogError("Unable to shut down external process, error code:" + Marshal.GetLastWin32Error());
            }
        }
    }

    #endregion

    #region Test Methods

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _processInfo1 =
                StartExternalProcess(@"D:\Projects\Log[]\LogInsights\src\LogInsights\bin\x64\Release\LogInsights.exe",
                    null);
            _processInfo2 = StartExternalProcess(@"C:\Windows\System32\cmd.exe", null);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            TerminateExternalProcess(_processInfo1);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            TerminateExternalProcess(_processInfo2);
        }
    }

    private void OnDestroy()
    {
        foreach (var processInfo in _processList)
        {
            TerminateProcess(processInfo.hProcess, 0);
            Debug.Log("The external process is shut down,error code: " + processInfo.dwProcessId);
        }
    }

    #endregion
}