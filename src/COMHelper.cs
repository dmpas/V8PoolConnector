using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Reflection;

internal class COMHelper
{
    public static void RegasmRegisterLocalServer(Type t)
    {
        GuardNullType(t, "t"); 

        using (RegistryKey keyCLSID = Registry.ClassesRoot.OpenSubKey(
            @"CLSID\" + t.GUID.ToString("B"), /*writable*/true))
        {
            keyCLSID.DeleteSubKeyTree("InprocServer32");

            using (RegistryKey subkey = keyCLSID.CreateSubKey("LocalServer32"))
            {
                subkey.SetValue("", Assembly.GetExecutingAssembly().Location,
                    RegistryValueKind.String);
            }
        }
    }

    public static void RegasmUnregisterLocalServer(Type t)
    {
        GuardNullType(t, "t"); 

        Registry.ClassesRoot.DeleteSubKeyTree(@"CLSID\" + t.GUID.ToString("B"));
    }

    private static void GuardNullType(Type t, String param)
    {
        if (t == null)
        {
            throw new ArgumentException("The CLR type must be specified.", param);
        }
    }
}

internal class COMNative
{
    [DllImport("ole32.dll")]
    public static extern int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

    [DllImport("ole32.dll")]
    public static extern void CoUninitialize();

    [DllImport("ole32.dll")]
    public static extern int CoRegisterClassObject(
        ref Guid rclsid,
        [MarshalAs(UnmanagedType.Interface)] IClassFactory pUnk,
        CLSCTX dwClsContext,
        REGCLS flags,
        out uint lpdwRegister);

    [DllImport("ole32.dll")]
    public static extern UInt32 CoRevokeClassObject(uint dwRegister);

    [DllImport("ole32.dll")]
    public static extern int CoResumeClassObjects();

    public const string IID_IClassFactory =
        "00000001-0000-0000-C000-000000000046";

    public const string IID_IUnknown =
        "00000000-0000-0000-C000-000000000046";

    public const string IID_IDispatch =
        "00020400-0000-0000-C000-000000000046";

    public const int CLASS_E_NOAGGREGATION = unchecked((int)0x80040110);

    public const int E_NOINTERFACE = unchecked((int)0x80004002);
}

[ComImport(), ComVisible(false), 
InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
Guid(COMNative.IID_IClassFactory)]
internal interface IClassFactory
{
    [PreserveSig]
    int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject);

    [PreserveSig]
    int LockServer(bool fLock);
}

[Flags]
internal enum CLSCTX : uint
{
    INPROC_SERVER = 0x1,
    INPROC_HANDLER = 0x2,
    LOCAL_SERVER = 0x4,
    INPROC_SERVER16 = 0x8,
    REMOTE_SERVER = 0x10,
    INPROC_HANDLER16 = 0x20,
    RESERVED1 = 0x40,
    RESERVED2 = 0x80,
    RESERVED3 = 0x100,
    RESERVED4 = 0x200,
    NO_CODE_DOWNLOAD = 0x400,
    RESERVED5 = 0x800,
    NO_CUSTOM_MARSHAL = 0x1000,
    ENABLE_CODE_DOWNLOAD = 0x2000,
    NO_FAILURE_LOG = 0x4000,
    DISABLE_AAA = 0x8000,
    ENABLE_AAA = 0x10000,
    FROM_DEFAULT_CONTEXT = 0x20000,
    ACTIVATE_32_BIT_SERVER = 0x40000,
    ACTIVATE_64_BIT_SERVER = 0x80000
}

[Flags]
internal enum REGCLS : uint
{
    SINGLEUSE = 0,
    MULTIPLEUSE = 1,
    MULTI_SEPARATE = 2,
    SUSPENDED = 4,
    SURROGATE = 8,
}