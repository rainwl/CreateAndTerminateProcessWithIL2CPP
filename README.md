# CreateAndTerminateProcessWithIL2CPP
## Overview
When developing with Unity, if you choose `IL2CPP` as the scripting backend, 
it brings along several challenges. 

- First, code that relies on C# reflection is not 
supported. Common libraries like Protocol Buffers and log4net, which use reflection, 
require modifications. 

- Secondly, the `System.Diagnostics.Process` namespace is also unsupported.
If you need to launch an external application, an alternative approach is necessary.
Unfortunately, Unity lacks sufficient resources to modify IL2CPP to support Process.
The solutions available online are quite scarce, 
with only one GitHub repository providing a potential workaround.
However, this solution is not readily usable due to various issues. 

Given this situation, 
I've developed a method utilizing kernel32 API calls to achieve launching and 
closing external processes, as well as terminating processes with specific names.

## Usage


Here, there's a script called `ProcessLauncher` which doesn't require inheriting
from `MonoBehaviour` (I inherited it for testing purposes using `Update`).
The key methods are located in the `Process Methods` region, 
namely `StartExternalProcess`, `TerminateExternalProcess`, and `CloseProcessByName`.


The `StartExternalProcess` method can either return a variable of type 
`ProcessInformation` or not, depending on your preference. If it does return, 
you can use the `TerminateExternalProcess` method with that `ProcessInformation` to 
close the specific process. For instance, if you've launched multiple external` .exe`
files, you can selectively close one or more of them based on the `ProcessInformation`.


Lastly, the `CloseProcessByName` method involves first creating a process snapshot,
then comparing the names to close processes with specific names. 
Note that the processName variable should include the ".exe" extension.
After closing the process, remember to clean up the snapshot.Like this:

```csharp
var snapshot = CreateToolhelp32Snapshot(2, 0);
CloseProcessByName("cmd.exe", snapshot);
CloseHandle(snapshot);
```


Regarding the usage of these methods, I've placed them in the `Update` function.
Pressing `A` will launch two external applications. You should modify this as needed. 
Pressing `B` and `C` will close these external applications based on their process 
information. Pressing `D` will close these processes based on their names.

## Notice

lpCommandLine is LPTSTR, not LPCTSTR, 
so the argument cannot be a string constant; it must be a writable array of strings

```c++
wchar_t wszCmd[] = L" -arg";
CreateProcess(L"C:\\Program Files\\WinRAR\\WinRAR.exe", wszCmd, ...);
```

Note that wszCmd[], the content of lpCommandLine, needs a space at the beginning, otherwise it is attached to `lpApplicationName`.

## Other
Here are some relevant links, including forums and repositories. If you find my code helpful, feel free to give it a star.

https://forum.unity.com/threads/solved-il2cpp-and-process-start.533988/

https://github.com/josh4364/IL2cppStartProcess

https://learn.microsoft.com/zh-cn/windows/win32/api/processthreadsapi/nf-processthreadsapi-createprocessw

