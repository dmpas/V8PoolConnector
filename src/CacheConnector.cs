using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using V82;

namespace V8Pool
{

    [Guid(CacheConnector.InterfaceId), ComVisible(true)]
    public interface ICacheConnector
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        object connect([MarshalAs(UnmanagedType.BStr)] [In] string connString);

        void setCacheId([MarshalAs(UnmanagedType.BStr)] [In] string cacheId);

        [return: MarshalAs(UnmanagedType.Interface)]
        object connectAgent([MarshalAs(UnmanagedType.BStr)] [In] string connString);

        [return: MarshalAs(UnmanagedType.Interface)]
        object connectWorkingProcess([MarshalAs(UnmanagedType.BStr)] [In] string connString);
    }


    [ClassInterface(ClassInterfaceType.None)]
    [Guid(CacheConnector.ClassId), ComVisible(true)]
    public class CacheConnector : ReferenceCountedObject, ICacheConnector
    {
        internal const string ClassId =
            "6919b458-f791-4b74-80b3-b77e0ac50cbf";
        internal const string InterfaceId =
            "bb6caf0e-a415-4386-aaea-c702960f76fa";
        internal const string EventsId =
            "af6ee9ab-d2b8-43ad-9dbb-db82efe4cdb5";

        private string _cacheId = "";

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

        public CacheConnector()
        {
            setCacheId("");
        }

        private class ConnectionData
        {
            public DateTime connected;
            public object connection;
        }

        private static Dictionary<string, Dictionary<string, ConnectionData>> data = new Dictionary<string, Dictionary<string, ConnectionData>>();
        private Dictionary<string, ConnectionData> localdata = null;

        public object connect(string connString)
        {
            lock (localdata)
            {
                if (localdata.ContainsKey(connString))
                {
                    /* Провѣрить существованіе объекта */
                    return localdata[connString].connection;
                }

                V82.COMConnectorClass ctr = new V82.COMConnectorClass();
                object result = ctr.Connect(connString);
                localdata[connString] = new ConnectionData { connected=DateTime.Now, connection=result };
                return result;
            }
        }

        private static Dictionary<string, Dictionary<string, V82.IServerAgentConnection>> agentData = new Dictionary<string, Dictionary<string, IServerAgentConnection>>();
        private Dictionary<string, V82.IServerAgentConnection> localAgentData = null;

        public object connectAgent([MarshalAs(UnmanagedType.BStr)] [In] string connString)
        {
            lock (localAgentData)
            {
                if (localAgentData.ContainsKey(connString))
                {
                    /* Провѣрить существованіе объекта */
                    return localAgentData[connString];
                }

                V82.COMConnectorClass ctr = new V82.COMConnectorClass();
                V82.IServerAgentConnection result = ctr.ConnectAgent(connString);
                localAgentData[connString] = result;
                return result;
            }
        }

        private static Dictionary<string, Dictionary<string, V82.IWorkingProcessConnection>> processData = new Dictionary<string, Dictionary<string, IWorkingProcessConnection>>();
        private Dictionary<string, V82.IWorkingProcessConnection> localProcessData = null;

        public object connectWorkingProcess([MarshalAs(UnmanagedType.BStr)] [In] string connString)
        {
            lock (localProcessData)
            {
                if (localProcessData.ContainsKey(connString))
                {
                    /* Провѣрить существованіе объекта */
                    return localProcessData[connString];
                }

                V82.COMConnectorClass ctr = new V82.COMConnectorClass();
                V82.IWorkingProcessConnection result = ctr.ConnectWorkingProcess(connString);
                localProcessData[connString] = result;
                return result;
            }
        }

        public void setCacheId(string cacheId)
        {
            this._cacheId = cacheId;
            lock (data) 
            {
                if (data.ContainsKey(cacheId))
                {
                    localdata = data[cacheId];
                }
                else
                {
                    localdata = new Dictionary<string, ConnectionData>();
                    data[cacheId] = localdata;
                }
            }

            lock (agentData)
            {
                if (agentData.ContainsKey(cacheId))
                {
                    localAgentData = agentData[cacheId];
                }
                else
                {
                    localAgentData = new Dictionary<string, IServerAgentConnection>();
                    agentData[cacheId] = localAgentData;
                }
            }

            lock (processData)
            {
                if (processData.ContainsKey(cacheId))
                {
                    localProcessData = processData[cacheId];
                }
                else
                {
                    localProcessData = new Dictionary<string, IWorkingProcessConnection>();
                    processData[cacheId] = localProcessData;
                }
            }
        }

    }


    internal class CacheConnectorClassFactory : IClassFactory
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

            if (riid == new Guid(CacheConnector.ClassId) ||
                riid == new Guid(COMNative.IID_IDispatch) ||
                riid == new Guid(COMNative.IID_IUnknown))
            {
                // Create the instance of the .NET object
                ppvObject = Marshal.GetComInterfaceForObject(
                    new CacheConnector(), typeof(ICacheConnector));
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

