using CRL.DBAdapter;
using System;
using System.Collections.Generic;


namespace CRL.Attribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : System.Attribute
    {
        /// <summary>
        /// 对象类型
        /// </summary>
        public Type Type;
        private DBAdapterBase _DBAdapter;
        /// <summary>
        /// 所有字段
        /// </summary>
        internal List<FieldAttribute> Fields = new List<FieldAttribute>();
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName
        {
            get;
            set;
        }
        /// <summary>
        /// 默认排序
        /// </summary>
        public string DefaultSort
        {
            get;
            set;
        }
        /// <summary>
        /// 自增主键
        /// </summary>
        internal FieldAttribute PrimaryKey
        {
            get;
            set;
        }
        /// <summary>
        /// 当前数据库适配器
        /// </summary>
        internal DBAdapterBase DBAdapter
        {
            get
            {
                if (this._DBAdapter == null)
                {
                }
                return this._DBAdapter;
            }
            set
            {
                this._DBAdapter = value;
            }
        }
        public override string ToString()
        {
            return this.TableName;
        }
    }
}
