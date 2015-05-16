using CoreHelper;
using CRL.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
namespace CRL.DBAdapter
{
    internal class MSSQLDBAdapter : DBAdapterBase
    {
        public override DBType DBType
        {
            get
            {
                return DBType.MSSQL;
            }
        }
        public override string TemplateGroupPage
        {
            get
            {
                return "\r\n--group分页\r\nCREATE PROCEDURE [dbo].{name}\r\n( \r\n\t{parame}\r\n) \r\n--参数传入 @pageSize,@pageIndex\r\nAS\r\nset  nocount  on\r\ndeclare @start nvarchar(20) \r\ndeclare @end nvarchar(20)\r\ndeclare @pageCount INT\r\n\r\nbegin\r\n\r\n    --获取记录数\r\n\t  select @count=count(0) from (select count(*) as a from  {sql}) t\r\n    if @count = 0\r\n        set @count = 1\r\n\r\n    --取得分页总数\r\n    set @pageCount=(@count+@pageSize-1)/@pageSize\r\n\r\n    /**当前页大于总页数 取最后一页**/\r\n    if @pageIndex>@pageCount\r\n        set @pageIndex=@pageCount\r\n\r\n\t--计算开始结束的行号\r\n\tset @start = @pageSize*(@pageIndex-1)+1\r\n\tset @end = @start+@pageSize-1 \r\n\tSELECT * FROM (select {fields},ROW_NUMBER() OVER ( Order by {rowOver} ) AS RowNumber From {sql}) T WHERE T.RowNumber BETWEEN @start AND @end order by {sort}\r\nend\r\n";
            }
        }
        public override string TemplatePage
        {
            get
            {
                return "\r\n--表分页\r\nCREATE PROCEDURE [dbo].{name}\r\n( \r\n\t{parame}\r\n) \r\n--参数传入 @pageSize,@pageIndex\r\nAS\r\nset  nocount  on\r\ndeclare @start nvarchar(20) \r\ndeclare @end nvarchar(20)\r\ndeclare @pageCount INT\r\n\r\nbegin\r\n\r\n    --获取记录数\r\n\t  select @count=count(0) from {sql}\r\n    if @count = 0\r\n        set @count = 1\r\n\r\n    --取得分页总数\r\n    set @pageCount=(@count+@pageSize-1)/@pageSize\r\n\r\n    /**当前页大于总页数 取最后一页**/\r\n    if @pageIndex>@pageCount\r\n        set @pageIndex=@pageCount\r\n\r\n\t--计算开始结束的行号\r\n\tset @start = @pageSize*(@pageIndex-1)+1\r\n\tset @end = @start+@pageSize-1 \r\n\tSELECT * FROM (select {fields},ROW_NUMBER() OVER ( Order by {rowOver} ) AS RowNumber From {sql}) T WHERE T.RowNumber BETWEEN @start AND @end order by {sort}\r\nend\r\n\r\n";
            }
        }
        public override string TemplateSp
        {
            get
            {
                return "\r\nCREATE PROCEDURE [dbo].{name}\r\n({parame})\r\nAS\r\nset  nocount  on\r\n\t{sql}\r\n";
            }
        }
        /// <summary>
        /// 创建存储过程脚本
        /// </summary>
        /// <param name="spName"></param>
        /// <returns></returns>
        public override string GetCreateSpScript(string spName, string script)
        {
            return string.Format("\r\nif not exists(select * from sysobjects where name='{0}' and type='P')\r\nbegin\r\nexec sp_executesql N' {1} '\r\nend", spName, script);
        }
        /// <summary>
        /// 获取字段类型映射
        /// </summary>
        /// <returns></returns>
        public override Dictionary<Type, string> GetFieldMapping()
        {
            return new Dictionary<Type, string>
			{
				
				{
					typeof(string),
					"nvarchar({0})"
				},
				
				{
					typeof(decimal),
					"decimal(18, 2)"
				},
				
				{
					typeof(double),
					"float"
				},
				
				{
					typeof(float),
					"real"
				},
				
				{
					typeof(bool),
					"bit"
				},
				
				{
					typeof(int),
					"int"
				},
				
				{
					typeof(short),
					"SMALLINT"
				},
				
				{
					typeof(Enum),
					"int"
				},
				
				{
					typeof(byte),
					"SMALLINT"
				},
				
				{
					typeof(DateTime),
					"datetime"
				},
				
				{
					typeof(ushort),
					"SMALLINT"
				},
				
				{
					typeof(long),
					"bigint"
				},
				
				{
					typeof(object),
					"nvarchar(30)"
				},
				
				{
					typeof(byte[]),
					"varbinary({0})"
				},
				
				{
					typeof(Guid),
					"nvarchar(50)"
				}
			};
        }
        /// <summary>
        /// 获取列类型和默认值
        /// </summary>
        /// <param name="info"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override string GetColumnType(FieldAttribute info, out string defaultValue)
        {
            Type propertyType = info.PropertyType;
            Dictionary<Type, string> dic = this.GetFieldMapping();
            defaultValue = info.DefaultValue;
            if (string.IsNullOrEmpty(defaultValue))
            {
                if (!info.IsPrimaryKey && propertyType == typeof(int))
                {
                    defaultValue = "(0)";
                }
                if (propertyType == typeof(DateTime))
                {
                    defaultValue = "(getdate())";
                }
            }
            string columnType;
            if (propertyType.FullName.IndexOf("System.") > -1)
            {
                columnType = dic[propertyType];
            }
            else
            {
                propertyType = info.PropertyType.BaseType;
                columnType = dic[propertyType];
            }
            if (propertyType == typeof(string) && info.Length > 3000)
            {
                columnType = "ntext";
            }
            if (info.Length > 0)
            {
                columnType = string.Format(columnType, info.Length);
            }
            if (info.IsPrimaryKey)
            {
                columnType = "int IDENTITY(1,1) NOT NULL";
            }
            if (!string.IsNullOrEmpty(info.ColumnType))
            {
                columnType = info.ColumnType;
            }
            return columnType;
        }
        /// <summary>
        /// 创建字段脚本
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public override string GetCreateColumnScript(FieldAttribute field)
        {
            string str = string.Format("alter table [{0}] add {1} {2}", field.TableName, field.KeyWordName, field.ColumnType);
            if (!string.IsNullOrEmpty(field.DefaultValue))
            {
                str += string.Format(" default({0})", field.DefaultValue);
            }
            if (field.NotNull)
            {
                str += " not null";
            }
            return str;
        }
        /// <summary>
        /// 创建索引脚本
        /// </summary>
        /// <param name="filed"></param>
        /// <returns></returns>
        public override string GetColumnIndexScript(FieldAttribute filed)
        {
            return string.Format("CREATE {2} NONCLUSTERED INDEX  IX_INDEX_{0}_{1}  ON dbo.[{0}]([{1}])", filed.TableName, filed.Name, (filed.FieldIndexType == FieldIndexType.非聚集唯一) ? "UNIQUE" : "");
        }
        /// <summary>
        /// 创建表脚本
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override void CreateTable(DBExtend helper, List<FieldAttribute> fields, string tableName)
        {
            List<string> defaultValues = new List<string>();
            string script = string.Format("create table [{0}] (\r\n", tableName);
            List<string> list2 = new List<string>();
            string primaryKey = "id";
            foreach (FieldAttribute item in fields)
            {
                if (item.IsPrimaryKey)
                {
                    primaryKey = item.Name;
                }
                string nullStr = item.NotNull ? "NOT NULL" : "";
                string str = string.Format("{0} {1} {2} ", item.KeyWordName, item.ColumnType, nullStr);
                list2.Add(str);
                if (!string.IsNullOrEmpty(item.DefaultValue))
                {
                    string v = string.Format("ALTER TABLE [dbo].[{0}] ADD  CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}]", tableName, item.Name, item.DefaultValue);
                    defaultValues.Add(v);
                }
            }
            script += string.Join(",\r\n", list2.ToArray());
            script += string.Format(" CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED \r\n(\r\n\t[{1}] ASC\r\n)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]\r\n", tableName, primaryKey);
            script += ") ON [PRIMARY]";
            helper.Execute(script, new Type[0]);
            foreach (string s in defaultValues)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    helper.Execute(s, new Type[0]);
                }
            }
        }
        /// <summary>
        /// 批量插入
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="helper"></param>
        /// <param name="details"></param>
        /// <param name="keepIdentity"></param>
        public override void BatchInsert<TItem>(DBHelper helper, List<TItem> details, bool keepIdentity = false)
        {
            string table = TypeCache.GetTableName(typeof(TItem));
            string sql = this.GetSelectTop("*", " from " + table + " where 1=0", "", 1);
            DataTable tempTable = helper.ExecDataTable(sql);
            List<FieldAttribute> typeArry = TypeCache.GetProperties(typeof(TItem), true);
            foreach (TItem item in details)
            {
                DataRow dr = tempTable.NewRow();
                foreach (FieldAttribute info in typeArry)
                {
                    string name = info.Name;
                    object value = info.GetValue(item);
                    if (!keepIdentity)
                    {
                        if (info.IsPrimaryKey)
                        {
                            continue;
                        }
                    }
                    if (string.IsNullOrEmpty(info.VirtualField))
                    {
                        object value2 = ObjectConvert.SetNullValue(value, info.PropertyType);
                        dr[name] = value2;
                    }
                }
                tempTable.Rows.Add(dr);
            }
            helper.InsertFromDataTable(tempTable, table, keepIdentity);
        }
        /// <summary>
        /// 获取插入语法
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="helper"></param>
        /// <returns></returns>
        public override int InsertObject(IModel obj, DBHelper helper)
        {
            Type type = obj.GetType();
            string table = TypeCache.GetTableName(type);
            List<FieldAttribute> typeArry = TypeCache.GetProperties(type, true);
            string sql = string.Format("insert into [{0}](", table);
            string sql2 = "";
            string sql3 = "";
            foreach (FieldAttribute info in typeArry)
            {
                string name = info.Name;
                if (!info.IsPrimaryKey)
                {
                    if (string.IsNullOrEmpty(info.VirtualField))
                    {
                        object value = info.GetValue(obj);
                        value = ObjectConvert.SetNullValue(value, info.PropertyType);
                        sql2 += string.Format("{0},", info.KeyWordName);
                        sql3 += string.Format("@{0},", name);
                        helper.AddParam(name, value);
                    }
                }
            }
            sql2 = sql2.Substring(0, sql2.Length - 1);
            sql3 = sql3.Substring(0, sql3.Length - 1);
            string text = sql;
            sql = string.Concat(new string[]
			{
				text,
				sql2,
				") values( ",
				sql3,
				") ; SELECT scope_identity() ;"
			});
            sql = this.SqlFormat(sql);
            return Convert.ToInt32(helper.ExecScalar(sql));
        }
        /// <summary>
        /// 获取 with(nolock)
        /// </summary>
        /// <returns></returns>
        public override string GetWithNolockFormat()
        {
            return " with(nolock)";
        }
        /// <summary>
        /// 获取前几条语句
        /// </summary>
        /// <param name="fields">id,name</param>
        /// <param name="query">from table where 1=1</param>
        /// <param name="top"></param>
        /// <returns></returns>
        public override string GetSelectTop(string fields, string query, string sort, int top)
        {
            return string.Format("select {0} {1} {2} {3}", new object[]
			{
				(top == 0) ? "" : ("top " + top),
				fields,
				query,
				sort
			});
        }
        public override string GetAllTablesSql(DBHelper helper)
        {
            return "select Lower(name),id from sysobjects where  type='u'";
        }
        public override string GetAllSPSql(DBHelper helper)
        {
            return "select name,id from sysobjects where  type='P'";
        }
        public override string SpParameFormat(string name, string type, bool output)
        {
            string str;
            if (!output)
            {
                str = "@{0} {1},";
            }
            else
            {
                str = "@{0} {1} output,";
            }
            return string.Format(str, name, type);
        }
        public override string KeyWordFormat(string value)
        {
            return string.Format("[{0}]", value);
        }
        public override string SqlFormat(string sql)
        {
            return sql;
        }
        public override string SubstringFormat(string field, int index, int length)
        {
            return string.Format(" SUBSTRING({0},{1},{2})", field, index, length);
        }
        public override string StringLikeFormat(string field, string parName)
        {
            return string.Format("{0} LIKE {1}", field, parName);
        }
        public override string StringNotLikeFormat(string field, string parName)
        {
            return string.Format("{0} NOT LIKE {1}", field, parName);
        }
        public override string StringContainsFormat(string field, string parName)
        {
            return string.Format("CHARINDEX({1},{0})>0", field, parName);
        }
        public override string BetweenFormat(string field, string parName, string parName2)
        {
            return string.Format("{0} between {1} and {2}", field, parName, parName2);
        }
        public override string DateDiffFormat(string field, string format, string parName)
        {
            return string.Format("DateDiff({0},{1},{2})", format, field, parName);
        }
        public override string InFormat(string field, string parName)
        {
            return string.Format("{0} IN ({1})", field, parName);
        }
        public override string NotInFormat(string field, string parName)
        {
            return string.Format("{0} NOT IN ({1})", field, parName);
        }
    }
}
