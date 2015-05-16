using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
namespace CoreHelper
{
    public class SqlHelper : DBHelper
    {
        private static System.Collections.Generic.Dictionary<string, string> formatCache = new System.Collections.Generic.Dictionary<string, string>();
        private static object lockObj = new object();
        public override DBType CurrentDBType
        {
            get
            {
                return DBType.MSSQL;
            }
        }
        /// <summary>
        /// 根据参数类型实例化
        /// </summary>
        /// <param name="content">内容</param>
        public SqlHelper(string content)
            : base(content)
        {
        }
        public override string FormatWithNolock(string cmdText)
        {
            string result;
            if (!Regex.IsMatch(cmdText, "^select\\s", RegexOptions.IgnoreCase))
            {
                result = cmdText;
            }
            else if (SqlHelper.formatCache.ContainsKey(cmdText))
            {
                result = SqlHelper.formatCache[cmdText];
            }
            else
            {
                string pattern = "(update|insert|delete|truncate)\\s+[a-z0-9._#]+";
                if (!Regex.IsMatch(cmdText, pattern, RegexOptions.IgnoreCase) && this.AutoFormatWithNolock)
                {
                    string text = Regex.Replace(cmdText, "\\r\\n", " ", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, " with\\s*\\(\\s*nolock\\s*\\)", " ", RegexOptions.IgnoreCase).Trim();
                    text = Regex.Replace(text, " (from(\\s+[a-z0-9._#\\[\\]]+)+?)\\s+(where|left|right|inner|group|order)", " $1 with(nolock) $3", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, " (join(\\s+[a-z0-9._#\\[\\]]+)+?)\\s+on", " $1 with(nolock) on", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, " (from\\s+([a-z0-9._#\\[\\]]+)((\\s+as)*(\\s+[a-z0-9._#]+))*)\\s*\\)", " $1 with(nolock))", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text.Trim(), " (from\\s+([a-z0-9._#\\[\\]]+)((\\s+as){0,1}(\\s+[a-z0-9._#]+)){0,1}$)", " $1 with(nolock)", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "([a-z0-9._#]+)\\s*,\\s*([a-z0-9._#\\[\\]]+)", "$1 , $2", RegexOptions.IgnoreCase);
                    string pattern2 = "from\\s+(((([a-z0-9._#]+)\\s*)+(,\\s*){0,1})+)";
                    Regex regex = new Regex(pattern2, RegexOptions.IgnoreCase);
                    Match match = regex.Match(text);
                    while (match.Success)
                    {
                        string text2 = match.Groups[1].ToString();
                        string newValue = Regex.Replace(text2, "([a-z0-9._#\\[\\]]+)\\s*(,|where)", " $1 with(nolock) $2", RegexOptions.IgnoreCase);
                        text = text.Replace(text2, newValue);
                        match = match.NextMatch();
                    }
                    lock (SqlHelper.lockObj)
                    {
                        if (!SqlHelper.formatCache.ContainsKey(cmdText))
                        {
                            SqlHelper.formatCache.Add(cmdText, text);
                        }
                    }
                    result = text;
                }
                else
                {
                    result = cmdText;
                }
            }
            return result;
        }
        protected override void fillCmdParams_(DbCommand cmd)
        {
            foreach (System.Collections.Generic.KeyValuePair<string, object> current in this._params)
            {
                DbParameter dbParameter = new SqlParameter(current.Key, current.Value);
                cmd.Parameters.Add(dbParameter);
            }
            foreach (System.Collections.Generic.KeyValuePair<string, object> current in this.OutParams)
            {
                if (current.Key != "return")
                {
                    DbParameter dbParameter = new SqlParameter(current.Key, SqlDbType.NVarChar, 500);
                    dbParameter.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(dbParameter);
                }
            }
            if (cmd.CommandType == CommandType.StoredProcedure)
            {
                DbParameter dbParameter2 = new SqlParameter("return", SqlDbType.Int);
                dbParameter2.Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(dbParameter2);
            }
        }
        protected override DbCommand createCmd_(string cmdText, DbConnection conn)
        {
            cmdText = this.FormatWithNolock(cmdText);
            return new SqlCommand(cmdText, (SqlConnection)conn);
        }
        protected override DbCommand createCmd_()
        {
            return new SqlCommand();
        }
        protected override DbDataAdapter createDa_(string cmdText, DbConnection conn)
        {
            cmdText = this.FormatWithNolock(cmdText);
            return new SqlDataAdapter(cmdText, (SqlConnection)conn);
        }
        protected override DbConnection createConn_()
        {
            return new SqlConnection(this.ConnectionString);
        }
        /// <summary>
        /// 新的分页存储过程，更改原来查询结果排序错误
        /// 以前传入排序参数可能不兼容，会导致语法错误
        /// </summary>
        /// <param name="tableName">要显示的表或多个表的连接</param>
        /// <param name="fields">要显示的字段列表</param>
        /// <param name="sortfield">排序字段</param>
        /// <param name="singleSortType">排序方法，false为升序，true为降序</param>
        /// <param name="pageSize">每页显示的记录个数</param>
        /// <param name="pageIndex">要显示那一页的记录</param>
        /// <param name="condition">查询条件,不需where</param>
        /// <param name="count">查询到的记录数</param>
        /// <returns></returns>
        public override DataTable TablesPage(string tableName, string fields, string sortfield, bool singleSortType, int pageSize, int pageIndex, string condition, out int count)
        {
            base.Params.Clear();
            base.Params.Add("tblName", tableName);
            base.Params.Add("fields", fields);
            base.Params.Add("sortfields", sortfield);
            base.Params.Add("singleSortType", singleSortType ? "1" : "0");
            base.Params.Add("pageSize", pageSize);
            base.Params.Add("pageIndex", pageIndex);
            base.Params.Add("strCondition", condition);
            base.AddOutParam("counts", null);
            DataTable result = base.RunDataTable("sp_TablesPageNew");
            count = System.Convert.ToInt32(base.GetOutParam("counts"));
            return result;
        }
        /// <summary>
        /// 根据表插入记录,dataTable需按查询生成结构
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="tableName"></param>
        /// <param name="keepIdentity"></param>
        public override void InsertFromDataTable(DataTable dataTable, string tableName, bool keepIdentity = false)
        {
            SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(this.ConnectionString, keepIdentity ? SqlBulkCopyOptions.KeepIdentity : SqlBulkCopyOptions.KeepNulls);
            sqlBulkCopy.DestinationTableName = tableName;
            sqlBulkCopy.BatchSize = dataTable.Rows.Count;
            if (dataTable != null && dataTable.Rows.Count != 0)
            {
                sqlBulkCopy.WriteToServer(dataTable);
            }
            sqlBulkCopy.Close();
        }
    }
}
