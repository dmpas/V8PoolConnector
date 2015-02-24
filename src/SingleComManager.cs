using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using V82;

namespace DmpasComSingleton
{


    [Guid(SingleComManager.InterfaceId), ComVisible(true)]
    public interface ISingleComManager
    {
        int getSomeData();
        void setSomeData(int data);

        [return: MarshalAs(UnmanagedType.Interface)]
        object connect([MarshalAs(UnmanagedType.BStr)] [In] string connString);
    }


    [ClassInterface(ClassInterfaceType.None)]
    [Guid(SingleComManager.ClassId), ComVisible(true)]
    public class SingleComManager : ReferenceCountedObject, ISingleComManager
    {
        internal const string ClassId =
            "6919b458-f791-4b74-80b3-b77e0ac50cbf";
        internal const string InterfaceId =
            "bb6caf0e-a415-4386-aaea-c702960f76fa";
        internal const string EventsId =
            "af6ee9ab-d2b8-43ad-9dbb-db82efe4cdb5";

        [EditorBrowsable(EditorBrowsableState.Never)]
        [ComRegisterFunction()]
        public static void Register(Type t)
        {
            try
            {
                COMHelper.RegasmRegisterLocalServer(t);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // Log the error
                throw ex; // Re-throw the exception
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [ComUnregisterFunction()]
        public static void Unregister(Type t)
        {
            try
            {
                COMHelper.RegasmUnregisterLocalServer(t);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // Log the error
                throw ex; // Re-throw the exception
            }
        }

        public SingleComManager()
        {
        }

        static int someData = 5;

        public int getSomeData()
        {
            return someData;
        }

        public void setSomeData(int data)
        {
            someData = data;
        }

        private static Dictionary<string, object> data = new Dictionary<string, object>();

        public object connect(string connString)
        {
            lock (data)
            {
                if (data.ContainsKey(connString))
                {
                    /* Провѣрить существованіе объекта */
                    return data[connString];
                }

                V82.COMConnectorClass ctr = new V82.COMConnectorClass();
                object result = ctr.Connect(connString);
                data[connString] = result;
                return result;
            }
        }

    }


    internal class SingleComManagerClassFactory : IClassFactory
    {
        public int CreateInstance(IntPtr pUnkOuter, ref Guid riid, 
            out IntPtr ppvObject)
        {
            ppvObject = IntPtr.Zero;

            if (pUnkOuter != IntPtr.Zero)
            {
                // The pUnkOuter parameter was non-NULL and the object does 
                // not support aggregation.
                Marshal.ThrowExceptionForHR(COMNative.CLASS_E_NOAGGREGATION);
            }

            if (riid == new Guid(SingleComManager.ClassId) ||
                riid == new Guid(COMNative.IID_IDispatch) ||
                riid == new Guid(COMNative.IID_IUnknown))
            {
                // Create the instance of the .NET object
                ppvObject = Marshal.GetComInterfaceForObject(
                    new SingleComManager(), typeof(ISingleComManager));
            }
            else
            {
                // The object that ppvObject points to does not support the 
                // interface identified by riid.
                Marshal.ThrowExceptionForHR(COMNative.E_NOINTERFACE);
            }

            return 0;   // S_OK
        }

        public int LockServer(bool fLock)
        {
            return 0;   // S_OK
        }
    }

    [ComVisible(false)]
    public class ReferenceCountedObject
    {
        public ReferenceCountedObject()
        {
            SingleComServer.Instance.Lock();
        }

        ~ReferenceCountedObject()
        {
            SingleComServer.Instance.Unlock();
        }
    }
}

