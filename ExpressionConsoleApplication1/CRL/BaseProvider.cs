using CoreHelper;
using CRL.Attribute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
namespace CRL
{
    /// <summary>
    /// 请实现dbHelper,和单例对象Instance
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class BaseProvider<TModel> where TModel : IModel, new()
    {
        private DBExtend _dbHelper;
        /// <summary>
        /// lockObj
        /// </summary>
        protected static object lockObj = new object();
        /// <summary>
        /// 数据访部对象
        /// 当前实例内只会创建一个,查询除外
        /// </summary>
        protected virtual DBExtend dbHelper
        {
            get
            {
                if (this._dbHelper == null)
                {
                    this._dbHelper = this.GetDbHelper(base.GetType());
                }
                return this._dbHelper;
            }
            set
            {
                this._dbHelper = value;
            }
        }
        /// <summary>
        /// 获取当前对象缓存,不指定条件
        /// 可重写
        /// </summary>
        public virtual List<TModel> AllCache
        {
            get
            {
                return null;
            }
        }
        internal DBExtend GetCurrentDBExtend()
        {
            return this.dbHelper;
        }
        /// <summary>
        /// 设置当前数据访问上下文,不能跨库
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseProvider"></param>
        protected void SetContext<T>(BaseProvider<T> baseProvider) where T : IModel, new()
        {
            DBExtend source = baseProvider.GetCurrentDBExtend();
            if (source.DatabaseName != this.dbHelper.DatabaseName)
            {
                throw new Exception(string.Format("不能跨库设置数据上下文访问,当前:{0} 设置为:{1}", this, baseProvider));
            }
            this.dbHelper = source;
        }
        /// <summary>
        /// 数据访问对象[基本方法]
        /// 按传入的类型
        /// </summary>
        protected DBExtend GetDbHelper<TType>() where TType : class
        {
            return this.GetDbHelper(typeof(TType));
        }
        private DBExtend GetDbHelper()
        {
            return this.GetDbHelper(base.GetType());
        }
        /// <summary>
        /// 数据访问对象[基本方法]
        /// 按指定的类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected DBExtend GetDbHelper(Type type)
        {
            string connString;
            //mssql
            //更改DBConnection目录内数据连接文件
            connString = "Server=.;Database=testDB; User=sa;Password=sasa;";
           
            DBHelper helper = new CoreHelper.SqlHelper(connString);
            DBExtend db = new DBExtend(helper);
            TypeCache.SetDBAdapterCache(typeof(TModel), db._DBAdapter);
            return db;
        }

        public LambdaQuery<TModel> GetLamadaQuery()
        {
            return this.GetLambdaQuery();
        }
        /// <summary>
        /// 创建当前类型查询表达式实列
        /// </summary>
        /// <returns></returns>
        public LambdaQuery<TModel> GetLambdaQuery()
        {
            DBExtend helper = this.GetDbHelper(base.GetType());
            return new LambdaQuery<TModel>(helper, true);
        }
        /// <summary>
        /// 按类型清除当前所有缓存
        /// </summary>
        public void ClearCache()
        {
            Type type = typeof(TModel);
            if (TypeCache.ModelKeyCache.ContainsKey(type))
            {
                //MemoryDataCache.RemoveCache(TypeCache.ModelKeyCache[type]);
            }
        }
        /// <summary>
        /// 从AllCache中进行查询
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public List<TModel> QueryFromAllCache(Predicate<TModel> match)
        {
            return this.AllCache.FindAll(match);
        }
        /// <summary>
        /// 从AllCache中进行查询
        /// 返回一项
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public TModel QueryItemFromAllCache(Predicate<TModel> match)
        {
            List<TModel> list = this.AllCache.FindAll(match);
            TModel result;
            if (list.Count == 0)
            {
                result = default(TModel);
            }
            else
            {
                result = list[0];
            }
            return result;
        }
        /// <summary>
        /// 按类型获取缓存,只能在继承类实现,只能同时有一个类型
        /// 不建议直接调用,请调用AllCache或重写调用
        /// 会按参数进行缓存,慎用
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="expMinute"></param>
        /// <returns></returns>
        protected List<TModel> GetCache(Expression<Func<TModel, bool>> expression = null, int expMinute = 5)
        {
            LambdaQuery<TModel> query = this.GetLamadaQuery();
            if (expression != null)
            {
                query = query.Where(expression);
            }
            return this.GetCache(query, expMinute);
        }
        /// <summary>
        /// 按类型获取缓存,只能在继承类实现,只能同时有一个类型
        /// 不建议直接调用,请调用AllCache或重写调用
        /// </summary>
        /// <param name="query"></param>
        /// <param name="expMinute"></param>
        /// <returns></returns>
        protected List<TModel> GetCache(LambdaQuery<TModel> query, int expMinute = 5)
        {
            Type type = typeof(TModel);
            List<TModel> list = new List<TModel>();
            if (!TypeCache.ModelKeyCache.ContainsKey(type))
            {
                DBExtend helper = this.GetDbHelper(base.GetType());
                string dataCacheKey;
                list = helper.QueryList<TModel>(query, expMinute, out dataCacheKey, false);
                lock (BaseProvider<TModel>.lockObj)
                {
                    if (!TypeCache.ModelKeyCache.ContainsKey(type))
                    {
                        TypeCache.ModelKeyCache.Add(type, dataCacheKey);
                    }
                }
            }
            else
            {
                string dataCacheKey = TypeCache.ModelKeyCache[type];
                //list = MemoryDataCache.GetCacheItem<TModel>(dataCacheKey);
            }
            return list;
        }
        /// <summary>
        /// 创建TABLE[基本方法]
        /// </summary>
        /// <returns></returns>
        public string CreateTable()
        {
            DBExtend helper = this.dbHelper;
            TModel obj = Activator.CreateInstance<TModel>();
            return obj.CreateTable(helper);
        }
        /// <summary>
        /// 创建表索引
        /// </summary>
        public void CreateTableIndex()
        {
            DBExtend helper = this.dbHelper;
            TModel obj = Activator.CreateInstance<TModel>();
            obj.CheckIndexExists(helper);
        }
        /// <summary>
        /// 写日志[基本方法]
        /// </summary>
        /// <param name="message"></param>
        public void Log(string message)
        {
            EventLog.Log(message, "CRL", false);
        }
        /// <summary>
        /// 添加一条记录[基本方法]
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public int Add(TModel p)
        {
            DBExtend helper = this.dbHelper;
            int id = helper.InsertFromObj<TModel>(p);
            FieldAttribute field = TypeCache.GetTable(p.GetType()).PrimaryKey;
            field.SetValue(p, id);
            return id;
        }
        /// <summary>
        /// 按条件取单个记录[基本方法]
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="idDest">是否按主键倒序</param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public TModel QueryItem(Expression<Func<TModel, bool>> expression, bool idDest = true, bool compileSp = false)
        {
            DBExtend helper = this.dbHelper;
            return helper.QueryItem<TModel>(expression, idDest, compileSp);
        }
        /// <summary>
        /// 按主键删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int DeleteById(int id)
        {
            return this.Delete(id);
        }
        /// <summary>
        /// 按主键删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Delete(int id)
        {
            DBExtend helper = this.dbHelper;
            FieldAttribute filed = TypeCache.GetTable(typeof(TModel)).PrimaryKey;
            string where = string.Format("{0}=@{0}", filed.Name);
            helper.AddParam(filed.Name, id);
            return helper.Delete<TModel>(where);
        }
        /// <summary>
        /// 按条件删除[基本方法]
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public int Delete(Expression<Func<TModel, bool>> expression)
        {
            DBExtend helper = this.dbHelper;
            return helper.Delete<TModel>(expression);
        }
        /// <summary>
        /// 返回全部结果[基本方法]
        /// </summary>
        /// <returns></returns>
        public List<TModel> QueryList()
        {
            DBExtend helper = this.GetDbHelper(base.GetType());
            return helper.QueryList<TModel>(null, false);
        }
        /// <summary>
        /// 指定条件查询[基本方法]
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public List<TModel> QueryList(Expression<Func<TModel, bool>> expression, bool compileSp = false)
        {
            DBExtend helper = this.GetDbHelper(base.GetType());
            return helper.QueryList<TModel>(expression, compileSp);
        }
        /// <summary>
        /// 指定条件查询[基本方法]
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cacheTime"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public List<TModel> QueryList(LambdaQuery<TModel> query, int cacheTime = 0, bool compileSp = false)
        {
            DBExtend helper = this.GetDbHelper(base.GetType());
            return helper.QueryList<TModel>(query, cacheTime, compileSp);
        }
        /// <summary>
        /// 自带存储过程分页查询[基本方法]
        /// </summary>
        /// <param name="parame"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<TModel> QueryListByPage(ParameCollection parame, out int count)
        {
            DBExtend helper = this.dbHelper;
            return helper.QueryListByPage<TModel>(parame, out count);
        }
        /// <summary>
        /// 动态存储过程分页
        /// </summary>
        /// <param name="query"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<TModel> AutoSpPage(LambdaQuery<TModel> query, out int count)
        {
            DBExtend helper = this.dbHelper;
            return helper.AutoSpPage<TModel>(query, out count);
        }
        /// <summary>
        /// 按ID整体更新[基本方法]
        /// </summary>
        /// <param name="item"></param>
        public void UpdateById(TModel item)
        {
            DBExtend helper = this.dbHelper;
            helper.UpdateById<TModel>(item);
        }
        /// <summary>
        /// 按对象差异更新,对象需由查询创建
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int Update(TModel item)
        {
            DBExtend helper = this.dbHelper;
            return helper.Update<TModel>(item);
        }
        /// <summary>
        /// 指定条件并按对象差异更新
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(Expression<Func<TModel, bool>> expression, TModel model)
        {
            DBExtend helper = this.dbHelper;
            return helper.Update<TModel>(expression, model);
        }
        /// <summary>
        /// 指定条件和参数进行更新[基本方法]
        /// </summary>
        /// <param name="expression">条件</param>
        /// <param name="setValue">值</param>
        /// <returns></returns>
        public int Update(Expression<Func<TModel, bool>> expression, ParameCollection setValue)
        {
            DBExtend helper = this.dbHelper;
            return helper.Update<TModel>(expression, setValue);
        }
        /// <summary>
        /// 批量插入[基本方法]
        /// </summary>
        /// <param name="list"></param>
        /// <param name="keepIdentity">是否保持自增主键</param>
        public void BatchInsert(List<TModel> list, bool keepIdentity = false)
        {
            DBExtend helper = this.dbHelper;
            helper.BatchInsert<TModel>(list, keepIdentity);
        }
        /// <summary>
        /// 指定格式化查询列表[基本方法]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parame"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        protected List<T> ExecListWithFormat<T>(string sql, ParameCollection parame, params Type[] types) where T : class, new()
        {
            DBExtend helper = this.dbHelper;
            foreach (KeyValuePair<string, object> p in parame)
            {
                helper.AddParam(p.Key, p.Value);
            }
            return helper.ExecList<T>(sql, types);
        }
        /// <summary>
        /// 指定格式化更新[基本方法]
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parame"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        protected int ExecuteWithFormat(string sql, ParameCollection parame, params Type[] types)
        {
            DBExtend helper = this.dbHelper;
            foreach (KeyValuePair<string, object> p in parame)
            {
                helper.AddParam(p.Key, p.Value);
            }
            return helper.Execute(sql, types);
        }
        /// <summary>
        /// 指定格式化返回单个结果[基本方法]
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parame"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        protected T ExecScalarWithFormat<T>(string sql, ParameCollection parame, params Type[] types)
        {
            DBExtend helper = this.dbHelper;
            foreach (KeyValuePair<string, object> p in parame)
            {
                helper.AddParam(p.Key, p.Value);
            }
            return helper.ExecScalar<T>(sql, types);
        }
        /// <summary>
        /// 执行存储过程返回结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <returns></returns>
        protected List<T> RunList<T>(string sp) where T : class, new()
        {
            DBExtend helper = this.dbHelper;
            return helper.RunList<T>(sp);
        }
        /// <summary>
        /// 对GROUP进行分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">查询 如:ProductReceiptDetail with (nolock) group by styleid</param>
        /// <param name="fields">查询字段 如:styleid,sum(num) as total</param>
        /// <param name="rowOver">行排序 如:sum(num) desc</param>
        /// <param name="sortfield">排序字段 如:total desc</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="pageIndex">页索引</param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected List<T> GroupByPage<T>(string query, string fields, string rowOver, string sortfield, int pageSize, int pageIndex, out int count) where T : class, new()
        {
            count = 0;
            DBExtend helper = this.dbHelper;
            return helper.AutoSpGroupPage<T>(query, fields, rowOver, sortfield, pageSize, pageIndex, out count);
        }
        /// <summary>
        /// 导出为XML
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public string ExportToXml(Expression<Func<TModel, bool>> expression)
        {
            List<TModel> list = this.QueryList(expression, false);
            /*return SerializeHelper.XmlSerialize(list, Encoding.UTF8);*/
            return "";
        }
        /// <summary>
        /// 导出到文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public int ExportToFile(string file, Expression<Func<TModel, bool>> expression)
        {
            List<TModel> list = this.QueryList(expression, false);
            File.Delete(file);
            //SerializeHelper.XmlSerialize(list, file);
            return list.Count;
        }
        /// <summary>
        /// 从XML导入
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="delExpression">要删除的数据</param>
        /// <param name="keepIdentity">是否保留自增主键</param>
        /// <returns></returns>
        public int ImportFromXml(string xml, Expression<Func<TModel, bool>> delExpression, bool keepIdentity = false)
        {
            /*List<TModel> obj = SerializeHelper.XmlDeserialize<List<TModel>>(xml, Encoding.UTF8);
            this.Delete(delExpression);
            this.BatchInsert(obj, keepIdentity);
            return obj.Count;*/

            return 0;
        }
        /// <summary>
        /// 从XML文件导入
        /// </summary>
        /// <param name="file"></param>
        /// <param name="delExpression">要删除的数据</param>
        /// <param name="keepIdentity">是否保留自增主键</param>
        /// <returns></returns>
        public int ImportFromFile(string file, Expression<Func<TModel, bool>> delExpression, bool keepIdentity = false)
        {
            /*List<TModel> obj = SerializeHelper.XmlDeserialize<List<TModel>>(file);
            int result;
            if (obj.Count == 0)
            {
                result = 0;
            }
            else
            {
                this.Delete(delExpression);
                this.BatchInsert(obj, keepIdentity);
                result = obj.Count;
            }*/
            return 0;
        }
        /// <summary>
        /// 统计
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public int Count(Expression<Func<TModel, bool>> expression, bool compileSp = false)
        {
            DBExtend helper = this.GetDbHelper(base.GetType());
            return helper.Count<TModel>(expression, compileSp);
        }
        /// <summary>
        /// sum 按表达式指定字段
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="expression"></param>
        /// <param name="field"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public TType Sum<TType>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> field, bool compileSp = false)
        {
            DBExtend helper = this.GetDbHelper(base.GetType());
            return helper.Sum<TType, TModel>(expression, field, compileSp);
        }
        /// <summary>
        /// sum 通过字符串指定字段
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="expression"></param>
        /// <param name="field"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public TType Sum<TType>(Expression<Func<TModel, bool>> expression, string field, bool compileSp = false)
        {
            DBExtend helper = this.GetDbHelper(base.GetType());
            return helper.Sum<TType, TModel>(expression, field, compileSp);
        }

    }
}
