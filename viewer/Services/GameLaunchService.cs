using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Backpack.Viewer.Localization;

namespace Backpack.Viewer.Services;

internal static class GameLaunchService
{
    private const string DllFileName = "backpack.dll";

    public static bool IsGameRunning()
        => Process.GetProcessesByName("YuanShen").Length > 0;

    public static string? GetDllPath()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "modules", DllFileName);
        return File.Exists(path) ? path : null;
    }

    public static async Task<int> LaunchAsync(string gameExePath)
    {
        var dllPath    = GetDllPath();
        var gameDir    = Path.GetDirectoryName(gameExePath) ?? AppContext.BaseDirectory;
        var cfgFile    = Path.Combine(Path.GetTempPath(), $"BackpackViewer_{Guid.NewGuid():N}.tmp");
        var currentExe = Environment.ProcessPath
            ?? throw new InvalidOperationException(Localized.Get("ErrNoProcessPath"));

        File.WriteAllLines(cfgFile, [
            gameExePath,
            dllPath ?? string.Empty,
            gameDir,
            string.Empty,
            "0",
        ]);

        var psi = new ProcessStartInfo
        {
            FileName         = currentExe,
            Arguments        = $"--elevated-inject \"{cfgFile}\"",
            UseShellExecute  = true,
            Verb             = "runas",
            WorkingDirectory = Path.GetDirectoryName(currentExe),
        };

        return await Task.Run(() =>
        {
            Process? helper;
            try
            {
                helper = Process.Start(psi);
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                TryDelete(cfgFile);
                throw new InvalidOperationException(Localized.Get("ErrElevationCancelled"));
            }

            if (helper is null)
            {
                TryDelete(cfgFile);
                throw new InvalidOperationException(Localized.Get("ErrHelperStartFailed"));
            }

            using (helper)
            {
                helper.WaitForExit();
                int code = helper.ExitCode;
                if (code != 0)
                {
                    TryDelete(cfgFile);
                    throw new InvalidOperationException(code switch
                    {
                        1 => Localized.Get("ErrInvalidConfig"),
                        2 => Localized.Get("ErrGameCreateFailed"),
                        3 => Localized.Get("ErrDllInjFailed"),
                        _ => string.Format(Localized.Get("ErrHelperExitCode"), code),
                    });
                }

                int gamePid = 0;
                try { int.TryParse(File.ReadAllText(cfgFile).Trim(), out gamePid); } catch { }
                TryDelete(cfgFile);
                return gamePid;
            }
        });
    }

    public static int RunElevatedInjection(string configFile)
    {
        try
        {
            if (!File.Exists(configFile)) return 1;

            string[] lines = File.ReadAllLines(configFile);
            if (lines.Length < 5) return 1;

            string gameExePath = lines[0];
            string dllPath     = lines[1];
            string workDir     = lines[2];
            string cmdArgs     = lines[3];

            int customCount = int.TryParse(lines[4], out int cnt) ? cnt : 0;
            var customDlls  = new List<string>();
            for (int i = 0; i < customCount && (5 + i) < lines.Length; i++)
                if (File.Exists(lines[5 + i]))
                    customDlls.Add(lines[5 + i]);

            string fullCmd = string.IsNullOrEmpty(cmdArgs)
                ? $"\"{gameExePath}\""
                : $"\"{gameExePath}\" {cmdArgs}";

            STARTUPINFOW si = new();
            si.cb = (uint)Marshal.SizeOf<STARTUPINFOW>();

            if (!NativeMethods.CreateProcessW(
                gameExePath, fullCmd, 0, 0, false, 0x4, 0, workDir, ref si, out PROCESS_INFORMATION pi))
                return 2;

            if (!string.IsNullOrEmpty(dllPath) && !InjectDll(pi.hProcess, dllPath))
            {
                NativeMethods.TerminateProcess(pi.hProcess, 1);
                NativeMethods.CloseHandle(pi.hThread);
                NativeMethods.CloseHandle(pi.hProcess);
                return 3;
            }

            foreach (var dll in customDlls)
                InjectDll(pi.hProcess, dll);

            NativeMethods.ResumeThread(pi.hThread);
            NativeMethods.CloseHandle(pi.hThread);
            File.WriteAllText(configFile, pi.dwProcessId.ToString());
            NativeMethods.CloseHandle(pi.hProcess);
            return 0;
        }
        catch
        {
            return 99;
        }
    }

    private static bool InjectDll(nint hProcess, string dllPath)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(dllPath + "\0");

        nint mem = NativeMethods.VirtualAllocEx(hProcess, 0, (nuint)bytes.Length, 0x3000, 0x04);
        if (mem == 0) return false;

        if (!NativeMethods.WriteProcessMemory(hProcess, mem, bytes, (nuint)bytes.Length, out _))
        {
            NativeMethods.VirtualFreeEx(hProcess, mem, 0, 0x8000);
            return false;
        }

        nint k32   = NativeMethods.GetModuleHandleW("kernel32.dll");
        nint loadW = NativeMethods.GetProcAddress(k32, "LoadLibraryW");
        if (loadW == 0)
        {
            NativeMethods.VirtualFreeEx(hProcess, mem, 0, 0x8000);
            return false;
        }

        nint thread = NativeMethods.CreateRemoteThread(hProcess, 0, 0, loadW, mem, 0, out _);
        if (thread == 0)
        {
            NativeMethods.VirtualFreeEx(hProcess, mem, 0, 0x8000);
            return false;
        }

        NativeMethods.WaitForSingleObject(thread, 10000);
        NativeMethods.GetExitCodeThread(thread, out uint result);
        NativeMethods.CloseHandle(thread);
        NativeMethods.VirtualFreeEx(hProcess, mem, 0, 0x8000);
        return result != 0;
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct STARTUPINFOW
    {
        public uint   cb;
        public nint   lpReserved, lpDesktop, lpTitle;
        public uint   dwX, dwY, dwXSize, dwYSize, dwXCountChars, dwYCountChars, dwFillAttribute, dwFlags;
        public ushort wShowWindow, cbReserved2;
        public nint   lpReserved2, hStdInput, hStdOutput, hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_INFORMATION
    {
        public nint hProcess, hThread;
        public uint dwProcessId, dwThreadId;
    }

    private static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CreateProcessW(
            string? lpApplicationName, string lpCommandLine,
            nint lpProcessAttributes, nint lpThreadAttributes,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandles,
            uint dwCreationFlags, nint lpEnvironment, string? lpCurrentDirectory,
            ref STARTUPINFOW lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll")]
        public static extern uint ResumeThread(nint hThread);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TerminateProcess(nint hProcess, uint uExitCode);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(nint hObject);

        [DllImport("kernel32.dll")]
        public static extern nint VirtualAllocEx(
            nint hProcess, nint lpAddress, nuint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VirtualFreeEx(
            nint hProcess, nint lpAddress, nuint dwSize, uint dwFreeType);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteProcessMemory(
            nint hProcess, nint lpBaseAddress, byte[] lpBuffer, nuint nSize,
            out nuint lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern nint CreateRemoteThread(
            nint hProcess, nint lpThreadAttributes, nuint dwStackSize,
            nint lpStartAddress, nint lpParameter, uint dwCreationFlags, out uint lpThreadId);

        [DllImport("kernel32.dll")]
        public static extern uint WaitForSingleObject(nint hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern nint GetModuleHandleW(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern nint GetProcAddress(nint hModule, string lpProcName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetExitCodeThread(nint hThread, out uint lpExitCode);
    }
}
