using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HongBaoConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var arg = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(arg))
                { continue; }

                if (arg.ToLower() == "y")
                {
                    Run();
                    //break;
                }

                Console.WriteLine(arg);
                Console.WriteLine("是否需要退出？y&n");
            }

        }

        public static void Run()
        {
            const double amount = 500;
            const int count = 10;
            var result = HongBaoCore(amount, count);

            foreach (var hbAmount in result.OrderBy(r => r))
            {
                Console.Write(hbAmount + "\r\n");
            }
            Console.Write("\r\n");
            /* for (int i = 0; i < count; i++)
             {
                 Random rd = new Random();
                 var index = rd.Next(0, count - i);
                 Console.Write(result[index] + "\r\n");
                 result.Remove(result[index]);
             }*/
        }

        public static List<double> HongBaoCore(double amount, int count)
        {
            List<double> zjList = new List<double>();

            for (int i = 1; i <= count; i++)
            {
                double useAmount = zjList.Sum();
                var balance = amount - useAmount;

                if (i < count)
                {
                    double zjAmount = balance / (count - i);

                    Random rd1 = new Random(Guid.NewGuid().GetHashCode());

                    var result1 = rd1.Next(0, Convert.ToInt32(Math.Ceiling(zjAmount)));

                    Random rd2 = new Random(Guid.NewGuid().GetHashCode());
                    var result2 = Math.Round(rd2.NextDouble(), 2);
                    zjList.Add(result1 + result2);
                }
                else
                {
                    zjList.Add(Math.Round(amount - useAmount, 2));
                }
            }

            return zjList;
        }
    }
}
