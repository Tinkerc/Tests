using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
namespace CoreHelper
{
    public abstract class DBHelper
    {
        private string databaseName;
        protected System.Collections.Generic.Dictionary<string, object> _params;
        /// <summary>
        /// 是否自动把查询加上WithNolock
        /// </summary>
        public bool AutoFormatWithNolock = true;
        /// <summary>
        /// 开启事务的CONN
        /// </summary>
        protected DbConnection _conn;
        protected DbTransaction _trans;
        /// <summary>
        /// 是否记录查询统计
        /// </summary>
        public static bool LogQueryCount = false;
        /// <summary>
        /// 连接串
        /// </summary>
        protected string ConnectionString;
        /// <summary>
        /// 新的输出参数集合
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> OutParams = new System.Collections.Generic.Dictionary<string, object>();
        private System.Collections.Generic.Dictionary<string, object> OutParamsPut = new System.Collections.Generic.Dictionary<string, object>();
        /// <summary>
        /// 统计开始时间
        /// </summary>
        public static System.DateTime CountStartTime = System.DateTime.Now;
        /// <summary>
        /// 缓存的查询语句执行次数
        /// </summary>
        public static System.Collections.Generic.Dictionary<string, int> ExecuteCmdCount = new System.Collections.Generic.Dictionary<string, int>();
        private static object lockObj = new object();
        private static System.DateTime lastSaveTime = System.DateTime.Now;
        /// <summary>
        /// 使用DataReader时,上次Command
        /// </summary>
        private DbCommand CurrentDataReadCommand = null;
        /// <summary>
        /// 数据库名
        /// </summary>
        public string DatabaseName
        {
            get
            {
                if (string.IsNullOrEmpty(this.databaseName))
                {
                    DbConnection dbConnection = this.createConn_();
                    this.databaseName = dbConnection.Database;
                    dbConnection.Close();
                }
                return this.databaseName;
            }
        }
        /// <summary>
        /// 输入参数
        /// 不推荐直接访问此属性,用AddParam方法代替
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> Params
        {
            get
            {
                return this._params;
            }
            set
            {
                this._params = value;
            }
        }
        /// <summary>
        /// 当前数据库类型
        /// </summary>
        public abstract DBType CurrentDBType
        {
            get;
        }
        /// <summary>
        /// 清除参数
        /// 在重复执行SQL时需调用进而重新设定新参数
        /// </summary>
        public void ClearParams()
        {
            this.Params.Clear();
            this.OutParams.Clear();
        }
        /// <summary>
        /// 添加一个参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddParam(string name, object value)
        {
            this.Params.Add(name, value);
        }
        /// <summary>
        /// 设置参数,没有就添加,有就更新
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetParam(string name, object value)
        {
            if (this.Params.ContainsKey(name))
            {
                this.Params[name] = value;
            }
            else
            {
                this.Params.Add(name, value);
            }
        }
        /// <summary>
        /// format为加上 with(nolock)
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual string FormatWithNolock(string sql)
        {
            return sql;
        }
        /// <summary>
        /// 获取存储过程的return值,如果没有则为0
        /// sql没有
        /// </summary>
        /// <returns></returns>
        public int GetReturnValue()
        {
            string key = "return";
            int result;
            if (!this.OutParamsPut.ContainsKey(key))
            {
                result = 0;
            }
            else
            {
                result = System.Convert.ToInt32(this.OutParamsPut[key]);
            }
            return result;
        }
        /// <summary>
        /// 添加一个输出参数
        /// 此参数只支持能转换为string类型
        /// </summary>
        /// <param name="name"></param>
        public void AddOutParam(string name, object value = null)
        {
            name = name.Replace("@", "");
            this.OutParams.Add(name, value);
        }
        /// <summary>
        /// 获取OUTPUT的值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetOutParam(string name)
        {
            name = name.Replace("@", "");
            if (this.CurrentDataReadCommand != null)
            {
                this.GetOutPutValue(this.CurrentDataReadCommand);
            }
            return this.OutParamsPut[name];
        }
        private void GetOutPutValue(DbCommand cmd)
        {
            foreach (DbParameter dbParameter in cmd.Parameters)
            {
                if (dbParameter.Direction == ParameterDirection.Output)
                {
                    string text = dbParameter.ParameterName;
                    text = text.Replace("@", "");
                    if (this.OutParamsPut.ContainsKey(text))
                    {
                        this.OutParamsPut[text] = dbParameter.Value;
                    }
                    else
                    {
                        this.OutParamsPut.Add(text, dbParameter.Value);
                    }
                }
                else if (dbParameter.Direction == ParameterDirection.ReturnValue)
                {
                    string text = "return";
                    if (this.OutParamsPut.ContainsKey(text))
                    {
                        this.OutParamsPut[text] = dbParameter.Value;
                    }
                    else
                    {
                        this.OutParamsPut.Add(text, dbParameter.Value);
                    }
                }
            }
        }
        public DBHelper(string _connectionString)
        {
            this._params = new System.Collections.Generic.Dictionary<string, object>();
            this.ConnectionString = _connectionString;
        }
        protected abstract void fillCmdParams_(DbCommand cmd);
        protected abstract DbCommand createCmd_(string cmdText, DbConnection conn);
        protected abstract DbCommand createCmd_();
        protected abstract DbDataAdapter createDa_(string cmdText, DbConnection conn);
        protected abstract DbConnection createConn_();
        public abstract void InsertFromDataTable(DataTable dataTable, string tableName, bool keepIdentity = false);
        public abstract DataTable TablesPage(string tableName, string fields, string sortfield, bool singleSortType, int pageSize, int pageIndex, string condition, out int count);
        private void LogCommand(DbCommand cmd, System.Exception error)
        {
            string text = "\r\n类型:{0} 语句:{1} 参数:\r\n";
            text = error.Message + string.Format(text, cmd.CommandType, cmd.CommandText);
            System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
            foreach (DbParameter dbParameter in cmd.Parameters)
            {
                if (dbParameter.ParameterName != "return")
                {
                    string item = string.Format("[{0}] {1}:{2}", dbParameter.Direction, dbParameter.ParameterName, dbParameter.Value);
                    list.Add(item);
                }
            }
            text += string.Join("\r\n", list.ToArray());
            EventLog.Log(text, "DbError");
            EventLog.SendToServer(text, "DbError");
        }
        private int do_(string text, CommandType type)
        {
            int result;
            if (this._trans != null)
            {
                result = this.doTrans_(text, type);
            }
            else
            {
                using (DbConnection dbConnection = this.createConn_())
                {
                    dbConnection.Open();
                    DbCommand dbCommand = this.createCmd_(text, dbConnection);
                    dbCommand.CommandType = type;
                    this.fillCmdParams_(dbCommand);
                    int num = 0;
                    try
                    {
                        num = dbCommand.ExecuteNonQuery();
                    }
                    catch (System.Exception ex)
                    {
                        if (!(ex is System.Threading.ThreadAbortException))
                        {
                            this.LogCommand(dbCommand, ex);
                            throw ex;
                        }
                    }
                    this.GetOutPutValue(dbCommand);
                    result = num;
                }
            }
            return result;
        }
        private int doTrans_(string text, CommandType type)
        {
            DbCommand dbCommand = this.createCmd_(text, this._conn);
            dbCommand.CommandType = type;
            if (this._trans == null)
            {
                throw new System.Exception("事务没有开启");
            }
            dbCommand.Transaction = this._trans;
            this.fillCmdParams_(dbCommand);
            int result = 0;
            try
            {
                result = dbCommand.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
                if (!(ex is System.Threading.ThreadAbortException))
                {
                    this.LogCommand(dbCommand, ex);
                    throw ex;
                }
            }
            this.GetOutPutValue(dbCommand);
            return result;
        }
        /// <summary>
        /// 增加到缓存
        /// </summary>
        /// <param name="cmd"></param>
        private static void AddQueryCmdCache(DbCommand cmd)
        {
            string text = cmd.CommandText;
            text = Regex.Replace(text, "\\r\\n", " ", RegexOptions.IgnoreCase);
            text += " ";
            foreach (DbParameter dbParameter in cmd.Parameters)
            {
                if (dbParameter.ParameterName != "return")
                {
                    string text2 = dbParameter.ParameterName.Replace("@", "");
                    if (cmd.CommandType == CommandType.StoredProcedure)
                    {
                        text += string.Format("{0}:[{1}] ", text2, dbParameter.Value);
                    }
                    else
                    {
                        text = Regex.Replace(text, "@" + text2.ToLower(), "[" + dbParameter.Value + "]", RegexOptions.IgnoreCase);
                    }
                }
            }
            string text3 = text.Trim();
            lock (DBHelper.lockObj)
            {
                if (DBHelper.ExecuteCmdCount.ContainsKey(text3))
                {
                    System.Collections.Generic.Dictionary<string, int> executeCmdCount;
                    string key;
                    (executeCmdCount = DBHelper.ExecuteCmdCount)[key = text3] = executeCmdCount[key] + 1;
                }
                else
                {
                    DBHelper.ExecuteCmdCount.Add(text3, 1);
                }
                if ((System.DateTime.Now - DBHelper.lastSaveTime).TotalHours > 2.0)
                {
                    DBHelper.ClearExecuteCmdsCount();
                    DBHelper.lastSaveTime = System.DateTime.Now;
                }
            }
        }
        /// <summary>
        /// 获取以排过序的统计结果
        /// </summary>
        /// <param name="min">过滤最小次数</param>
        /// <returns></returns>
        public static DataView GetExecuteCmdsCount(int min)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("SQL", typeof(string));
            dataTable.Columns.Add("COUNT", typeof(int));
            System.Collections.Generic.Dictionary<string, int> dictionary = new System.Collections.Generic.Dictionary<string, int>(DBHelper.ExecuteCmdCount);
            foreach (System.Collections.Generic.KeyValuePair<string, int> current in dictionary)
            {
                DataRow dataRow = dataTable.NewRow();
                if (current.Value > min)
                {
                    dataRow["SQL"] = current.Key;
                    dataRow["COUNT"] = current.Value;
                    dataTable.Rows.Add(dataRow);
                }
            }
            DataView defaultView = dataTable.DefaultView;
            defaultView.Sort = "COUNT desc";
            return defaultView;
        }
        /// <summary>
        /// 清空统计缓存
        /// </summary>
        public static void ClearExecuteCmdsCount()
        {
            DBHelper.SaveExecuteCmdsCount();
            DBHelper.ExecuteCmdCount.Clear();
            DBHelper.CountStartTime = System.DateTime.Now;
        }
        /// <summary>
        /// 保存统计结果
        /// </summary>
        public static void SaveExecuteCmdsCount()
        {
            if (DBHelper.LogQueryCount)
            {
                DataView executeCmdsCount = DBHelper.GetExecuteCmdsCount(1);
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                stringBuilder.Append(string.Format("时间 {0}-{1} 总数:{2}\r\n", DBHelper.CountStartTime.ToShortTimeString(), System.DateTime.Now.ToShortTimeString(), executeCmdsCount.Count));
                foreach (DataRowView dataRowView in executeCmdsCount)
                {
                    stringBuilder.Append(string.Format("{1}\t{0}\r\n", dataRowView[0], dataRowView[1]));
                }
                EventLog.Log(stringBuilder.ToString(), "ExecuteCount", false);
            }
        }
        private DataSet doDateSet_(string text, CommandType type)
        {
            DataSet result;
            using (DbConnection dbConnection = this.createConn_())
            {
                dbConnection.Open();
                DbDataAdapter dbDataAdapter = this.createDa_(text, dbConnection);
                dbDataAdapter.SelectCommand.CommandType = type;
                this.fillCmdParams_(dbDataAdapter.SelectCommand);
                DataSet dataSet = new DataSet();
                try
                {
                    DBHelper.AddQueryCmdCache(dbDataAdapter.SelectCommand);
                    dbDataAdapter.Fill(dataSet);
                    this.GetOutPutValue(dbDataAdapter.SelectCommand);
                }
                catch (System.Exception ex)
                {
                    this.LogCommand(dbDataAdapter.SelectCommand, ex);
                    throw ex;
                }
                result = dataSet;
            }
            return result;
        }
        private object doScalar_(string text, CommandType type)
        {
            object result;
            if (this._trans != null)
            {
                result = this.doScalarTrans_(text, type);
            }
            else
            {
                using (DbConnection dbConnection = this.createConn_())
                {
                    dbConnection.Open();
                    DbCommand dbCommand = this.createCmd_(text, dbConnection);
                    this.fillCmdParams_(dbCommand);
                    dbCommand.CommandType = type;
                    object obj = null;
                    try
                    {
                        DBHelper.AddQueryCmdCache(dbCommand);
                        obj = dbCommand.ExecuteScalar();
                        this.GetOutPutValue(dbCommand);
                    }
                    catch (System.Exception ex)
                    {
                        this.LogCommand(dbCommand, ex);
                        throw ex;
                    }
                    result = obj;
                }
            }
            return result;
        }
        private object doScalarTrans_(string text, CommandType type)
        {
            DbCommand dbCommand = this.createCmd_(text, this._conn);
            if (this._trans == null)
            {
                throw new System.Exception("事务没有开启");
            }
            dbCommand.Transaction = this._trans;
            this.fillCmdParams_(dbCommand);
            dbCommand.CommandType = type;
            object result = null;
            try
            {
                DBHelper.AddQueryCmdCache(dbCommand);
                result = dbCommand.ExecuteScalar();
            }
            catch (System.Exception ex)
            {
                this.LogCommand(dbCommand, ex);
                throw ex;
            }
            return result;
        }
        private DbDataReader doDataReader_(string text, CommandType type)
        {
            DbConnection dbConnection = this.createConn_();
            DbDataReader result;
            try
            {
                this.CurrentDataReadCommand = null;
                dbConnection.Open();
                this.CurrentDataReadCommand = this.createCmd_(text, dbConnection);
                this.CurrentDataReadCommand.CommandType = type;
                this.fillCmdParams_(this.CurrentDataReadCommand);
                DbDataReader dbDataReader;
                try
                {
                    DBHelper.AddQueryCmdCache(this.CurrentDataReadCommand);
                    dbDataReader = this.CurrentDataReadCommand.ExecuteReader(CommandBehavior.CloseConnection);
                    this.GetOutPutValue(this.CurrentDataReadCommand);
                }
                catch (System.Exception ex)
                {
                    this.LogCommand(this.CurrentDataReadCommand, ex);
                    throw ex;
                }
                result = dbDataReader;
            }
            catch (DbException ex2)
            {
                dbConnection.Close();
                throw ex2;
            }
            return result;
        }
        /// <summary>
        /// 执行一条sql语句，返回影响行数
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public int Execute(string sql)
        {
            return this.do_(sql, CommandType.Text);
        }
        /// <summary>
        /// 执行一个存储过程，返回影响行数
        /// </summary>
        /// <param name="sp">存储过程</param>
        /// <returns></returns>
        public int Run(string sp)
        {
            return this.do_(sp, CommandType.StoredProcedure);
        }
        /// <summary>
        /// 执行一条sql语句，返回DataTable
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public DataTable ExecDataTable(string sql)
        {
            return this.ExecDataSet(sql).Tables[0];
        }
        /// <summary>
        /// 执行一个存储过程，返回DataTable
        /// </summary>
        /// <param name="sp">存储过程</param>
        /// <returns></returns>
        public DataTable RunDataTable(string sp)
        {
            return this.RunDataSet(sp).Tables[0];
        }
        /// <summary>
        /// 执行一条sql语句，返回DataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataSet ExecDataSet(string sql)
        {
            return this.doDateSet_(sql, CommandType.Text);
        }
        /// <summary>
        /// 执行一个存储过程，返回DataSet
        /// </summary>
        /// <param name="sp">存储过程</param>
        /// <returns></returns>
        public DataSet RunDataSet(string sp)
        {
            return this.doDateSet_(sp, CommandType.StoredProcedure);
        }
        /// <summary>
        /// 执行一条sql语句，返回首行首列
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public object ExecScalar(string sql)
        {
            return this.doScalar_(sql, CommandType.Text);
        }
        /// <summary>
        /// 执行一个存储过程，返回首行首列
        /// </summary>
        /// <param name="sp">存储过程</param>
        /// <returns></returns>
        public object RunScalar(string sp)
        {
            return this.doScalar_(sp, CommandType.StoredProcedure);
        }
        /// <summary>
        /// 执行一条sql语句，返回DbDataReader
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public DbDataReader ExecDataReader(string sql)
        {
            return this.doDataReader_(sql, CommandType.Text);
        }
        /// <summary>
        /// 执行一个存储过程，返回DbDataReader
        /// </summary>
        /// <param name="sp">存储过程</param>
        /// <returns></returns>
        public DbDataReader RunDataReader(string sp)
        {
            return this.doDataReader_(sp, CommandType.StoredProcedure);
        }
        /// <summary>
        /// 开始事务,调用事务必须调用CommitTran()提交事务或者调用RollbackTran()回滚事务
        /// </summary>
        public void BeginTran()
        {
            this._conn = this.createConn_();
            try
            {
                this._conn.Open();
                this._trans = this._conn.BeginTransaction();
            }
            catch (DbException ex)
            {
                throw new System.Exception(ex.Message);
            }
        }
        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTran()
        {
            if (this._conn == null)
            {
                throw new System.Exception("数据连接意外关闭");
            }
            try
            {
                this._trans.Commit();
                this._conn.Close();
            }
            catch (System.InvalidOperationException var_0_39)
            {
                this._conn.Close();
            }
            catch (DbException ex)
            {
                this._conn.Close();
                throw new System.Exception(ex.Message);
            }
        }
        /// <summary>
        /// 回滚事务事务
        /// </summary>
        public void RollbackTran()
        {
            if (this._conn == null)
            {
                throw new System.Exception("数据连接意外关闭");
            }
            try
            {
                this._trans.Rollback();
                this._conn.Close();
            }
            catch (System.InvalidOperationException var_0_39)
            {
                this._conn.Close();
            }
            catch (DbException ex)
            {
                this._conn.Close();
                throw new System.Exception(ex.Message);
            }
        }
    }
}
