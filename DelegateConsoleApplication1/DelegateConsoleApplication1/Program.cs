using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DelegateConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            TestOne();

            List<UserInfo> list = new List<UserInfo>()
            {
             new UserInfo()
             {
                 Id = 1001,
                 Name = "张三"
             },   
             new UserInfo()
             {
                 Id = 1002,
                 Name = "李四"
             },
             new UserInfo()
             {
                 Id = 1003,
                 Name = "李四"
             }
            };
            //执行一个不带返回参数的委托方法
            Action<int> match5 = i => Console(new[] { i });
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

            Func<UserInfo, string> keySelector = info => info.Name;

            var sasdfss = list.GroupBy(u => u.Name);

            Expression<Func<UserInfo, bool>> prExpression = info => info.Name.Contains("A");

            var res3 =
              from st in list
              group st by new { st.Name, st.Id };

            Debug.Write("\n\n查询结果变量的类型：" + res3.GetType().Name + "\n");
            Debug.Write("\n分别输出各分组的信息：\n");
            foreach (var g in res3)
            {
                Debug.WriteLine(g.Key);
                foreach (var item in g)
                {
                    Debug.WriteLine("姓名：" + item.Name);
                    Debug.WriteLine("成绩：" + item.Id);
                }
            }

            Trace.Write("11111111111111111111");

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

        public string Name { get; set; }
    }
}
