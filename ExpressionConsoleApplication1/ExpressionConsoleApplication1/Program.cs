using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //Expression<Func<int, bool>> exprTree = num => num < 5;
            /* Expression<Func<UserEntity, bool>> exprTree = entity => entity.Id == 1001 && entity.UserName.Contains("abc") && entity.Age > 19;

             // Decompose the expression tree.
             ParameterExpression param = (ParameterExpression)exprTree.Parameters[0];
             BinaryExpression operation = (BinaryExpression)exprTree.Body;
             ParameterExpression left = (ParameterExpression)operation.Left;
             ConstantExpression right = (ConstantExpression)operation.Right;

             Console.WriteLine("Decomposed expression: {0} => {1} {2} {3}",
                               param.Name, left.Name, operation.NodeType, right.Value);*/
            TestExpression();
            Console.Read();
        }

        private static void TestExpression()
        {
            List<UserEntity> sList = new List<UserEntity>()
            {

            };

            var result = sList.Select(u => new
            {
                u.Age,
                u.Id
            });

            result.OrderBy(u => new { u.Age, u.Id });


            var query = ProductDataManage.Instance.GetLamadaQuery();

            query = query.Where(b => b.Id < 700 && b.UserName == "张三" && b.Age > 4);//查询条件
            query.Where(b => b.UserName.lik("w2") || b.UserName.Contains("sss"));

            //添加引用CRL
            query = query.Select(b => b.Age > 19);//选择查询的字段
            int? n2 = 10;
            query = query.Top(10);//取多少条
            /* //query = query.Where(b => b.Id < 700 && b.InterFaceUser == "USER1");//查询条件
             //query.Where(b => b.ProductName.Contains("w2") || b.ProductName.Contains("sss"));
             string s = "ssss";
             int n = 10;
             classA a = new classA() { Name = "ffffff" };
             query.Where(b => b.ProductName == s && b.Id > n || b.ProductName.Contains("sss"));
             query.Where(b => b.Id == n2.Value);
             //query.Where(b => b.ProductName == a.Name);
             //query.Where(b => b.ProductName == a.Method());
             query = query.OrderBy(b => b.Id, true);//排序条件*/
            // var list = Code.ProductDataManage.Instance.QueryList(query);
        }
    }

    /// <summary>
    /// ProductData业务处理类
    /// 这里实现处理逻辑
    /// </summary>
    public class ProductDataManage : CRL.BaseProvider<UserEntity>
    {
        /// <summary>
        /// 实现会话实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseProvider"></param>
        /// <returns></returns>
        public static ProductDataManage ContextInstance<T>(CRL.BaseProvider<T> baseProvider) where T : CRL.IModel, new()
        {
            var instance = Instance;
            instance.SetContext(baseProvider);
            return instance;
        }

        /// <summary>
        /// 实例访问入口
        /// </summary>
        public static ProductDataManage Instance
        {
            get { return new ProductDataManage(); }
        }
    }

    public class UserEntity : CRL.IModelBase
    {
        public long Id { get; set; }

        public string UserName { get; set; }

        public int Age { get; set; }
    }
}
