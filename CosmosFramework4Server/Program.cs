using System;
using Cosmos;
using Cosmos.Network;
using System.Threading.Tasks;
using ProtocolCore;

namespace CosmosFramework4Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Utility.Debug.SetHelper(new ConsoleDebugHelper());
            Utility.Debug.LogInfo("Server start Running");
            NetworkManager.Instance.InitNetwork(System.Net.Sockets.ProtocolType.Udp);
            Task.Run(PollingManager.Instance.OnRefresh);
            Task.Run(AsyncCoroutine.Instance.Start);
            while (true){}
            Console.ReadLine();
        }
        /// <summary>
        /// 异步单线程协程测试函数
        /// </summary>
        static void AsyncCoroutineTest()
        {
            Task.Run(AsyncCoroutine.Instance.Start);
            Console.WriteLine(DateTime.Now.Ticks / 10000);
            Console.WriteLine(Utility.Time.MillisecondNow());
            AsyncCoroutine.Instance.WaitTimeAsyncCallback(1001, () => {
                Console.WriteLine("Callback01");
                Task.Run(() => { AsyncCoroutine.Instance.WaitTimeAsyncCallback(2300, () => Console.WriteLine("Callback0233")); });
            });
            AsyncCoroutine.Instance.WaitTimeAsyncCallback(1500, () => Console.WriteLine("Callback02"));
            AsyncCoroutine.Instance.WaitTimeAsyncCallback(2000, () => Console.WriteLine("Callback03"));
            AsyncCoroutine.Instance.WaitTimeAsyncCallback(3000, () => {
                Console.WriteLine("Callback04");
                AsyncCoroutine.Instance.WaitTimeAsyncCallback(6000, () => Console.WriteLine("Callback05"));
            });
            Console.WriteLine(AsyncCoroutine.Instance.CorouCount);
            AsyncCoroutine.Instance.WaitTimeAsyncCallback(15001, () => {
                Console.WriteLine("Callback06");
                Console.WriteLine(AsyncCoroutine.Instance.CorouCount);
            });
        }
    }
}
