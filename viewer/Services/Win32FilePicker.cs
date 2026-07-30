using System.Runtime.InteropServices;

namespace Backpack.Viewer.Services;

internal static class Win32FilePicker
{
    private const uint SIGDN_FILESYSPATH   = 0x80058000;
    private const uint FOS_FORCEFILESYSTEM = 0x00000040;
    private const uint FOS_FILEMUSTEXIST   = 0x00001000;

    public static string? PickFile(nint owner, params (string Name, string Spec)[] filters)
    {
        var dialog = (IFileOpenDialog)new FileOpenDialogRcw();
        try
        {
            if (filters.Length > 0)
            {
                var specs = new COMDLG_FILTERSPEC[filters.Length];
                for (int i = 0; i < filters.Length; i++)
                    specs[i] = new COMDLG_FILTERSPEC { pszName = filters[i].Name, pszSpec = filters[i].Spec };
                dialog.SetFileTypes((uint)specs.Length, specs);
            }

            dialog.GetOptions(out uint options);
            dialog.SetOptions(options | FOS_FORCEFILESYSTEM | FOS_FILEMUSTEXIST);

            if (dialog.Show(owner) != 0)
                return null;

            dialog.GetResult(out IShellItem item);
            try
            {
                item.GetDisplayName(SIGDN_FILESYSPATH, out nint buffer);
                var path = Marshal.PtrToStringUni(buffer);
                Marshal.FreeCoTaskMem(buffer);
                return path;
            }
            finally
            {
                Marshal.ReleaseComObject(item);
            }
        }
        finally
        {
            Marshal.ReleaseComObject(dialog);
        }
    }

    [ComImport]
    [Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
    private class FileOpenDialogRcw { }

    [ComImport]
    [Guid("d57c7288-d4ad-4768-be02-9d969532d960")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileOpenDialog
    {
        [PreserveSig] int Show(nint parent);
        void SetFileTypes(uint cFileTypes, [In, MarshalAs(UnmanagedType.LPArray)] COMDLG_FILTERSPEC[] rgFilterSpec);
        void SetFileTypeIndex(uint iFileType);
        void GetFileTypeIndex(out uint piFileType);
        void Advise(nint pfde, out uint pdwCookie);
        void Unadvise(uint dwCookie);
        void SetOptions(uint fos);
        void GetOptions(out uint pfos);
        void SetDefaultFolder(nint psi);
        void SetFolder(nint psi);
        void GetFolder(out nint ppsi);
        void GetCurrentSelection(out nint ppsi);
        void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetFileName(out nint pszName);
        void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
        void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
        void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
        void GetResult(out IShellItem ppsi);
        void AddPlace(nint psi, int fdap);
        void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
        void Close(int hr);
        void SetClientGuid(ref Guid guid);
        void ClearClientData();
        void SetFilter(nint pFilter);
        void GetResults(out nint ppenum);
        void GetSelectedItems(out nint ppsai);
    }

    [ComImport]
    [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItem
    {
        void BindToHandler(nint pbc, ref Guid bhid, ref Guid riid, out nint ppv);
        void GetParent(out IShellItem ppsi);
        void GetDisplayName(uint sigdnName, out nint ppszName);
        void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
        void Compare(IShellItem psi, uint hint, out int piOrder);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct COMDLG_FILTERSPEC
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string pszName;
        [MarshalAs(UnmanagedType.LPWStr)] public string pszSpec;
    }
}
