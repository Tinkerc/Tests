using CoreHelper;
using CRL.Attribute;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace CRL.DBAdapter
{
    internal class ORACLEDBAdapter : DBAdapterBase
    {
        public override DBType DBType
        {
            get
            {
                return DBType.ORACLE;
            }
        }
        public override string TemplateGroupPage
        {
            get
            {
                throw new NotSupportedException("ORACLE不支持动态创建存储过程");
            }
        }
        public override string TemplatePage
        {
            get
            {
                throw new NotSupportedException("ORACLE不支持动态创建存储过程");
            }
        }
        public override string TemplateSp
        {
            get
            {
                throw new NotSupportedException("ORACLE不支持动态创建存储过程");
            }
        }
        /// <summary>
        /// 创建存储过程脚本
        /// </summary>
        /// <param name="spName"></param>
        /// <returns></returns>
        public override string GetCreateSpScript(string spName, string script)
        {
            throw new NotSupportedException("ORACLE不支持动态创建存储过程");
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
					"VARCHAR2({0})"
				},
				
				{
					typeof(decimal),
					"NUMBER"
				},
				
				{
					typeof(double),
					"DOUBLE PRECISION"
				},
				
				{
					typeof(float),
					"FLOAT(24)"
				},
				
				{
					typeof(bool),
					"INTEGER"
				},
				
				{
					typeof(int),
					"INTEGER"
				},
				
				{
					typeof(short),
					"INTEGER"
				},
				
				{
					typeof(Enum),
					"INTEGER"
				},
				
				{
					typeof(byte),
					"INTEGER"
				},
				
				{
					typeof(DateTime),
					"TIMESTAMP"
				},
				
				{
					typeof(ushort),
					"INTEGER"
				},
				
				{
					typeof(object),
					"NARCHAR2(30)"
				},
				
				{
					typeof(byte[]),
					"BLOB"
				},
				
				{
					typeof(Guid),
					"VARCHAR2(50)"
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
                    defaultValue = "0";
                }
                if (propertyType == typeof(DateTime))
                {
                    defaultValue = "TIMESTAMP";
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
                columnType = "CLOB";
            }
            if (info.Length > 0)
            {
                columnType = string.Format(columnType, info.Length);
            }
            if (info.IsPrimaryKey)
            {
                columnType = "NUMBER(4) Not Null Primary Key";
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
            string str = string.Format("alter table {0} add {1} {2};", field.TableName, field.KeyWordName, field.ColumnType);
            if (!string.IsNullOrEmpty(field.DefaultValue))
            {
                str += string.Format(" default '{0}' ", field.DefaultValue);
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
            string indexName = string.Format("pk_{0}_{1}", filed.TableName, filed.Name);
            return string.Format("create {3} index {0} on {1}({2}); ", new object[]
			{
				indexName,
				filed.TableName,
				filed.KeyWordName,
				(filed.FieldIndexType == FieldIndexType.非聚集唯一) ? "UNIQUE" : ""
			});
        }
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="fields"></param>
        /// <param name="tableName"></param>
        public override void CreateTable(DBExtend helper, List<FieldAttribute> fields, string tableName)
        {
            List<string> lines = new List<string>();
            string script = string.Format("create table {0}(\r\n", tableName);
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
                if (item.IsPrimaryKey)
                {
                    str = " " + item.Name + " INTEGER Not Null Primary Key";
                }
                list2.Add(str);
            }
            script += string.Join(",\r\n", list2.ToArray());
            script += ")";
            string sequenceName = string.Format("{0}_sequence", tableName);
            string triggerName = string.Format("{0}_trigge", tableName);
            string sequenceScript = string.Format("Create Sequence {0} MINVALUE 1  MAXVALUE 99999 INCREMENT BY 1 START WITH 1 NOCACHE CYCLE", sequenceName);
            string triggerScript = string.Format("\r\ncreate or replace trigger {0}\r\n  before insert on {1}   \r\n  for each row\r\ndeclare\r\n  nextid number;\r\nbegin\r\n  IF :new.{3} IS NULL or :new.{3}=0 THEN\r\n    select {2}.nextval \r\n    into nextid\r\n    from sys.dual;\r\n    :new.{3}:=nextid;\r\n  end if;\r\nend ;", new object[]
			{
				triggerName,
				tableName,
				sequenceName,
				primaryKey
			});
            lines.Add(sequenceScript);
            helper.SetParam("script", script);
            helper.Run("sp_ExecuteScript");
            foreach (string s in lines)
            {
                try
                {
                    helper.Execute(s, new Type[0]);
                }
                catch (Exception ero_19C)
                {
                }
            }
        }
        /// <summary>
        /// 批量插入,mysql不支持批量插入
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="helper"></param>
        /// <param name="details"></param>
        /// <param name="keepIdentity"></param>
        public override void BatchInsert<TItem>(DBHelper helper, List<TItem> details, bool keepIdentity = false)
        {
            foreach (TItem item in details)
            {
                helper.ClearParams();
                this.InsertObject(item, helper);
            }
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
            string sql = string.Format("insert into {0}(", table);
            string sql2 = "";
            string sql3 = "";
            string sequenceName = string.Format("{0}_sequence", table);
            string sqlGetIndex = string.Format("select {0}.nextval from dual", sequenceName);
            int id = Convert.ToInt32(helper.ExecScalar(sqlGetIndex));
            foreach (FieldAttribute info in typeArry)
            {
                string name = info.Name;
                if (info.IsPrimaryKey)
                {
                }
                if (string.IsNullOrEmpty(info.VirtualField))
                {
                    object value = info.GetValue(obj);
                    value = ObjectConvert.SetNullValue(value, info.PropertyType);
                    sql2 += string.Format("{0},", info.KeyWordName);
                    sql3 += string.Format("@{0},", info.KeyWordName);
                    helper.AddParam(info.KeyWordName, value);
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
				")"
			});
            sql = this.SqlFormat(sql);
            FieldAttribute primaryKey = TypeCache.GetTable(obj.GetType()).PrimaryKey;
            helper.SetParam(primaryKey.Name, id);
            helper.Execute(sql);
            return id;
        }
        /// <summary>
        /// 获取 with(nolock)
        /// </summary>
        /// <returns></returns>
        public override string GetWithNolockFormat()
        {
            return "";
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
            if (!query.ToLower().Contains("where"))
            {
                query += " where 1=1 ";
            }
            return string.Format("select {0} {1} {2} {3}", new object[]
			{
				fields,
				query,
				(top == 0) ? "" : (" and ROWNUM<=" + top),
				sort
			});
        }
        public override string GetAllTablesSql(DBHelper helper)
        {
            return "SELECT lower(table_name),1 FROM user_TABLES";
        }
        public override string GetAllSPSql(DBHelper helper)
        {
            return "select object_name,1 from user_objects where object_type='PROCEDURE'";
        }
        public override string SpParameFormat(string name, string type, bool output)
        {
            string str;
            if (!output)
            {
                str = "{0} in {1},";
            }
            else
            {
                str = "{0} out {1},";
            }
            return string.Format(str, name, type);
        }
        public override string KeyWordFormat(string value)
        {
            string keyWords = " ACCESS  ADD  ALL  ALTER  AND  ANY  AS  ASC  AUDIT  BETWEEN  BY  CHAR CHECK  CLUSTER  COLUMN  COMMENT  COMPRESS  CONNECT  CREATE  CURRENT DATE  DECIMAL  DEFAULT  DELETE  DESC  DISTINCT  DROP  ELSE  EXCLUSIVE EXISTS  FILE  FLOAT FOR  FROM  GRANT  GROUP  HAVING  IDENTIFIED IMMEDIATE  IN  INCREMENT  INDEX  INITIAL  INSERT  INTEGER  INTERSECT INTO  IS  LEVEL  LIKE  LOCK  LONG  MAXEXTENTS  MINUS  MLSLABEL  MODE MODIFY  NOAUDIT  NOCOMPRESS  NOT  NOWAIT  NULL  NUMBER  OF  OFFLINE ON  ONLINE  OPTION  OR  ORDER P CTFREE PRIOR PRIVILEGES PUBLIC RAW RENAME RESOURCE REVOKE ROW ROWID ROWNUM ROWS SELECT SESSION SET SHARE SIZE SMALLINT START SUCCESSFUL SYNONYM SYSDATE TABLE THEN TO TRIGGER UID UNION UNIQUE UPDATE USER VALIDATE VALUES VARCHAR VARCHAR2 VIEW WHENEVER WHERE WITH ";
            string result;
            if (keyWords.Contains(" " + value.ToUpper() + " "))
            {
                result = value + "_";
            }
            else
            {
                result = value;
            }
            return result;
        }
        public override string SqlFormat(string sql)
        {
            return Regex.Replace(sql, "@(\\w+)", ":$1");
        }
       
        public override string SubstringFormat(string field, int index, int length)
        {
            throw new NotImplementedException();
        }
        public override string StringLikeFormat(string field, string parName)
        {
            throw new NotImplementedException();
        }
        public override string StringNotLikeFormat(string field, string parName)
        {
            throw new NotImplementedException();
        }
        public override string StringContainsFormat(string field, string parName)
        {
            throw new NotImplementedException();
        }
        public override string BetweenFormat(string field, string parName, string parName2)
        {
            throw new NotImplementedException();
        }
        public override string DateDiffFormat(string field, string format, string parName)
        {
            throw new NotImplementedException();
        }
        public override string InFormat(string field, string parName)
        {
            throw new NotImplementedException();
        }
        public override string NotInFormat(string field, string parName)
        {
            throw new NotImplementedException();
        }
    }
}
