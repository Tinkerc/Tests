using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            /* ConsoleAsync1();
             stopwatch.Stop();
             Console.WriteLine("同步方法用时：" + stopwatch.ElapsedMilliseconds);
             stopwatch.Reset();
             stopwatch.Start();*/
            Console.WriteLine("我是主线程：Thread Id {0}", Thread.CurrentThread.ManagedThreadId);

            Console.WriteLine("异步方法100000000");
            ConsoleAsync(100000000);

            Console.WriteLine("执行另外一个方法");
            ConsoleAsync(3000);

            //Console.WriteLine("同步方法100000000：" + Calculate(100000000));
            stopwatch.Stop();
            Console.WriteLine("异步方法用时：" + stopwatch.ElapsedMilliseconds);

            Console.Read();
        }

        private static async void ConsoleAsync(int arraySize)
        {
            Console.WriteLine("我是ConsoleAsync线程：Thread Id {0}", Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine(arraySize + "异步方法开始");
            Console.WriteLine("Result:" + await SumAsync(arraySize));
            Console.WriteLine(arraySize + "异步方法结束");
        }
        private static async Task<string> SumAsync(int arraySize)
        {
            //HttpClient client = new HttpClient();
            //Task<string> getStringTask = client.GetStringAsync("http://www.baidu.com");
            //Console.WriteLine(DateTime.Now.Millisecond + " 异步 " + (await getStringTask).Length);


            Task<string> getStringTask = new Task<string>(() => Calculate(arraySize));
            getStringTask.Start();
            //getStringTask.Wait();

            return await getStringTask;
        }

        private static string Calculate(int arraySize)
        {
           // Thread.Sleep(3000);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var data = new int[arraySize];
            var r = new Random();
            for (int i = 0; i < arraySize; i++)
            {
                data[i] = r.Next(40);
            }

            var sum = data.AsParallel().Where(d => d < 20).Sum();

            Console.WriteLine("我是Calculate线程：Thread Id {0}", Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine("用时：" + stopwatch.ElapsedMilliseconds);

            return arraySize + "总和：" + sum;
        }
    }
}
