using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace MaoPaoConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            TestOne();
            int[] arr = new[] { 25, 23, 27, 22, 20 };

            Console(arr);

            int temp = 0;
            for (int i = 0; i < arr.Length - 1; i++)
            {
                for (int j = 0; j < arr.Length - 1 - i; j++)
                {
                    if (arr[j] > arr[j + 1])
                    {
                      
                        arr[j + 1] = arr[j + 1] ^ arr[j];
                        arr[j] = arr[j] ^ arr[j + 1];
                        arr[j + 1] = arr[j + 1] ^ arr[j];

                        //temp = arr[j + 1];
                        //arr[j + 1] = arr[j];
                        //arr[j] = temp;
                    }
                }
                Console(arr);
            }

            List<int> dd = arr.ToList();
            dd.ForEach(print);


            //执行一个不带返回参数的委托方法
            Action<int> match5 = i => Console(new[] {i});
            Action<int> match6 = delegate(int i) { Console(new[] { i }); };

            // 执行一个带bool返回值的委托方法
            Predicate<int> match0 = i => i > 0;
            Predicate<int> match10 = delegate(int i) { return i > 10; };

            //执行一个带返回值的委托方法
            Func<int, string, bool> match3 = (i, s) =>
            {
                return i.ToString() == s;
            };
            Func<int, string, bool> match2 = new Func<int, string, bool>(delegate(int i, string s)
            {
                return (i.ToString() == s ? true : false);
            });

            //如果Func的返回类型定义为bool值，那么以下两个表达式是相等的
            Predicate<int> match = Testdelegate;
            Func<int, bool> match1 = Testdelegate;


            var result = match3(1, "2");

            var ff = dd.FindAll(match);
            dd.Where(d => d == 0);
            var sss= dd.GroupBy(d => d);
            //Func<int, string, string> dd = (i, s) => ;

            List<UserInfo> dsf = new List<UserInfo>();
            var sasdfss = dsf.GroupBy(d => d);

                TestPlus();

            System.Console.ReadLine();
        }

        public static bool Testdelegate(int i)
        {
            return i == 1;
        }

        public static void TestPlus()
        {
            int i = 0;
            System.Console.WriteLine("++i:" + (++i));

            int b = 0;
            System.Console.WriteLine("b++:" + (b++));
            System.Console.WriteLine("b++之后:" + b);

            System.Console.WriteLine("Environment.UserName;:" + Environment.UserName);

            //MessageBox.Show(Environment.UserName);

            string msg = "你好啊，你叫什么名字啊，是从哪里来的啊，多大了啊";

            var dd = Regex.Split(msg, ",");

        }

        public static void print(int i)
        {
            System.Console.WriteLine(i.ToString());
        }

        private static void Console(int[] arr)
        {
            System.Console.WriteLine("");
            System.Console.WriteLine("============第一轮排序===============");
            for (int i = 0; i <= arr.Length - 1; i++)
            {
                System.Console.Write(arr[i] + "    ");
            }
        }

        private delegate void TestMethod(string dd);

        public static void TestOne()
        {
            TestMethod p = delegate(string dd)
            {
                var a = "dd";
                var b = "ssfa";

                System.Console.WriteLine(a + dd + b);
            };

            p("hello wolrd");
        }
    }

    public class UserInfo
    {
        public int Id { get; set; }
    }
}
