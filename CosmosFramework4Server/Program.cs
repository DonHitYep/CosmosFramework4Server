using System;
using Cosmos;
using System.Threading.Tasks;
namespace CosmosFramework4Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            AsyncCoroutineTest();
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
