using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace V8Pool
{
    sealed internal class SingleComServer
    {
        private SingleComServer()
        {
        }

        private static SingleComServer _instance = new SingleComServer();
        public static SingleComServer Instance
        {
            get { return _instance; }
        }

        private object syncRoot = new Object();
        private bool _bRunning = false;
        private uint _nMainThreadID = 0;
        private int _nLockCnt = 0;
        private Timer _gcTimer;
        private Timer _killTimer = null;

        private int killTimeout = 10000;

        private static void KillTimerProc(object stateInfo)
        {
            NativeMethod.PostThreadMessage((stateInfo as SingleComServer)._nMainThreadID, 
                NativeMethod.WM_QUIT, UIntPtr.Zero, IntPtr.Zero); 
        }

        private void startKillTimer()
        {
            stopKillTimer();
            _killTimer = new Timer(KillTimerProc, this, killTimeout, killTimeout);
        }

        private void stopKillTimer()
        {
            if (_killTimer != null)
            {
                _killTimer.Dispose();
                _killTimer = null;
            }
        }

        private static void GarbageCollect(object stateInfo)
        {
            GC.Collect();
        }

        private uint _cookieSimpleObj;

        private void PreMessageLoop()
        {
            Guid clsidSimpleObj = new Guid(CacheConnector.ClassId);

            int hResult = COMNative.CoRegisterClassObject(
                ref clsidSimpleObj,
                new CacheConnectorClassFactory(),
                CLSCTX.LOCAL_SERVER,
                REGCLS.MULTIPLEUSE | REGCLS.SUSPENDED,
                out _cookieSimpleObj);
            if (hResult != 0)
            {
                throw new ApplicationException(
                    "CoRegisterClassObject failed w/err 0x" + hResult.ToString("X"));
            }

            hResult = COMNative.CoResumeClassObjects();
            if (hResult != 0)
            {
                if (_cookieSimpleObj != 0)
                {
                    COMNative.CoRevokeClassObject(_cookieSimpleObj);
                }

                throw new ApplicationException(
                    "CoResumeClassObjects failed w/err 0x" + hResult.ToString("X"));
            }

            _nMainThreadID = NativeMethod.GetCurrentThreadId();
            _nLockCnt = 0;
            _gcTimer = new Timer(new TimerCallback(GarbageCollect), null,
                5000, 5000);
        }

        private void RunMessageLoop()
        {
            MSG msg;
            while (NativeMethod.GetMessage(out msg, IntPtr.Zero, 0, 0))
            {
                NativeMethod.TranslateMessage(ref msg);
                NativeMethod.DispatchMessage(ref msg);
            }
        }

        private void PostMessageLoop()
        {
            if (_cookieSimpleObj != 0)
            {
                COMNative.CoRevokeClassObject(_cookieSimpleObj);
            }

            if (_gcTimer != null)
            {
                _gcTimer.Dispose();
            }

            Thread.Sleep(1000);
        }

        public void Run()
        {
            lock (syncRoot)
            {
                if (_bRunning)
                    return;

                _bRunning = true;
            }

            try
            {
                PreMessageLoop();
                RunMessageLoop();
                PostMessageLoop();
            }
            finally
            {
                _bRunning = false;
            }
        }

        public int Lock()
        {
            stopKillTimer();
            return Interlocked.Increment(ref _nLockCnt);
        }

        public int Unlock()
        {
            int nRet = Interlocked.Decrement(ref _nLockCnt);

            if (nRet == 0)
            {
                startKillTimer();
            }

            return nRet;
        }

        public int GetLockCount()
        {
            return _nLockCnt;
        }
    }
}