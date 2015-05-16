using CoreHelper;
using CRL.Attribute;
using System;
using System.Collections.Generic;
namespace CRL.DBAdapter
{
    internal abstract class DBAdapterBase
    {
        public abstract DBType DBType
        {
            get;
        }
        /// <summary>
        /// GROUP分页模版
        /// </summary>
        public abstract string TemplateGroupPage
        {
            get;
        }
        /// <summary>
        /// 查询分页模版
        /// </summary>
        public abstract string TemplatePage
        {
            get;
        }
        /// <summary>
        /// 存储过程模版
        /// </summary>
        public abstract string TemplateSp
        {
            get;
        }
        /// <summary>
        /// 根据数据库类型获取适配器
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static DBAdapterBase GetDBAdapterBase(DBType dbType)
        {
            DBAdapterBase db = null;
            switch (dbType)
            {
                case DBType.MSSQL:
                    db = new MSSQLDBAdapter();
                    break;
                
                case DBType.ORACLE:
                    db = new ORACLEDBAdapter();
                    break;
            }
            if (db == null)
            {
                throw new Exception("找不到对应的DBAdapte" + dbType);
            }
            return db;
        }
        /// <summary>
        ///             获取列类型和默认值
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public abstract string GetColumnType(FieldAttribute info, out string defaultValue);
        /// <summary>
        /// 获取字段类型转换
        /// </summary>
        /// <returns></returns>
        public abstract Dictionary<Type, string> GetFieldMapping();
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="filed"></param>
        /// <returns></returns>
        public abstract string GetColumnIndexScript(FieldAttribute filed);
        /// <summary>
        /// 增加列
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public abstract string GetCreateColumnScript(FieldAttribute field);
        /// <summary>
        /// 创建存储过程
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="script"></param>
        /// <returns></returns>
        public abstract string GetCreateSpScript(string spName, string script);
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="fields"></param>
        /// <param name="tableName"></param>
        public abstract void CreateTable(DBExtend helper, List<FieldAttribute> fields, string tableName);
        /// <summary>
        /// 批量插入方法
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="helper"></param>
        /// <param name="details"></param>
        /// <param name="keepIdentity">否保持自增主键</param>
        public abstract void BatchInsert<TItem>(DBHelper helper, List<TItem> details, bool keepIdentity = false) where TItem : IModel, new();
        /// <summary>
        /// 获取UPDATE语法
        /// </summary>
        /// <param name="table"></param>
        /// <param name="setString"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual string GetUpdateSql(string table, string setString, string where)
        {
            return string.Format("update {0} set {1} where {2}", table, setString, where);
        }
        /// <summary>
        /// 获取删除语法
        /// </summary>
        /// <param name="table"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual string GetDeleteSql(string table, string where)
        {
            return string.Format("delete from {0}  where {1}", table, where);
        }
        /// <summary>
        /// 插入对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="helper"></param>
        /// <returns></returns>
        public abstract int InsertObject(IModel obj, DBHelper helper);
        /// <summary>
        /// 获取查询前几条
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="query"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public abstract string GetSelectTop(string fields, string query, string sort, int top);
        /// <summary>
        /// 获取with nolock语法
        /// </summary>
        /// <returns></returns>
        public abstract string GetWithNolockFormat();
        /// <summary>
        /// 获取所有存储过程
        /// </summary>
        /// <returns></returns>
        public abstract string GetAllSPSql(DBHelper helper);
        /// <summary>
        /// 获取所有表,查询需要转为小写
        /// </summary>
        /// <returns></returns>
        public abstract string GetAllTablesSql(DBHelper helper);
        /// <summary>
        /// 存储过程参数格式货
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public abstract string SpParameFormat(string name, string type, bool output);
        /// <summary>
        /// 关键字格式化,可能会增加后辍
        /// </summary>
        public abstract string KeyWordFormat(string value);
        /// <summary>
        /// 语句自定义格式化处理
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public abstract string SqlFormat(string sql);

        public abstract string SubstringFormat(string field, int index, int length);
        public abstract string StringLikeFormat(string field, string parName);
        public abstract string StringNotLikeFormat(string field, string parName);
        public abstract string StringContainsFormat(string field, string parName);
        public abstract string BetweenFormat(string field, string parName, string parName2);
        public abstract string DateDiffFormat(string field, string format, string parName);
        public abstract string InFormat(string field, string parName);
        public abstract string NotInFormat(string field, string parName);
    }
}
