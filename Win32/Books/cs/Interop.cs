//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.UserActivities;
using Windows.UI.ViewManagement;

namespace Books
{
    [Guid("ECC62F5D-14AA-4971-9F06-B2159B1FFD40")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IClassicApplicationViewFactory
    {
        IClassicApplicationView GetForWindow(IntPtr windowHandle, [MarshalAs(UnmanagedType.LPStruct)] Guid riid);
    }

    [Guid("7A05F995-6242-440E-A64E-34B7ED3413D3")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IClassicApplicationView
    {
        ApplicationViewTitleBar GetTitleBar([MarshalAs(UnmanagedType.LPStruct)] Guid riid);
        IntPtr GetIcon();
        void SetIcon(IntPtr icon);
    }

    [ComImport]
    [Guid("4A765F48-1D55-49DE-9B20-19F09AD0D1A7")]
    public class ClassicApplicationViewFactory
    {
    }

    [Guid("1ADE314D-0E0A-40D9-824C-9A088A50059F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    public interface IUserActivityInterop
    {
        UserActivitySession CreateSessionForWindow(IntPtr windowHandle, [MarshalAs(UnmanagedType.LPStruct)] Guid riid);

    }

    [Guid("DD69F876-9699-4715-9095-E37EA30DFA1B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    public interface IUserActivityRequestManagerInterop
    {
        UserActivityRequestManager GetForWindow(IntPtr windowHandle, [MarshalAs(UnmanagedType.LPStruct)] Guid riid);
    }

    [ComVisible(true)]
    [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IServiceProvider
    {
        [PreserveSig]
        int QueryService([MarshalAs(UnmanagedType.LPStruct)] Guid serviceId, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IntPtr result);
    }

    [Guid("1791e8f6-21c7-4340-882a-a6a93e3fd73b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ILaunchUIContext
    {
        void SetAssociatedWindow(IntPtr windowHandle);
        void SetTabGroupingPreference(int groupingPreference);
    }

    [ComVisible(true)]
    [Guid("0d12c4c8-a3d9-4e24-94c1-0e20c5a956c4")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ILaunchUIContextProvider
    {
        void UpdateContext(ILaunchUIContext context);
    }

    public static class ComInterop
    {
        public static ApplicationViewTitleBar GetTitleBarForWindow(IntPtr windowHandle)
        {
            try
            {
                var factory = (IClassicApplicationViewFactory)new ClassicApplicationViewFactory();
                var view = factory.GetForWindow(windowHandle, typeof(IClassicApplicationView).GUID);
                return view.GetTitleBar(typeof(ApplicationViewTitleBar).GetInterface("IApplicationViewTitleBar").GUID);
            }
            catch (COMException ex) when ((uint)ex.ErrorCode == 0x80040154) // REGDB_E_CLASSNOTREG
            {
                return null;
            }
        }

        public static UserActivitySession CreateSessionForWindow(this UserActivity activity, IntPtr windowHandle)
        {
            return ((IUserActivityInterop)(object)activity).CreateSessionForWindow(windowHandle, typeof(UserActivitySession).GetInterface("IUserActivitySession").GUID);
        }

        public static UserActivityRequestManager GetUserActivityRequestManagerForWindow(IntPtr windowHandle)
        {
            var interop = (IUserActivityRequestManagerInterop)WindowsRuntimeMarshal.GetActivationFactory(typeof(UserActivityRequestManager));
            return interop?.GetForWindow(windowHandle, typeof(UserActivityRequestManager).GetInterface("IUserActivityRequestManager").GUID);
        }
    }

    public static class DwmInterop
    {
        [DllImport("dwmapi.dll", ExactSpelling = true)]
        public static extern int DwmSetWindowAttribute(IntPtr windowHandle, int attribute, ref int value, int size);

        [DllImport("dwmapi.dll", ExactSpelling = true)]
        public static extern int DwmSetWindowAttribute(IntPtr windowHandle, int attribute, ref IntPtr value, int size);

        public const int DWMWA_TABBING_ENABLED = 16;
        public const int DWMWA_ASSOCIATED_WINDOW = 17;
        public const int DWMWA_TAB_GROUPING_PREFERENCE = 18;

        public const int DWMTGP_DEFAULT = 0;
        public const int DWMTGP_TAB_WITH_ASSOCIATED_WINDOW = 1;
        public const int DWMTGP_NEW_TAB_GROUP = 2;

        [Flags]
        public enum DWM_TAB_WINDOW_REQUIREMENTS
        {
            DWMTWR_NONE = 0x0000,
            DWMTWR_IMPLEMENTED_BY_SYSTEM = 0x0001,
            DWMTWR_WINDOW_RELATIONSHIP = 0x0002,
            DWMTWR_WINDOW_STYLES = 0x0004,
            DWMTWR_WINDOW_REGION = 0x0008,
            DWMTWR_WINDOW_DWM_ATTRIBUTES = 0x0010,
            DWMTWR_WINDOW_MARGINS = 0x0020,
            DWMTWR_TABBING_ENABLED = 0x0040,
            DWMTWR_USER_POLICY = 0x0080
        }

        [DllImport("dwmapi.dll", EntryPoint = "DwmGetUnmetTabRequirements", ExactSpelling = true)]
        public static extern int RawDwmGetUnmetTabRequirements(IntPtr windowHandle, out DWM_TAB_WINDOW_REQUIREMENTS unmetRequirements);

        public static int DwmGetUnmetTabRequirements(IntPtr windowHandle, out DWM_TAB_WINDOW_REQUIREMENTS unmetRequirements)
        {
            try
            {
                return RawDwmGetUnmetTabRequirements(windowHandle, out unmetRequirements);
            }
            catch (EntryPointNotFoundException)
            {
                unmetRequirements = DWM_TAB_WINDOW_REQUIREMENTS.DWMTWR_IMPLEMENTED_BY_SYSTEM;
                return unchecked((int)0x80070057); // E_NOTIMPL
            }
        }
    }

    public static class ShellInterop
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHELLEXECUTEINFO
        {
            public int StructureSize;
            public uint Mask;
            public IntPtr WindowHandle;
            public string Verb;
            public string File;
            public string Parameters;
            public string Directory;
            public int ShowCommand;
            public IntPtr Site;
            public IntPtr IDList;
            public string ClassName;
            public IntPtr ClassKey;
            public uint HotKey;
            public IntPtr IconOrMonitor;
            public IntPtr ProcessHandle;
        };

        public const uint SEE_MASK_FLAG_HINST_IS_SITE = 0x08000000;
        public const int SW_SHOWNORMAL = 1;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int ShellExecuteEx(ref SHELLEXECUTEINFO info);
    }

    public class ComPtr : IDisposable
    {
        IntPtr rawUnknown;

        public ComPtr(object o)
        {
            rawUnknown = Marshal.GetIUnknownForObject(o);
        }

        public IntPtr IntPtr => rawUnknown;

        public void Dispose()
        {
            var unknown = System.Threading.Interlocked.Exchange(ref rawUnknown, IntPtr.Zero);
            if (unknown != IntPtr.Zero)
            {
                Marshal.Release(unknown);
            }
        }
    }
}
