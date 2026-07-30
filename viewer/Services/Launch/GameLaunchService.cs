using System.Diagnostics;
using Backpack.Viewer.Localization;

namespace Backpack.Viewer.Services;

internal static partial class GameLaunchService
{
    private const string DllFileName = "backpack.dll";

    public static bool IsGameRunning()
        => Process.GetProcessesByName("YuanShen").Length > 0;

    public static string? GetDllPath()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "modules", DllFileName);
        return File.Exists(path) ? path : null;
    }

    public static string GetOutputDir(string gameExePath) =>
        Path.Combine(Path.GetDirectoryName(gameExePath) ?? AppContext.BaseDirectory, "output");

    public static async Task<int> LaunchAsync(string gameExePath)
    {
        var dllPath    = GetDllPath();
        var gameDir    = Path.GetDirectoryName(gameExePath) ?? AppContext.BaseDirectory;
        var cfgFile    = Path.Combine(Path.GetTempPath(), $"BackpackViewer_{Guid.NewGuid():N}.tmp");
        var currentExe = Environment.ProcessPath
            ?? throw new InvalidOperationException(SR.ErrNoProcessPath);

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
                throw new InvalidOperationException(SR.ErrElevationCancelled);
            }

            if (helper is null)
            {
                TryDelete(cfgFile);
                throw new InvalidOperationException(SR.ErrHelperStartFailed);
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
                        1 => SR.ErrInvalidConfig,
                        2 => SR.ErrGameCreateFailed,
                        3 => SR.ErrDllInjFailed,
                        _ => string.Format(SR.ErrHelperExitCode, code),
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
            si.cb = (uint)System.Runtime.InteropServices.Marshal.SizeOf<STARTUPINFOW>();

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

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { }
    }
}
