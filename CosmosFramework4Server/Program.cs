using System;
using Cosmos;
using Cosmos.Network;
using System.Threading.Tasks;
using ProtocolCore;
using System.IO;
using System.Runtime.InteropServices;

namespace CosmosFramework4Server
{
    class Program
    {
        public delegate bool ControlCtrlDelegate(int CtrlType);
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate HandlerRoutine, bool Add);
        static ControlCtrlDelegate newDelegate = new ControlCtrlDelegate(HandlerRoutine);
        public static bool HandlerRoutine(int CtrlType)
        {
            Utility.Debug.LogInfo("Server Shutdown !");//按控制台关闭按钮关闭 
            return false;
        }
        static string ip = "127.0.0.1";
        static int port = 8511;
        static void Main(string[] args)
        {
            SetConsoleCtrlHandler(newDelegate, true);
            Utility.Logger.SetHelper(new ConsoleLoggerHelper());
            Utility.Debug.SetHelper(new ConsoleDebugHelper());
            Utility.Debug.LogInfo("Server Start Running !");
            GameManager.NetworkManager.Connect(ip, port, System.Net.Sockets.ProtocolType.Udp);
            GameManager.External.GetModule<PeerManager>();
            Task.Run(GameManagerAgent.Instance.OnRefresh);
            while (true) { }
         //   Task.Run(AsyncCoroutine.Instance.Start);
        }
        /// <summary>
        /// 异步单线程协程测试函数
        /// </summary>
        static void AsyncCoroutineTest()
        {
            Task.Run(AsyncCoroutine.Instance.Start);
            Console.WriteLine(DateTime.Now.Ticks / 10000);
            Console.WriteLine(Utility.Time.MillisecondNow());
            AsyncCoroutine.Instance.WaitTimeAsyncCallback(1001, () =>
            {
                Console.WriteLine("Callback01");
                Task.Run(() => { AsyncCoroutine.Instance.WaitTimeAsyncCallback(2300, () => Console.WriteLine("Callback0233")); });
            });
            AsyncCoroutine.Instance.WaitTimeAsyncCallback(1500, () => Console.WriteLine("Callback02"));
            AsyncCoroutine.Instance.WaitTimeAsyncCallback(2000, () => Console.WriteLine("Callback03"));
            AsyncCoroutine.Instance.WaitTimeAsyncCallback(3000, () =>
            {
                Console.WriteLine("Callback04");
                AsyncCoroutine.Instance.WaitTimeAsyncCallback(6000, () => Console.WriteLine("Callback05"));
            });
            Console.WriteLine(AsyncCoroutine.Instance.CorouCount);
            AsyncCoroutine.Instance.WaitTimeAsyncCallback(15001, () =>
            {
                Console.WriteLine("Callback06");
                Console.WriteLine(AsyncCoroutine.Instance.CorouCount);
            });
        }
    }
}
