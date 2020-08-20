using System;
using Cosmos;
namespace CosmosFramework4Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            Console.WriteLine(DateTime.Now.Ticks / 10000);
            Console.WriteLine(Utility.Time.MillisecondNow());
            SingleThreadAsyncCoroutine.Instance.WaitTimeAsyncCallback(1000000, () => Console.WriteLine("Callback01"));
            SingleThreadAsyncCoroutine.Instance.WaitTimeAsyncCallback(1100000, () => Console.WriteLine("Callback02"));
            SingleThreadAsyncCoroutine.Instance.WaitTimeAsyncCallback(2000000, () => Console.WriteLine("Callback03"));
            SingleThreadAsyncCoroutine.Instance.WaitTimeAsyncCallback(60000000, () => Console.WriteLine("Callback04"));
            Console.WriteLine(SingleThreadAsyncCoroutine.Instance.CorouCount);

            //Locker locker = new Locker(SingleThreadAsyncCoroutine.Instance);
            //if (locker.HasLock)
            //{
            //    Console.WriteLine("线程被锁定，无法使用");
            //}
            SingleThreadAsyncCoroutine.Instance.Run();
            SingleThreadAsyncCoroutine.Instance.WaitTimeAsyncCallback(9000, () => Console.WriteLine("Callback05"));

            Console.ReadLine();
        }
    }
}
