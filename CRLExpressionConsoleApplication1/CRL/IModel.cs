using CoreHelper;
using CRL.Attribute;
using CRL.DBAdapter;
using System;
using System.Collections.Generic;
namespace CRL
{
    /// <summary>
    /// 基类,不包含任何字段
    /// 如果有自定义主键名对象,请继承此类型
    /// </summary>
    [Serializable]
    public abstract class IModel : ICloneable
    {
        private Dictionary<string, object> Datas = new Dictionary<string, object>();
        /// <summary>
        /// 存放原始克隆
        /// </summary>
        private object _originClone = null;
        private bool boundChange = true;
        private ParameCollection changes = new ParameCollection();
        private string modelKey = null;
        /// <summary>
        /// 获取关联查询的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Field(MappingField = false)]
        public object this[string key]
        {
            get
            {
                object obj = null;
                if (!this.Datas.TryGetValue(key.ToLower(), out obj))
                {
                    throw new Exception(string.Format("对象:{0}不存在索引值:{1}", base.GetType(), key));
                }
                return obj;
            }
            set
            {
                this.Datas[key.ToLower()] = value;
            }
        }
        [Field(MappingField = false)]
        internal object OriginClone
        {
            get
            {
                return this._originClone;
            }
            set
            {
                this._originClone = value;
            }
        }
        [Field(MappingField = false)]
        internal bool BoundChange
        {
            get
            {
                return this.boundChange;
            }
            set
            {
                this.boundChange = value;
            }
        }
        [Field(MappingField = false)]
        internal ParameCollection Changes
        {
            get
            {
                return this.changes;
            }
            set
            {
                this.changes = value;
            }
        }
        /// <summary>
        /// 数据校验方法
        /// </summary>
        /// <returns></returns>
        public virtual string CheckData()
        {
            return "";
        }
        /// <summary>
        /// 检查索引
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public void CheckIndexExists(DBExtend helper)
        {
            List<string> list = this.GetIndexScript(helper);
            foreach (string item in list)
            {
                try
                {
                    helper.Execute(item, new Type[0]);
                }
                catch (Exception ero)
                {
                    EventLog.Log(string.Format("创建索引失败:{0}\r\n{1}", ero.Message, item));
                }
            }
        }
        internal static string CreateColumn(DBExtend helper, FieldAttribute item)
        {
            DBAdapterBase dbAdapter = helper._DBAdapter;
            string result = "";
            if (string.IsNullOrEmpty(item.ColumnType))
            {
                throw new Exception("ColumnType is null");
            }
            string str = dbAdapter.GetCreateColumnScript(item);
            string indexScript = "";
            if (item.FieldIndexType != FieldIndexType.无)
            {
                indexScript = dbAdapter.GetColumnIndexScript(item);
            }
            try
            {
                helper.Execute(str, new Type[0]);
                if (!string.IsNullOrEmpty(indexScript))
                {
                    helper.Execute(indexScript, new Type[0]);
                }
                result += string.Format("创建字段:{0}\r\n", item.Name);
            }
            catch (Exception ero)
            {
                result += string.Format("创建字段:{0} 发生错误:{1}\r\n", item.Name, ero.Message);
            }
            return result;
        }
        /// <summary>
        /// 检查对应的字段是否存在,不存在则创建
        /// </summary>
        /// <param name="helper"></param>
        public string CheckColumnExists(DBExtend helper)
        {
            string result = "";
            DBAdapterBase dbAdapter = helper._DBAdapter;
            List<FieldAttribute> columns = this.GetColumns(dbAdapter);
            string tableName = TypeCache.GetTableName(base.GetType());
            foreach (FieldAttribute item in columns)
            {
                string sql = dbAdapter.GetSelectTop(item.KeyWordName, "from " + tableName, "", 1);
                try
                {
                    helper.Execute(sql, new Type[0]);
                }
                catch
                {
                    result += IModel.CreateColumn(helper, item);
                }
            }
            return result;
        }
        internal static void SetColumnDbType(DBAdapterBase dbAdapter, FieldAttribute info, Dictionary<Type, string> dic)
        {
            if (info.FieldType == FieldType.数据库字段)
            {
                Type propertyType = info.PropertyType;
                if (propertyType.FullName.IndexOf("System.") > -1 && !dic.ContainsKey(propertyType))
                {
                    throw new Exception(string.Format("找不到对应的字段类型映射 {0} 在 {1}", propertyType, dbAdapter));
                }
                string defaultValue;
                string columnType = dbAdapter.GetColumnType(info, out defaultValue);
                info.ColumnType = columnType;
                info.DefaultValue = defaultValue;
                if (info.ColumnType.Contains("{0}"))
                {
                    throw new Exception(string.Format("属性:{0} 需要指定长度 ColumnType:{1}", info.Name, info.ColumnType));
                }
            }
        }
        /// <summary>
        /// 获取列
        /// </summary>
        /// <returns></returns>
        private List<FieldAttribute> GetColumns(DBAdapterBase dbAdapter)
        {
            Dictionary<Type, string> dic = dbAdapter.GetFieldMapping();
            Type type = base.GetType();
            string tableName = TypeCache.GetTableName(type);
            List<FieldAttribute> typeArry = TypeCache.GetProperties(type, true);
            List<FieldAttribute> columns = new List<FieldAttribute>();
            for (int i = typeArry.Count - 1; i >= 0; i--)
            {
                FieldAttribute info = typeArry[i];
                if (info.FieldType != FieldType.虚拟字段)
                {
                    IModel.SetColumnDbType(dbAdapter, info, dic);
                    columns.Add(info);
                }
            }
            return columns;
        }
        internal List<string> GetIndexScript(DBExtend helper)
        {
            DBAdapterBase dbAdapter = helper._DBAdapter;
            List<string> list2 = new List<string>();
            List<FieldAttribute> columns = this.GetColumns(dbAdapter);
            foreach (FieldAttribute item in columns)
            {
                if (item.FieldIndexType != FieldIndexType.无)
                {
                    string indexScript = dbAdapter.GetColumnIndexScript(item);
                    list2.Add(indexScript);
                }
            }
            return list2;
        }
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public string CreateTable(DBExtend helper)
        {
            string msg;
            this.CreateTable(helper, out msg);
            return msg;
        }
        /// <summary>
        /// 创建表
        /// 会检查表是否存在,如果存在则检查字段
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool CreateTable(DBExtend helper, out string message)
        {
            DBAdapterBase dbAdapter = helper._DBAdapter;
            message = "";
            TypeCache.SetDBAdapterCache(base.GetType(), dbAdapter);
            string tableName = TypeCache.GetTableName(base.GetType());
            string sql = dbAdapter.GetSelectTop("0", "from " + tableName, "", 1);
            bool needCreate = false;
            try
            {
                helper.Execute(sql, new Type[0]);
            }
            catch
            {
                needCreate = true;
            }
            bool result;
            if (needCreate)
            {
                List<string> list = new List<string>();
                try
                {
                    List<FieldAttribute> columns = this.GetColumns(dbAdapter);
                    dbAdapter.CreateTable(helper, columns, tableName);
                    message = string.Format("创建表:{0}\r\n", TypeCache.GetTableName(base.GetType()));
                    this.CheckIndexExists(helper);
                    result = true;
                    return result;
                }
                catch (Exception ero)
                {
                    message = "创建表时发生错误 类型{0} {1}\r\n";
                    message = string.Format(message, base.GetType(), ero.Message);
                    throw new Exception(message);
                }
            }
            message = this.CheckColumnExists(helper);
            result = true;
            return result;
        }
        /// <summary>
        /// 表示值被更改了
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected internal void SetChanges(string name, object value)
        {
            if (this.BoundChange)
            {
                if (!(name.ToLower() == "boundchange"))
                {
                    this.Changes[name] = value;
                }
            }
        }
        /// <summary>
        /// 创建当前对象的浅表副本
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return base.MemberwiseClone();
        }
        internal string GetModelKey()
        {
            if (this.modelKey == null)
            {
                Type type = base.GetType();
                TableAttribute tab = TypeCache.GetTable(type);
                this.modelKey = string.Format("{0}_{1}", type, tab.PrimaryKey.GetValue(this));
            }
            return this.modelKey;
        }
    }
}
