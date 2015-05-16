using CoreHelper;
using CRL.Attribute;
using CRL.DBAdapter;
using CRL.Dynamic;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
namespace CRL
{
    /// <summary>
    /// 对象数据访问
    /// </summary>
    public class DBExtend
    {
        private enum TranStatus
        {
            未开始,
            已开始
        }
        private DBHelper helper;
        private static object lockObj = new object();
        private Dictionary<string, object> ParamsBackup;
        private Dictionary<string, object> OutParamsBackup;
        private DBExtend.TranStatus currentTranStatus = DBExtend.TranStatus.未开始;
        private static Dictionary<string, int> spCahe = new Dictionary<string, int>();
        internal string DatabaseName
        {
            get
            {
                return this.helper.DatabaseName;
            }
        }
        /// <summary>
        /// 当前数据库适配器
        /// </summary>
        internal DBAdapterBase _DBAdapter
        {
            get
            {
                return DBAdapterBase.GetDBAdapterBase(this.helper.CurrentDBType);
            }
        }
        /// <summary>
        /// 构造DBExtend
        /// </summary>
        /// <param name="_helper"></param>
        public DBExtend(DBHelper _helper)
        {
            if (_helper == null)
            {
                throw new Exception("数据访问对象未实例化,请实现CRL.SettingConfig.GetDbAccess");
            }
            this.helper = _helper;
        }
        /// <summary>
        /// 增加参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddParam(string name, object value)
        {
            value = ObjectConvert.SetNullValue(value, null);
            this.helper.AddParam(name, value);
        }
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetParam(string name, object value)
        {
            value = ObjectConvert.SetNullValue(value, null);
            this.helper.SetParam(name, value);
        }
        /// <summary>
        /// 增加输出参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value">对应类型任意值</param>
        public void AddOutParam(string name, object value = null)
        {
            this.helper.AddOutParam(name, value);
        }
        /// <summary>
        /// 获取存储过程return的值
        /// </summary>
        /// <returns></returns>
        public int GetReturnValue()
        {
            return this.helper.GetReturnValue();
        }
        /// <summary>
        /// 获取OUTPUT的值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetOutParam(string name)
        {
            return this.helper.GetOutParam(name);
        }
        public T GetOutParam<T>(string name)
        {
            object obj = this.helper.GetOutParam(name);
            return ObjectConvert.ConvertObject<T>(obj);
        }
        private void ClearParame()
        {
            this.helper.ClearParams();
        }
        /// <summary>
        /// 备份参数
        /// </summary>
        private void BackupParams()
        {
            this.ParamsBackup = new Dictionary<string, object>(this.helper.Params);
            this.OutParamsBackup = new Dictionary<string, object>(this.helper.OutParams);
            this.helper.ClearParams();
        }
        /// <summary>
        /// 还原参数
        /// </summary>
        private void RecoveryParams()
        {
            this.helper.Params = new Dictionary<string, object>(this.ParamsBackup);
            this.helper.OutParams = new Dictionary<string, object>(this.OutParamsBackup);
            this.ParamsBackup = null;
            this.OutParamsBackup = null;
        }
        private void CheckData(IModel obj)
        {
            List<FieldAttribute> types = TypeCache.GetProperties(obj.GetType(), true);
            string msg;
            foreach (FieldAttribute p in types)
            {
                if (p.PropertyType == typeof(string))
                {
                    string value = string.Concat(p.GetValue(obj));
                    if (p.NotNull && string.IsNullOrEmpty(value))
                    {
                        msg = string.Format("对象{0}属性{1}值不能为空", obj.GetType(), p.Name);
                        throw new Exception(msg);
                    }
                    if (value.Length > p.Length && p.Length < 3000)
                    {
                        msg = string.Format("对象{0}属性{1}长度超过了设定值{2}", obj.GetType(), p.Name, p.Length);
                        throw new Exception(msg);
                    }
                }
            }
            msg = obj.CheckData();
            if (!string.IsNullOrEmpty(msg))
            {
                msg = string.Format("数据校验证失败,在类型{0} {1} 请核对校验规则", obj.GetType(), msg);
                throw new Exception(msg);
            }
        }
        /// <summary>
        /// 按表达式更新缓存中项
        /// 当前类型有缓存时才会进行查询
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="expression"></param>
        private void UpdateCacheItem<TItem>(Expression<Func<TItem, bool>> expression, ParameCollection c) where TItem : IModel, new()
        {
            
        }
        /// <summary>
        /// 更新缓存中的一项
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="newObj"></param>
        /// <param name="c"></param>
        private void UpdateCacheItem<TItem>(TItem newObj, ParameCollection c) where TItem : IModel
        {
            
        }
        /// <summary>
        /// 格式化为更新值查询
        /// </summary>
        /// <param name="setValue"></param>
        /// <returns></returns>
        private string ForamtSetValue<T>(ParameCollection setValue) where T : IModel
        {
            string tableName = TypeCache.GetTableName(typeof(T));
            string setString = "";
            foreach (KeyValuePair<string, object> pair in setValue)
            {
                string name = pair.Key;
                object value = pair.Value;
                value = ObjectConvert.SetNullValue(value, null);
                if (name.StartsWith("$"))
                {
                    name = name.Substring(1, name.Length - 1);
                    setString += string.Format(" {0}={1},", name, value);
                }
                else
                {
                    setString += string.Format(" {0}=@{0},", name);
                    this.helper.AddParam(name, value);
                }
            }
            setString = setString.Substring(0, setString.Length - 1);
            return setString;
        }
        /// <summary>
        /// 通过关键类型,格式化SQL语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="args">格式化SQL语句的关键类型</param>
        /// <returns></returns>
        private string AutoFormat(string sql, params Type[] args)
        {
            string result;
            if (args == null)
            {
                result = sql;
            }
            else if (args.Length == 0)
            {
                result = sql;
            }
            else
            {
                Regex r = new Regex("\\$(\\w+)", RegexOptions.IgnoreCase);
                List<string> pars = new List<string>();
                Match i = r.Match(sql);
                while (i.Success)
                {
                    string par = i.Groups[1].ToString();
                    if (!pars.Contains(par))
                    {
                        pars.Add(par);
                    }
                    i = i.NextMatch();
                }
                foreach (string par in pars)
                {
                    for (int j = 0; j < args.Length; j++)
                    {
                        Type type = args[j];
                        string tableName = TypeCache.GetTableName(type);
                        string fullTypeName = DBExtend.GetTypeFullName(type);
                        
                        if (fullTypeName.IndexOf("." + par + ".") > -1)
                        {
                            sql = sql.Replace("$" + par, tableName);
                        }
                    }
                }
                if (sql.IndexOf("$") > -1)
                {
                    throw new Exception("格式化SQL语句时发生错误,表名未被替换:" + sql);
                }
                result = sql;
            }
            return result;
        }
        private static string GetTypeFullName(Type type)
        {
            string str = "";
            while (type != typeof(IModel))
            {
                str = str + "." + type.FullName + ".;";
                type = type.BaseType;
            }
            return str;
        }
        /// <summary>
        /// 指定替换对象查询,并返回对象列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public List<T> ExecList<T>(string sql, params Type[] types) where T : class, new()
        {
            sql = this._DBAdapter.SqlFormat(sql);
            DbDataReader reader = this.GetDataReader(sql, types);
            return ObjectConvert.DataReaderToList<T>(reader, false, null);
        }
        private DbDataReader GetDataReader(string sql, params Type[] types)
        {
            sql = this.AutoFormat(sql, types);
            sql = this._DBAdapter.SqlFormat(sql);
            DbDataReader reader = this.helper.ExecDataReader(sql);
            this.ClearParame();
            return reader;
        }
        public Dictionary<TKey, TValue> ExecDictionary<TKey, TValue>(string sql, params Type[] types)
        {
            DbDataReader reader = this.GetDataReader(sql, types);
            return ObjectConvert.DataReadToDictionary<TKey, TValue>(reader);
        }
        /// <summary>
        /// 指定替换对象更新
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public int Execute(string sql, params Type[] types)
        {
            sql = this.AutoFormat(sql, types);
            sql = this._DBAdapter.SqlFormat(sql);
            int count = this.helper.Execute(sql);
            this.ClearParame();
            return count;
        }
        /// <summary>
        /// 指定替换对象返回单个结果
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="types">格式化SQL语句的关键类型</param>
        /// <returns></returns>
        public object ExecScalar(string sql, params Type[] types)
        {
            sql = this.AutoFormat(sql, types);
            sql = this._DBAdapter.SqlFormat(sql);
            object obj = this.helper.ExecScalar(sql);
            this.ClearParame();
            return obj;
        }
        /// <summary>
        /// 指定替换对象返回单个结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public T ExecScalar<T>(string sql, params Type[] types)
        {
            sql = this._DBAdapter.SqlFormat(sql);
            object obj = this.ExecScalar(sql, types);
            return ObjectConvert.ConvertObject<T>(obj);
        }
        /// <summary>
        /// 返回首个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public T ExecObject<T>(string sql, params Type[] types) where T : class, new()
        {
            List<T> list = this.ExecList<T>(sql, types);
            T result;
            if (list.Count == 0)
            {
                result = default(T);
            }
            else
            {
                result = list[0];
            }
            return result;
        }
        /// <summary>
        /// 执行存储过程返回结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <returns></returns>
        public List<T> RunList<T>(string sp) where T : class, new()
        {
            DbDataReader reader = this.helper.RunDataReader(sp);
            this.ClearParame();
            return ObjectConvert.DataReaderToList<T>(reader, false, null);
        }
        /// <summary>
        /// 执行一个存储过程
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public int Run(string sp)
        {
            int count = this.helper.Run(sp);
            this.ClearParame();
            return count;
        }
        /// <summary>
        /// 返回首个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <returns></returns>
        public T RunObject<T>(string sp) where T : class, new()
        {
            List<T> list = this.RunList<T>(sp);
            T result;
            if (list.Count == 0)
            {
                result = default(T);
            }
            else
            {
                result = list[0];
            }
            return result;
        }
        /// <summary>
        /// 执行存储过程并返回结果
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public object RunScalar(string sp)
        {
            object obj = this.helper.RunScalar(sp);
            this.ClearParame();
            return obj;
        }
        /// <summary>
        /// 开始物务
        /// </summary>
        public void BeginTran()
        {
            if (this.currentTranStatus != DBExtend.TranStatus.未开始)
            {
                throw new Exception("事务开始失败,已有未完成的事务");
            }
            this.helper.BeginTran();
            this.currentTranStatus = DBExtend.TranStatus.已开始;
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        public void RollbackTran()
        {
            if (this.currentTranStatus != DBExtend.TranStatus.已开始)
            {
                throw new Exception("事务回滚失败,没有需要回滚的事务");
            }
            this.helper.RollbackTran();
            this.currentTranStatus = DBExtend.TranStatus.未开始;
        }
        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTran()
        {
            if (this.currentTranStatus != DBExtend.TranStatus.已开始)
            {
                throw new Exception("事务提交失败,没有需要提交的事务");
            }
            this.helper.CommitTran();
            this.currentTranStatus = DBExtend.TranStatus.未开始;
        }
        internal void CheckTableCreated<T>() where T : IModel, new()
        {
            this.CheckTableCreated(typeof(T));
        }
        /// <summary>
        /// 检查表是否被创建
        /// </summary>
        internal void CheckTableCreated(Type type)
        {
            
        }
        /// <summary>
        /// 对表进行分页并编译成存储过程
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="query1"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<TItem> AutoSpPage<TItem>(LambdaQuery<TItem> query1, out int count) where TItem : IModel, new()
        {
            this.CheckTableCreated<TItem>();
            string fields = query1.GetQueryFieldString((FieldAttribute b) => b.Length > 500 || b.PropertyType == typeof(byte[]));
            string rowOver = query1.QueryOrderBy;
            if (string.IsNullOrEmpty(rowOver))
            {
                rowOver = "t1.id desc";
            }
            string orderBy = Regex.Replace(rowOver, "t\\d\\.", "t.");
            string query2 = query1.GetQueryConditions();
            ParameCollection parame = query1.QueryParames;
            query2 = this._DBAdapter.SqlFormat(query2);
            count = 0;
            foreach (KeyValuePair<string, object> i in parame)
            {
                this.AddParam(i.Key, i.Value);
            }
            this.AddParam("pageIndex", query1.PageIndex);
            this.AddParam("pageSize", query1.PageSize);
            this.AddOutParam("count", 1);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("fields", fields);
            dic.Add("sort", orderBy);
            dic.Add("rowOver", rowOver);
            string sp = this.CompileSqlToSp(this._DBAdapter.TemplatePage, query2, dic);
           
            List<TItem> list = new List<TItem>();
            this.ClearParame();
            return list;
        }
        /// <summary>
        /// 对GROUP进行分页
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="query">查询 如:ProductReceiptDetail with (nolock) group by styleid</param>
        /// <param name="fields">查询字段 如:styleid,sum(num) as total</param>
        /// <param name="rowOver">行排序 如:sum(num) desc</param>
        /// <param name="sort">排序字段 如:total desc</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="pageIndex">页索引</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<TItem> AutoSpGroupPage<TItem>(string query, string fields, string rowOver, string sort, int pageSize, int pageIndex, out int count) where TItem : class, new()
        {
            query = this._DBAdapter.SqlFormat(query);
            count = 0;
            this.AddParam("pageIndex", pageIndex);
            this.AddParam("pageSize", pageSize);
            this.helper.AddOutParam("count", 1);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("fields", fields);
            dic.Add("rowOver", rowOver);
            dic.Add("sort", sort);
            string sp = this.CompileSqlToSp(this._DBAdapter.TemplateGroupPage, query, dic);
            DbDataReader reader;
            try
            {
                reader = this.helper.RunDataReader(sp);
                count = this.GetOutParam<int>("count");
            }
            catch (Exception ero)
            {
                if (!ero.Message.Contains("找不到存储过程"))
                {
                    throw ero;
                }
                DBExtend.spCahe.Remove(sp);
                sp = this.CompileSqlToSp(this._DBAdapter.TemplateGroupPage, query, dic);
                reader = this.helper.RunDataReader(sp);
                count = this.GetOutParam<int>("count");
            }
            this.ClearParame();
            return ObjectConvert.DataReaderToList<TItem>(reader, false, null);
        }
        /// <summary>
        /// 将SQL语句编译成储存过程
        /// </summary>
        /// <param name="template">模版</param>
        /// <param name="sql">语桀犬吠尧</param>
        /// <param name="parames">模版替换参数</param>
        /// <returns></returns>
        private string CompileSqlToSp(string template, string sql, Dictionary<string, string> parames = null)
        {
            sql = this._DBAdapter.SqlFormat(sql);
            lock (DBExtend.lockObj)
            {
                if (DBExtend.spCahe.Count == 0)
                {
                    this.BackupParams();
                    DBExtend.spCahe = this.ExecDictionary<string, int>(this._DBAdapter.GetAllSPSql(this.helper), new Type[0]);
                    this.RecoveryParams();
                }
            }
            string fields = "";
            if (parames != null)
            {
                if (parames.ContainsKey("fields"))
                {
                    fields = parames["fields"];
                }
                if (parames.ContainsKey("sort"))
                {
                    fields = fields + "_" + parames["sort"];
                }
            }
            string sp = StringHelper.EncryptMD5(fields + "_" + sql.Trim(), null);
            sp = "ZautoSp_" + sp.Substring(8, 16);
            if (!DBExtend.spCahe.ContainsKey(sp))
            {
                sql = this.helper.FormatWithNolock(sql);
                string spScript = Base.SqlToProcedure(template, this.helper, sql, sp, parames);
                try
                {
                    this.BackupParams();
                    this.helper.Execute(spScript);
                    this.RecoveryParams();
                    lock (DBExtend.lockObj)
                    {
                        if (!DBExtend.spCahe.ContainsKey(sp))
                        {
                            DBExtend.spCahe.Add(sp, 0);
                        }
                    }
                    string log = string.Format("创建存储过程:{0}\r\n{1}", sp, spScript);
                    EventLog.Log(log, "sqlToSp", false);
                }
                catch (Exception ero)
                {
                    this.RecoveryParams();
                    throw new Exception("动态创建存储过程时发生错误:" + ero.Message);
                }
            }
            return sp;
        }
        public T AutoSpQueryItem<T>(string sql, params Type[] types) where T : class, new()
        {
            List<T> list = this.AutoSpQuery<T>(sql, types);
            return list.FirstOrDefault<T>();
        }
        /// <summary>
        /// 将查询自动转化为存储过程执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public List<T> AutoSpQuery<T>(string sql, params Type[] types) where T : class, new()
        {
            DbDataReader reader = this.AutoSpQuery(sql, types);
            return ObjectConvert.DataReaderToList<T>(reader, false, null);
        }
        private DbDataReader AutoSpQuery(string sql, params Type[] types)
        {
            sql = this.AutoFormat(sql, types);
            sql = this._DBAdapter.SqlFormat(sql);
            string sp = this.CompileSqlToSp(this._DBAdapter.TemplateSp, sql, null);
            DbDataReader reader;
            try
            {
                reader = this.helper.RunDataReader(sp);
            }
            catch (Exception ero)
            {
                if (!ero.Message.Contains("找不到存储过程"))
                {
                    throw ero;
                }
                DBExtend.spCahe.Remove(sp);
                sp = this.CompileSqlToSp(this._DBAdapter.TemplateSp, sql, null);
                reader = this.helper.RunDataReader(sp);
            }
            this.ClearParame();
            return reader;
        }
        /// <summary>
        /// 将查询自动转化为存储过程执行
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public Dictionary<TKey, TValue> AutoSpQuery<TKey, TValue>(string sql, params Type[] types)
        {
            sql = this.AutoFormat(sql, types);
            sql = this._DBAdapter.SqlFormat(sql);
            string sp = this.CompileSqlToSp(this._DBAdapter.TemplateSp, sql, null);
            DbDataReader reader = null;
            try
            {
                reader = this.helper.RunDataReader(sp);
            }
            catch (Exception ero)
            {
                if (!ero.Message.Contains("找不到存储过程"))
                {
                    throw ero;
                }
                DBExtend.spCahe.Remove(sp);
                sp = this.CompileSqlToSp(this._DBAdapter.TemplateSp, sql, null);
                reader = this.helper.RunDataReader(sp);
            }
            this.ClearParame();
            return ObjectConvert.DataReadToDictionary<TKey, TValue>(reader);
        }
        /// <summary>
        /// 将更新自动转化为存储过程执行
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int AutoSpUpdate(string sql, params Type[] types)
        {
            sql = this.AutoFormat(sql, types);
            sql = this._DBAdapter.SqlFormat(sql);
            string sp = this.CompileSqlToSp(this._DBAdapter.TemplateSp, sql, null);
            int i;
            try
            {
                i = this.helper.Run(sp);
            }
            catch (Exception ero)
            {
                if (ero.Message.Contains("找不到存储过程"))
                {
                    DBExtend.spCahe.Remove(sp);
                    sp = this.CompileSqlToSp(this._DBAdapter.TemplateSp, sql, null);
                    i = this.helper.Run(sp);
                }
                throw ero;
            }
            this.ClearParame();
            return i;
        }
        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public T AutoExecuteScalar<T>(string sql, params Type[] types)
        {
            sql = this._DBAdapter.SqlFormat(sql);
            sql = this.AutoFormat(sql, types);
            sql = this._DBAdapter.SqlFormat(sql);
            string sp = this.CompileSqlToSp(this._DBAdapter.TemplateSp, sql, null);
            object obj;
            try
            {
                obj = this.RunScalar(sp);
            }
            catch (Exception ero)
            {
                if (ero.Message.Contains("找不到存储过程"))
                {
                    DBExtend.spCahe.Remove(sp);
                    sp = this.CompileSqlToSp(this._DBAdapter.TemplateSp, sql, null);
                    obj = this.RunScalar(sp);
                }
                throw ero;
            }
            this.ClearParame();
            return ObjectConvert.ConvertObject<T>(obj);
        }
        /// <summary>
        /// 按条件删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Delete<T>(string where) where T : IModel, new()
        {
            this.CheckTableCreated<T>();
            string table = TypeCache.GetTableName(typeof(T));
            string sql = this._DBAdapter.GetDeleteSql(table, where);
            sql = this._DBAdapter.SqlFormat(sql);
            int i = this.helper.Execute(sql);
            this.ClearParame();
            return i;
        }
        /// <summary>
        /// 按主键删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Delete<T>(int id) where T : IModelBase, new()
        {
            return this.Delete<T>((T b) => b.Id == id);
        }
        /// <summary>
        /// 指定条件删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public int Delete<T>(Expression<Func<T, bool>> expression) where T : IModel, new()
        {
            LambdaQuery<T> query = new LambdaQuery<T>(this, false);
            string condition = query.FormatExpression(expression);
            foreach (KeyValuePair<string, object> i in query.QueryParames)
            {
                this.AddParam(i.Key, i.Value);
            }
            return this.Delete<T>(condition);
        }
        /// <summary>
        /// 批量插入,并指定是否保持自增主键
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="details"></param>
        /// <param name="keepIdentity"></param>
        public void BatchInsert<TItem>(List<TItem> details, bool keepIdentity = false) where TItem : IModel, new()
        {
            
        }
        /// <summary>
        /// 单个插入
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int InsertFromObj<TItem>(TItem obj) where TItem : IModel, new()
        {
            this.CheckTableCreated<TItem>();
            FieldAttribute primaryKey = TypeCache.GetTable(obj.GetType()).PrimaryKey;
            this.CheckData(obj);
            int index = this._DBAdapter.InsertObject(obj, this.helper);
            primaryKey.SetValue(obj, index);
            this.ClearParame();
            this.UpdateCacheItem<TItem>(obj, null);
            return index;
        }
        /// <summary>
        /// 按ID查询
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public TItem QueryItem<TItem>(int id) where TItem : IModelBase, new()
        {
            return this.QueryItem<TItem>((TItem b) => b.Id == id, true, false);
        }
        /// <summary>
        /// 查询返回单个结果
        /// 如果只查询ID,调用QueryItem(id)
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="expression"></param>
        /// <param name="idDest">是否按主键倒序</param>
        /// <param name="compileSp">是否编译成存储过程</param>
        /// <returns></returns>
        public TItem QueryItem<TItem>(Expression<Func<TItem, bool>> expression, bool idDest = true, bool compileSp = false) where TItem : IModel, new()
        {
            LambdaQuery<TItem> query = new LambdaQuery<TItem>(this, true);
            
            query.OrderByPrimaryKey(idDest);
            List<TItem> list = this.QueryList<TItem>(query, 0, compileSp);
            TItem result;
            if (list.Count == 0)
            {
                result = default(TItem);
            }
            else
            {
                result = list[0];
            }
            return result;
        }
        /// <summary>
        /// 使用lamada设置条件查询
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="expression"></param>
        /// <param name="compileSp">是否编译成储过程</param>
        /// <returns></returns>
        public List<TItem> QueryList<TItem>(Expression<Func<TItem, bool>> expression = null, bool compileSp = false) where TItem : IModel, new()
        {
            LambdaQuery<TItem> query = new LambdaQuery<TItem>(this, true);
            
            string key;
            return this.QueryList<TItem>(query, 0, out key, compileSp);
        }
        /// <summary>
        /// 使用完整的LamadaQuery查询
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="query"></param>
        /// <param name="cacheTime">过期时间,分</param>
        /// <param name="compileSp">是否编译成储过程</param>
        /// <returns></returns>
        public List<TItem> QueryList<TItem>(LambdaQuery<TItem> query, int cacheTime = 0, bool compileSp = false) where TItem : IModel, new()
        {
            string key;
            return this.QueryList<TItem>(query, cacheTime, out key, compileSp);
        }
        /// <summary>
        /// 使用完整的LamadaQuery查询
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="query"></param>
        /// <param name="cacheTime"></param>
        /// <param name="cacheKey">过期时间,分</param>
        /// <param name="compileSp">是否编译成储过程</param>
        /// <returns></returns>
        public List<TItem> QueryList<TItem>(LambdaQuery<TItem> query, int cacheTime, out string cacheKey, bool compileSp = false) where TItem : IModel, new()
        {
            this.CheckTableCreated<TItem>();
            bool setConstraintObj = true;
            cacheKey = "";
            foreach (KeyValuePair<string, object> i in query.QueryParames)
            {
                this.AddParam(i.Key, i.Value);
            }
            string sql = query.GetQuery();
            sql = this._DBAdapter.SqlFormat(sql);
            List<TItem> list;
            if (cacheTime <= 0)
            {
                DbDataReader reader;
                if (!compileSp)
                {
                    if (query.QueryTop > 0)
                    {
                        this.helper.AutoFormatWithNolock = false;
                    }
                    reader = this.helper.ExecDataReader(sql);
                }
                else
                {
                    string sp = this.CompileSqlToSp(this._DBAdapter.TemplateSp, sql, null);
                    reader = this.helper.RunDataReader(sql);
                }
                list = ObjectConvert.DataReaderToList<TItem>(reader, setConstraintObj, query.FieldMapping);
            }
            else
            {
                list = new List<TItem>();
            }
            this.ClearParame();
            this.SetOriginClone<TItem>(list);
            return list;
        }
        /// <summary>
        /// 使用封装的参数进行分页
        /// Fields(string),Sortfield(string),SortType(bool),PageSize(int),PageIndex(int),Condition(string)
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="parames"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<TItem> QueryListByPage<TItem>(ParameCollection parames, out int count) where TItem : IModel, new()
        {
            Predicate<FieldAttribute> predicate = null;
            string fields = string.Concat(parames["Fields"]);
            if (string.IsNullOrEmpty(fields) || fields.Trim() == "*")
            {
                List<FieldAttribute> typeArry = TypeCache.GetProperties(typeof(TItem), true);
                List<FieldAttribute> arg_63_0 = typeArry;
                if (predicate == null)
                {
                    predicate = ((FieldAttribute b) => b.Length > 500 || b.PropertyType == typeof(byte[]));
                }
                arg_63_0.RemoveAll(predicate);
                fields = Base.GetQueryFields(typeArry, false);
            }
            string sort = string.Concat(parames["sort"]);
            if (string.IsNullOrEmpty(sort))
            {
                sort = "id desc";
            }
            string pageSize = string.Concat(parames["PageSize"]);
            if (string.IsNullOrEmpty(pageSize))
            {
                pageSize = "20";
            }
            string pageIndex = string.Concat(parames["PageIndex"]);
            if (string.IsNullOrEmpty(pageIndex))
            {
                pageIndex = "1";
            }
            string condition = string.Concat(parames["Condition"]);
            return this.QueryListByPage<TItem>(condition, fields, sort, int.Parse(pageSize), int.Parse(pageIndex), out count);
        }
        private List<TItem> QueryListByPage<TItem>(string query, string fields, string sort, int pageSize, int pageIndex, out int count) where TItem : IModel, new()
        {
            query = this._DBAdapter.SqlFormat(query);
            
            List<TItem> list = new List<TItem>();
            count = 0;
           
            return list;
        }
        /// <summary>
        /// count
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public int Count<T>(Expression<Func<T, bool>> expression, bool compileSp = false) where T : IModel, new()
        {
            LambdaQuery<T> query = new LambdaQuery<T>(this, false);
            string condition = query.FormatExpression(expression);
            foreach (KeyValuePair<string, object> i in query.QueryParames)
            {
                this.AddParam(i.Key, i.Value);
            }
            return this.Count<T>(condition, compileSp);
        }
        /// <summary>
        /// count
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        protected int Count<T>(string condition, bool compileSp = false) where T : IModel, new()
        {
            this.CheckTableCreated<T>();
            string tableName = TypeCache.GetTableName<T>();
            string sql = string.Concat(new object[]
			{
				"select count(0) from ",
				tableName,
				' ',
				this._DBAdapter.GetWithNolockFormat(),
				" where ",
				condition
			});
            int result;
            if (compileSp)
            {
                result = this.AutoExecuteScalar<int>(sql, new Type[0]);
            }
            else
            {
                result = this.ExecScalar<int>(sql, new Type[0]);
            }
            return result;
        }
        /// <summary>
        /// sum
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="expression"></param>
        /// <param name="field"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public TType Sum<TType, TModel>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> field, bool compileSp = false) where TModel : IModel, new()
        {
            MemberExpression mExp = (MemberExpression)field.Body;
            string sortField = mExp.Member.Name;
            return this.Sum<TType, TModel>(expression, sortField, compileSp);
        }
        /// <summary>
        /// sum
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="expression"></param>
        /// <param name="field"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public TType Sum<TType, TModel>(Expression<Func<TModel, bool>> expression, string field, bool compileSp = false) where TModel : IModel, new()
        {
            LambdaQuery<TModel> query = new LambdaQuery<TModel>(this, false);
            string condition = query.FormatExpression(expression);
            foreach (KeyValuePair<string, object> i in query.QueryParames)
            {
                this.AddParam(i.Key, i.Value);
            }
            return this.Sum<TType, TModel>(condition, field, compileSp);
        }
        /// <summary>
        /// sum
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="condition"></param>
        /// <param name="field"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        protected TType Sum<TType, TModel>(string condition, string field, bool compileSp = false) where TModel : IModel, new()
        {
            this.CheckTableCreated<TModel>();
            string tableName = TypeCache.GetTableName<TModel>();
            if (!field.Contains(","))
            {
                field = this._DBAdapter.KeyWordFormat(field);
            }
            string sql = string.Concat(new object[]
			{
				"select sum(",
				field,
				") from ",
				tableName,
				' ',
				this._DBAdapter.GetWithNolockFormat(),
				" where ",
				condition
			});
            TType result;
            if (compileSp)
            {
                result = this.AutoExecuteScalar<TType>(sql, new Type[0]);
            }
            else
            {
                result = this.ExecScalar<TType>(sql, new Type[0]);
            }
            return result;
        }
        /// <summary>
        /// 创建副本
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="list"></param>
        private void SetOriginClone<TItem>(List<TItem> list) where TItem : IModel, new()
        {
            /*if (!SettingConfig.UsePropertyChange)
            {
                foreach (TItem item in list)
                {
                    TItem clone = item.Clone() as TItem;
                    clone.OriginClone = null;
                    item.OriginClone = clone;
                }
            }*/
        }
        private ParameCollection GetUpdateField<T>(T obj) where T : IModel, new()
        {
            ParameCollection c = new ParameCollection();
            List<FieldAttribute> fields = TypeCache.GetProperties(typeof(T), true);
            ParameCollection result;
            if (obj.Changes.Count > 0)
            {
                using (Dictionary<string, object>.Enumerator enumerator = obj.Changes.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<string, object> item = enumerator.Current;
                        FieldAttribute f = fields.Find(delegate(FieldAttribute b)
                        {
                            string arg_1E_0 = b.Name.ToLower();
                            KeyValuePair<string, object> item2 = item;
                            return arg_1E_0 == item2.Key.ToLower();
                        });
                        if (f != null)
                        {
                            if (!f.IsPrimaryKey && f.FieldType != FieldType.虚拟字段)
                            {
                                ParameCollection arg_D7_0 = c;
                                KeyValuePair<string, object> item3 = item;
                                string arg_D7_1 = item3.Key;
                                item3 = item;
                                arg_D7_0[arg_D7_1] = item3.Value;
                            }
                        }
                    }
                }
                result = c;
            }
            else
            {
                object origin = obj.OriginClone;
                if (origin == null)
                {
                    throw new Exception("_originClone为空,请确认此对象是由查询创建");
                }
                this.CheckData(obj);
                foreach (FieldAttribute f in fields)
                {
                    if (!f.IsPrimaryKey)
                    {
                        if (string.IsNullOrEmpty(f.VirtualField))
                        {
                            object originValue = f.GetValue(origin);
                            object currentValue = f.GetValue(obj);
                            if (!object.Equals(originValue, currentValue))
                            {
                                c.Add(f.Name, currentValue);
                            }
                        }
                    }
                }
                result = c;
            }
            return result;
        }
        /// <summary>
        /// 指定拼接条件更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setValue"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        private int Update<T>(ParameCollection setValue, string where) where T : IModel, new()
        {
            this.CheckTableCreated<T>();
            Type type = typeof(T);
            string table = TypeCache.GetTableName(type);
            string setString = this.ForamtSetValue<T>(setValue);
            string sql = this._DBAdapter.GetUpdateSql(table, setString, where);
            sql = this._DBAdapter.SqlFormat(sql);
            int i = this.helper.Execute(sql);
            this.ClearParame();
            return i;
        }
        /// <summary>
        /// 按对象差异更新,由主键确定记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int Update<T>(T obj) where T : IModel, new()
        {
            ParameCollection c = this.GetUpdateField<T>(obj);
            int result;
            if (c.Count == 0)
            {
                result = 0;
            }
            else
            {
                FieldAttribute primaryKey = TypeCache.GetTable(obj.GetType()).PrimaryKey;
                object keyValue = primaryKey.GetValue(obj);
                string where = string.Format("{0}=@{0}", primaryKey.Name);
                this.AddParam(primaryKey.Name, keyValue);
                int i = this.Update<T>(c, where);
                this.UpdateCacheItem<T>(obj, c);
                if (i == 0)
                {
                    throw new Exception("更新失败,找不到主键为 " + keyValue + " 的记录");
                }
                result = i;
            }
            return result;
        }
        /// <summary>
        /// 指定条件并按对象差异更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update<T>(Expression<Func<T, bool>> expression, T model) where T : IModel, new()
        {
            ParameCollection c = this.GetUpdateField<T>(model);
            int result;
            if (c.Count == 0)
            {
                result = 0;
            }
            else
            {
                result = this.Update<T>(expression, c);
            }
            return result;
        }
        /// <summary>
        /// 指定条件和参数进行更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">条件</param>
        /// <param name="setValue">值</param>
        /// <returns></returns>
        public int Update<T>(Expression<Func<T, bool>> expression, ParameCollection setValue) where T : IModel, new()
        {
            if (setValue.Count == 0)
            {
                throw new Exception("更新时发生错误,参数值为空 ParameCollection setValue");
            }
            LambdaQuery<T> query = new LambdaQuery<T>(this, false);
            string condition = query.FormatExpression(expression);
            foreach (KeyValuePair<string, object> i in query.QueryParames)
            {
                this.AddParam(i.Key, i.Value);
            }
            int count = this.Update<T>(setValue, condition);
            this.UpdateCacheItem<T>(expression, setValue);
            return count;
        }
        /// <summary>
        /// 按主键更新整个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        public int UpdateById<T>(T item) where T : IModel, new()
        {
            return this.Update<T>(item);
        }
    }
}
