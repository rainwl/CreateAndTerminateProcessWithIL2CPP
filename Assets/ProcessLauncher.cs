using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Debug = UnityEngine.Debug;

// ReSharper disable InconsistentNaming

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

    [DllImport("kernel32.dll")]
    // ReSharper disable once IdentifierTypo
    public static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

    [DllImport("kernel32.dll")]
    private static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

    [DllImport("kernel32.dll")]
    private static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    #endregion

    #region Fields

    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESSENTRY32
    {
        public uint dwSize;
        public uint cntUsage;
        public uint th32ProcessID;
        public IntPtr th32DefaultHeapID;
        public uint th32ModuleID;
        public uint cntThreads;
        public uint th32ParentProcessID;
        public int pcPriClassBase;
        public uint dwFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExeFile;
    }

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
    public struct ProcessInformation
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    private ProcessInformation _processInfo1;
    private ProcessInformation _processInfo2;

    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    public static List<ProcessInformation> _processList = new List<ProcessInformation>();

    #endregion

    #region Process Methods

    public static ProcessInformation StartExternalProcess(string programPath, string arguments,string currentDirectory)
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
            currentDirectory,
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

    public static void TerminateExternalProcess(ProcessInformation processInfo)
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

    public static void CloseProcessByName(string processName, IntPtr snapShot)
    {
        var processEntry = new PROCESSENTRY32
        {
            dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32))
        };

        // ReSharper disable once InvertIf
        if (Process32First(snapShot, ref processEntry))
        {
            do
            {
                // ReSharper disable once InvertIf
                if (processEntry.szExeFile.Equals(processName, StringComparison.OrdinalIgnoreCase))
                {
                    var processHandle = OpenProcess(processEntry.th32ProcessID);
                    // ReSharper disable once InvertIf
                    if (processHandle != IntPtr.Zero)
                    {
                        TerminateProcess(processHandle, 0); // Terminate the process
                        CloseHandle(processHandle);
                        break;
                    }
                }
            } while (Process32Next(snapShot, ref processEntry));
        }
    }

    private static IntPtr OpenProcess(uint processId)
    {
        const uint processTerminate = 0x0001;
        return OpenProcess(processTerminate, false, processId);
    }

    #endregion

    #region Test Methods

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _processInfo1 =
                StartExternalProcess(@"D:\Projects\Log[]\LogInsights\src\LogInsights\bin\x64\Release\LogInsights.exe",
                    null,null);
            _processInfo2 = StartExternalProcess(@"C:\Windows\System32\cmd.exe", null,null);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            TerminateExternalProcess(_processInfo1);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            TerminateExternalProcess(_processInfo2);
        }

        // ReSharper disable once InvertIf
        if (Input.GetKeyDown(KeyCode.D))
        {
            var snapshot = CreateToolhelp32Snapshot(2, 0);
            CloseProcessByName("cmd.exe", snapshot);
            CloseProcessByName("LogInsights.exe", snapshot);
            CloseHandle(snapshot);
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